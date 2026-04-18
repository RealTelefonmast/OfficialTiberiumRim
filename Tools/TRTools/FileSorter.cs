using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TRTools;

/// <summary>
/// Analyses .cs files via Roslyn syntax trees, resolves each file's primary type's
/// inheritance chain against a table of known RimWorld/Verse category roots, then
/// copies files into a sorted dump folder — similar to --flatten-textures but for code.
/// </summary>
internal static class FileSorter
{
    // Maps well-known external (DLL) base type names → destination folder.
    // Traversal ascends the inheritance chain; the first (most specific) match wins.
    // Add entries here to recognise new base types from mods or the engine.
    private static readonly Dictionary<string, string> CategoryRoots =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // ── Verse graphics ───────────────────────────────────────────────
            ["Graphic"]              = "Graphics",

            // ── Verse things (specific before general so traversal stops early)
            ["Building"]             = "Buildings",
            ["Plant"]                = "Plants",
            ["Pawn"]                 = "Pawns",
            ["Mote"]                 = "Rendering",
            ["Projectile"]           = "Projectiles",
            ["Thing"]                = "Things",

            // ── Comps ────────────────────────────────────────────────────────
            ["ThingComp"]            = "Comps",
            ["CompProperties"]       = "CompProperties",

            // ── Defs ─────────────────────────────────────────────────────────
            ["Def"]                  = "Defs",

            // ── World / game components ──────────────────────────────────────
            ["GameComponent"]        = "GameComponents",
            ["MapComponent"]         = "MapComponents",
            ["WorldComponent"]       = "WorldComponents",
            ["WorldObject"]          = "World",

            // ── AI ───────────────────────────────────────────────────────────
            ["JobDriver"]            = "AI",
            ["JobGiver"]             = "AI",
            ["WorkGiver"]            = "AI",

            // ── Hediffs ──────────────────────────────────────────────────────
            ["Hediff"]               = "Hediffs",
            ["HediffComp"]           = "Hediffs",

            // ── UI ───────────────────────────────────────────────────────────
            ["ITab"]                 = "UI",
            ["Window"]               = "UI",
            ["Dialog"]               = "UI",
            ["Page"]                 = "UI",
            ["Gizmo"]                = "UI",
            ["Command"]              = "UI",

            // ── Workers / misc ───────────────────────────────────────────────
            ["IncidentWorker"]       = "Incidents",
            ["Designator"]           = "Designators",
            ["Need"]                 = "Needs",
            ["Alert"]                = "Alerts",
            ["PlaceWorker"]          = "PlaceWorkers",
            ["StatWorker"]           = "Stats",
            ["StatPart"]             = "Stats",
            ["WeatherEvent"]         = "Weather",
            ["GameCondition"]        = "Weather",
            ["WeatherWorker"]        = "Weather",
        };

    private static readonly HashSet<string> SkippedDirs =
        new(StringComparer.OrdinalIgnoreCase) { ".git", "bin", "obj", ".vs", ".idea" };

    private readonly record struct TypeEntry(
        string              Name,
        string              FilePath,
        IReadOnlyList<string> BaseNames);

    // ── Public entry point ───────────────────────────────────────────────────

    public static void Run(
        string                   rootPath,
        string                   scopePath,
        IReadOnlySet<string>     excludedAbsPaths,
        string                   destDir,
        bool                     confirm,
        Action<ConsoleColor, string> colored,
        Action<string>           write,
        Action<string>           printSection)
    {
        // Collect .cs files respecting scope, excludes and skipped dirs
        var csFiles = Directory
            .EnumerateFiles(scopePath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !IsInSkippedDir(scopePath, f))
            .Where(f => !IsInExcludedPath(f, excludedAbsPaths))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        printSection("Sort Files" + (confirm ? "" : " [DRY RUN]"));
        write("Scope:  " + Path.GetRelativePath(rootPath, scopePath));
        write("Dest:   " + Path.GetRelativePath(rootPath, destDir));
        write("Files:  " + csFiles.Count);
        write("");

        // Parse all files and build a type-name → base-names graph
        var allEntries = new List<TypeEntry>(csFiles.Count * 2);
        foreach (string f in csFiles)
            allEntries.AddRange(ParseTypeEntries(f));

        var typeGraph = BuildTypeGraph(allEntries);

        var entriesByFile = allEntries
            .GroupBy(e => e.FilePath, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        // Build the copy plan
        //   usedInCategory tracks filenames already assigned per category folder,
        //   so duplicates get _2, _3, ... suffixes.
        var usedInCategory = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var plan           = new List<(string src, string dest, string category, bool renamed)>();
        var unclassified   = new List<string>();

        foreach (string file in csFiles)
        {
            string? primary  = PrimaryTypeName(file, entriesByFile);
            string? category = primary != null
                ? ResolveCategory(primary, typeGraph, new HashSet<string>(StringComparer.OrdinalIgnoreCase))
                : null;

            if (category == null)
            {
                unclassified.Add(file);
                continue;
            }

            if (!usedInCategory.TryGetValue(category, out var usedNames))
                usedInCategory[category] = usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string origName = Path.GetFileName(file);
            string stem     = Path.GetFileNameWithoutExtension(origName);
            string ext      = Path.GetExtension(origName);
            string destName = origName;
            int    dupIdx   = 2;

            while (!usedNames.Add(destName))
            {
                destName = stem + "_" + dupIdx + ext;
                dupIdx++;
            }

            bool renamed = !string.Equals(origName, destName, StringComparison.OrdinalIgnoreCase);
            plan.Add((file, Path.Combine(destDir, category, destName), category, renamed));
        }

        // Report summary
        write("Classified:   " + plan.Count);
        write("Unclassified: " + unclassified.Count);
        write("");

        if (!confirm)
            colored(ConsoleColor.DarkGray, "Dry run — add --confirm to copy files.");

        write("");

        // Output grouped by category
        foreach (var group in plan
            .GroupBy(m => m.category, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
        {
            colored(ConsoleColor.Cyan, "[" + group.Key + "]  (" + group.Count() + ")");

            foreach (var (src, dest, _, renamed) in group
                .OrderBy(m => Path.GetFileName(m.src), StringComparer.OrdinalIgnoreCase))
            {
                if (renamed)
                    colored(ConsoleColor.Yellow,
                        "  " + Path.GetRelativePath(rootPath, src)
                        + "  →  " + Path.GetFileName(dest));
                else
                    write("  " + Path.GetRelativePath(rootPath, src));

                if (confirm)
                    CopyFile(src, dest, colored, rootPath);
            }
            write("");
        }

        // Unclassified bucket
        if (unclassified.Count > 0)
        {
            colored(ConsoleColor.DarkGray, "[Unsorted]  (" + unclassified.Count + ")");
            foreach (string f in unclassified.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                write("  " + Path.GetRelativePath(rootPath, f));
                if (confirm)
                    CopyFile(f, Path.Combine(destDir, "Unsorted", Path.GetFileName(f)), colored, rootPath);
            }
            write("");
        }

        if (confirm)
            write("Done. Copied " + (plan.Count + unclassified.Count) + " file(s) to "
                + Path.GetRelativePath(rootPath, destDir));
    }

    // ── Type graph ───────────────────────────────────────────────────────────

    // Build type name → direct base names dict from all parsed entries.
    // When a name appears in multiple files (duplicates), the first wins for
    // graph purposes — good enough for category resolution.
    private static Dictionary<string, IReadOnlyList<string>> BuildTypeGraph(
        IEnumerable<TypeEntry> entries)
    {
        var graph = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in entries)
            graph.TryAdd(e.Name, e.BaseNames);
        return graph;
    }

    // ── Roslyn parsing ───────────────────────────────────────────────────────

    private static List<TypeEntry> ParseTypeEntries(string filePath)
    {
        var result = new List<TypeEntry>();
        try
        {
            string text = File.ReadAllText(filePath);
            var    tree = CSharpSyntaxTree.ParseText(text);
            var    root = tree.GetCompilationUnitRoot();

            foreach (var decl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                string name  = StripGenericsAndQualifiers(decl.Identifier.Text);
                var    bases = (decl.BaseList?.Types
                    .Select(b => StripGenericsAndQualifiers(b.Type.ToString()))
                    .Where(b => b.Length > 0)
                    .ToList()) ?? new List<string>();

                result.Add(new TypeEntry(name, filePath, bases));
            }
        }
        catch
        {
            // unparseable file — skip silently
        }
        return result;
    }

    // ── Category resolution ──────────────────────────────────────────────────

    // Returns the name of the primary type for a file:
    //   1. Type whose name matches the file stem exactly (case-insensitive)
    //   2. First type declared in the file
    //   3. null if the file has no type declarations
    private static string? PrimaryTypeName(
        string filePath,
        Dictionary<string, List<TypeEntry>> entriesByFile)
    {
        if (!entriesByFile.TryGetValue(filePath, out var entries) || entries.Count == 0)
            return null;

        string stem = Path.GetFileNameWithoutExtension(filePath);
        foreach (var e in entries)
            if (string.Equals(e.Name, stem, StringComparison.OrdinalIgnoreCase))
                return e.Name;

        return entries[0].Name;
    }

    // Walks up the inheritance chain, returning the category folder of the
    // first ancestor found in CategoryRoots.  visited prevents infinite loops.
    private static string? ResolveCategory(
        string                               typeName,
        Dictionary<string, IReadOnlyList<string>> graph,
        HashSet<string>                      visited)
    {
        if (!visited.Add(typeName)) return null;  // cycle guard

        // Direct hit
        if (CategoryRoots.TryGetValue(typeName, out string? cat))
            return cat;

        // Not a known external root — look it up in the codebase graph
        if (!graph.TryGetValue(typeName, out var bases))
            return null;

        foreach (string baseName in bases)
        {
            // Check the base directly first (avoids one level of recursion in the common case)
            if (CategoryRoots.TryGetValue(baseName, out string? baseCat))
                return baseCat;

            string? resolved = ResolveCategory(baseName, graph, visited);
            if (resolved != null)
                return resolved;
        }

        return null;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static void CopyFile(
        string src, string dest,
        Action<ConsoleColor, string> colored,
        string rootPath)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(src, dest, overwrite: true);
        }
        catch (Exception ex)
        {
            colored(ConsoleColor.Red,
                "  FAILED " + Path.GetRelativePath(rootPath, src) + " — " + ex.Message);
        }
    }

    // "System.Collections.Generic.List<T>" → "List"
    // "Bar<T,U>" → "Bar",  "Foo.Bar" → "Bar"
    private static string StripGenericsAndQualifiers(string name)
    {
        int lt = name.IndexOf('<');
        if (lt >= 0) name = name[..lt];

        int dot = name.LastIndexOf('.');
        if (dot >= 0) name = name[(dot + 1)..];

        return name.Trim();
    }

    private static bool IsInExcludedPath(string filePath, IReadOnlySet<string> excludedAbsPaths)
    {
        foreach (string excl in excludedAbsPaths)
            if (filePath.StartsWith(excl, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    private static bool IsInSkippedDir(string root, string filePath)
    {
        string rel = Path.GetRelativePath(root, filePath);
        foreach (string part in rel.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            if (SkippedDirs.Contains(part)) return true;
        return false;
    }
}
