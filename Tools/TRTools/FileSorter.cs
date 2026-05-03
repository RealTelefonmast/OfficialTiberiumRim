using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TRTools;

/// <summary>
/// Analyses .cs files via Roslyn syntax trees, resolves each file's primary type's
/// inheritance chain against a table of known RimWorld/Verse category roots, then
/// copies files into a sorted dump folder — similar to --flatten-textures but for code.
/// <para>
/// Category roots are loaded from <c>sort-categories.json</c>, looked up first next to
/// the executable (<c>AppContext.BaseDirectory</c>) and then in the working directory.
/// If no file is found, built-in defaults are used.  Edit <c>sort-categories.json</c>
/// freely — no recompile needed.
/// </para>
/// </summary>
internal static class FileSorter
{
    private const string ConfigFileName = "sort-categories.json";

    // Built-in fallback: used when no sort-categories.json is found on disk.
    // Maps well-known external (DLL) base type names → destination folder.
    // Traversal ascends the inheritance chain; the first (most specific) match wins.
    private static readonly Dictionary<string, string> DefaultCategoryRoots =
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

    /// <summary>
    /// Attempts to load <c>sort-categories.json</c> from the exe directory or CWD.
    /// Returns the parsed dict on success, or <see cref="DefaultCategoryRoots"/> on failure.
    /// </summary>
    private static (Dictionary<string, string> roots, string? sourceFile) LoadCategoryRoots()
    {
        // Search order: exe dir first (for published/standalone), then CWD (dotnet run)
        string[] candidates =
        [
            Path.Combine(AppContext.BaseDirectory, ConfigFileName),
            Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName),
        ];

        foreach (string path in candidates)
        {
            if (!File.Exists(path)) continue;
            try
            {
                string json = File.ReadAllText(path);
                var opts = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip };
                var raw  = JsonSerializer.Deserialize<Dictionary<string, string>>(json, opts);
                if (raw == null) continue;

                // Strip the _comment key if present, build case-insensitive dict
                var roots = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var (k, v) in raw)
                    if (!k.StartsWith("_"))
                        roots[k] = v;

                return (roots, path);
            }
            catch
            {
                // Malformed JSON — skip and try next candidate
            }
        }

        return (DefaultCategoryRoots, null);
    }

    private static readonly HashSet<string> SkippedDirs =
        new(StringComparer.OrdinalIgnoreCase) { ".git", "bin", "obj", ".vs", ".idea" };

    private enum TypeKind { Class, Interface, Struct, Record, Enum, Delegate }

    private readonly record struct TypeEntry(
        string                Name,
        string                FilePath,
        IReadOnlyList<string> BaseNames,
        TypeKind              Kind,
        bool                  IsStatic,   // class or struct with static modifier
        bool                  IsAbstract, // class with abstract modifier
        bool                  IsPatch);   // has [HarmonyPatch] attribute

    // ── Public entry points ──────────────────────────────────────────────────

    /// <summary>
    /// Scans .cs files, attempts to resolve each primary type's category, then groups
    /// unresolvable types by their terminal unknown ancestor.  Output is sorted by
    /// frequency so the highest-value entries to add to sort-categories.json appear first.
    /// </summary>
    public static void RunProbe(
        string                       rootPath,
        string                       scopePath,
        IReadOnlySet<string>         excludedAbsPaths,
        Action<ConsoleColor, string> colored,
        Action<string>               write,
        Action<string>               printSection)
    {
        var (categoryRoots, configFile) = LoadCategoryRoots();

        var csFiles = Directory
            .EnumerateFiles(scopePath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !IsInSkippedDir(scopePath, f))
            .Where(f => !IsInExcludedPath(f, excludedAbsPaths))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        printSection("Probe: Unresolvable Inheritance Roots");
        write("Scope:   " + Path.GetRelativePath(rootPath, scopePath));
        write("Config:  " + (configFile != null ? Path.GetRelativePath(rootPath, configFile) : "(built-in defaults — sort-categories.json not found)"));
        write("Files:   " + csFiles.Count);
        write("");

        // Parse and build graph
        var allEntries = new List<TypeEntry>(csFiles.Count * 2);
        foreach (string f in csFiles)
            allEntries.AddRange(ParseTypeEntries(f));

        var typeGraph      = BuildTypeGraph(allEntries);
        var entriesByFile  = allEntries
            .GroupBy(e => e.FilePath, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        // Classify each file
        var classifiedCount  = 0;
        var noDeclarations   = new List<string>();  // files with no type declarations

        // terminal unknown → list of (primaryTypeName, declaredBases, typesSubfolder, filePath)
        var unresolvable = new Dictionary<string, List<(string typeName, string bases, string typesSub, string file)>>(
            StringComparer.OrdinalIgnoreCase);

        foreach (string file in csFiles)
        {
            string? primary = PrimaryTypeName(file, entriesByFile);
            if (primary == null)
            {
                noDeclarations.Add(file);
                continue;
            }

            string? category = ResolveCategory(primary, typeGraph, categoryRoots,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase));

            if (category != null)
            {
                classifiedCount++;
                continue;
            }

            // Find terminal unknown — deepest base not in graph or categoryRoots
            string terminal = FindTerminalUnknown(primary, typeGraph, categoryRoots,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase))
                ?? primary;

            // Build display string of declared bases and determine Types subfolder
            string basesDisplay = "";
            string typesSub     = "";
            if (entriesByFile.TryGetValue(file, out var fileEntries))
            {
                var entry = fileEntries.FirstOrDefault(e =>
                    string.Equals(e.Name, primary, StringComparison.OrdinalIgnoreCase));
                if (entry.Name != null)
                {
                    if (entry.BaseNames.Count > 0)
                        basesDisplay = ": " + string.Join(", ", entry.BaseNames);
                    typesSub = GetUnsortedSubfolder(entry, typeGraph);
                }
            }

            if (!unresolvable.TryGetValue(terminal, out var bucket))
                unresolvable[terminal] = bucket = new List<(string, string, string, string)>();

            bucket.Add((primary, basesDisplay, typesSub, file));
        }

        // Summary line
        int totalUnresolvable = unresolvable.Values.Sum(b => b.Count);
        write($"Classified:    {classifiedCount}");
        write($"Unresolvable:  {totalUnresolvable}  across {unresolvable.Count} unknown root(s)");
        write($"No types:      {noDeclarations.Count}");
        write("");

        if (unresolvable.Count == 0)
        {
            colored(ConsoleColor.Green, "All files are classifiable — nothing to add to sort-categories.json.");
            return;
        }

        // Print groups, highest frequency first
        foreach (var (terminal, bucket) in unresolvable
            .OrderByDescending(kvp => kvp.Value.Count)
            .ThenBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
        {
            // Show the most common Types subfolder this group would land in
            string dominantSub = bucket
                .GroupBy(b => b.typesSub.Length > 0 ? "Types/" + b.typesSub : "Types")
                .OrderByDescending(g => g.Count())
                .First().Key;

            colored(ConsoleColor.Yellow,
                $"[{terminal}]  ({bucket.Count} file{(bucket.Count == 1 ? "" : "s")})  → {dominantSub}");

            int nameWidth = Math.Max(bucket.Max(b => b.typeName.Length), 20);

            foreach (var (typeName, bases, typesSub, file) in bucket
                .OrderBy(b => b.typeName, StringComparer.OrdinalIgnoreCase))
            {
                string rel    = Path.GetRelativePath(rootPath, file);
                string label  = typeName.PadRight(nameWidth);
                string dest   = typesSub.Length > 0 ? "Types/" + typesSub : "Types";
                string suffix = bases.Length > 0 ? $"  {bases}" : "";
                write($"  {label}{suffix}");
                colored(ConsoleColor.DarkGray, $"    {rel}  [{dest}]");
            }

            write("");
        }

        // No-declaration files → flat Types/
        if (noDeclarations.Count > 0)
        {
            colored(ConsoleColor.DarkGray, $"[(no type declarations)]  ({noDeclarations.Count} file{(noDeclarations.Count == 1 ? "" : "s")})  → Types");
            foreach (string f in noDeclarations.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                colored(ConsoleColor.DarkGray, "  " + Path.GetRelativePath(rootPath, f));
            write("");
        }
    }

    /// <summary>
    /// Walks up the inheritance chain and returns the deepest base type that is both:
    /// (a) not in <paramref name="categoryRoots"/>, and
    /// (b) not defined in the local type graph (i.e. it comes from an external DLL).
    /// This is the type the user should add to sort-categories.json to unblock categorization.
    /// Returns <c>null</c> if the chain resolves or contains only cycles.
    /// </summary>
    private static string? FindTerminalUnknown(
        string                                    typeName,
        Dictionary<string, IReadOnlyList<string>> graph,
        Dictionary<string, string>                categoryRoots,
        HashSet<string>                           visited)
    {
        if (!visited.Add(typeName)) return null;              // cycle guard
        if (categoryRoots.ContainsKey(typeName)) return null; // chain resolves here

        // Not in the local graph → it's an external type and the terminal unknown
        if (!graph.TryGetValue(typeName, out var bases) || bases.Count == 0)
            return typeName;

        // Walk bases; if any resolves the chain, this path is fine
        foreach (string baseName in bases)
        {
            if (categoryRoots.ContainsKey(baseName)) return null;

            string? deeper = FindTerminalUnknown(baseName, graph, categoryRoots, visited);
            if (deeper != null) return deeper;
        }

        // All bases are cycles or in-graph dead ends — this node is the effective terminal
        return typeName;
    }

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

        var (categoryRoots, configFile) = LoadCategoryRoots();

        printSection("Sort Files" + (confirm ? "" : " [DRY RUN]"));
        write("Scope:   " + Path.GetRelativePath(rootPath, scopePath));
        write("Dest:    " + Path.GetRelativePath(rootPath, destDir));
        write("Config:  " + (configFile != null ? Path.GetRelativePath(rootPath, configFile) : "(built-in defaults)"));
        write("Files:   " + csFiles.Count);
        write("");

        // Parse all files and build a type-name → base-names graph
        var allEntries = new List<TypeEntry>(csFiles.Count * 2);
        foreach (string f in csFiles)
            allEntries.AddRange(ParseTypeEntries(f));

        var typeGraph = BuildTypeGraph(allEntries);

        var entriesByFile = allEntries
            .GroupBy(e => e.FilePath, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        // Build the copy plan.
        //   usedInCategory tracks filenames already assigned per category folder,
        //   so duplicates get _2, _3, ... suffixes.
        //   Unclassified files land in Types/ with subfolders determined by declaration shape.
        var usedInCategory = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var plan           = new List<(string src, string dest, string category, bool renamed)>();

        foreach (string file in csFiles)
        {
            string? primary  = PrimaryTypeName(file, entriesByFile);
            string? category = primary != null
                ? ResolveCategory(primary, typeGraph, categoryRoots, new HashSet<string>(StringComparer.OrdinalIgnoreCase))
                : null;

            if (category == null)
            {
                // Classify into Types/* subfolder by declaration shape
                string sub = "";
                if (primary != null && entriesByFile.TryGetValue(file, out var fe))
                {
                    var entry = fe.FirstOrDefault(e =>
                        string.Equals(e.Name, primary, StringComparison.OrdinalIgnoreCase));
                    if (entry.Name != null)
                        sub = GetUnsortedSubfolder(entry, typeGraph);
                }
                category = sub.Length > 0 ? "Types/" + sub : "Types";
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

        // Count Types/* vs classified
        int typesCount      = plan.Count(p => p.category.StartsWith("Types", StringComparison.OrdinalIgnoreCase));
        int classifiedCount = plan.Count - typesCount;

        // Build dynamic breakdown of all occupied Types/* subfolders, canonical order
        string[] subOrder = ["Types", "Types/Enums", "Types/Delegates", "Types/Interfaces",
                             "Types/Structs", "Types/Records", "Types/Patches", "Types/Utils",
                             "Types/Abstracts", "Types/Entities", "Types/Exposables"];
        var typesCounts = plan
            .Where(p => p.category.StartsWith("Types", StringComparison.OrdinalIgnoreCase))
            .GroupBy(p => p.category, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
        string breakdown = string.Join("  ", subOrder
            .Where(s => typesCounts.ContainsKey(s))
            .Select(s => s.Replace("Types/", "") + ": " + typesCounts[s]));

        write("Classified:   " + classifiedCount);
        write("Types bucket: " + typesCount + "  (" + breakdown + ")");
        write("");

        if (!confirm)
            colored(ConsoleColor.DarkGray, "Dry run — add --confirm to copy files.");

        write("");

        // Output grouped by category — classified first (alphabetical), then Types/*
        var groups = plan
            .GroupBy(m => m.category, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key.StartsWith("Types", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
            .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var group in groups)
        {
            bool isTypes = group.Key.StartsWith("Types", StringComparison.OrdinalIgnoreCase);
            var  label   = "[" + group.Key + "]  (" + group.Count() + ")";
            if (isTypes)
                colored(ConsoleColor.DarkGray, label);
            else
                colored(ConsoleColor.Cyan, label);

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

        if (confirm)
            write("Done. Copied " + plan.Count + " file(s) to "
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

            // ── TypeDeclarationSyntax: class / interface / struct / record ────
            foreach (var decl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                string name  = StripGenericsAndQualifiers(decl.Identifier.Text);
                var    bases = (decl.BaseList?.Types
                    .Select(b => StripGenericsAndQualifiers(b.Type.ToString()))
                    .Where(b => b.Length > 0)
                    .ToList()) ?? new List<string>();

                TypeKind kind = decl switch
                {
                    InterfaceDeclarationSyntax => TypeKind.Interface,
                    StructDeclarationSyntax    => TypeKind.Struct,
                    RecordDeclarationSyntax r  => r.ClassOrStructKeyword.RawKind == (int)SyntaxKind.StructKeyword
                                                  ? TypeKind.Struct : TypeKind.Record,
                    _                          => TypeKind.Class,
                };

                bool isStatic   = kind != TypeKind.Interface
                    && decl.Modifiers.Any(m => m.RawKind == (int)SyntaxKind.StaticKeyword);
                bool isAbstract = kind == TypeKind.Class
                    && decl.Modifiers.Any(m => m.RawKind == (int)SyntaxKind.AbstractKeyword);
                bool isPatch    =
                    // class name convention: ends with Patch or Patches
                    name.EndsWith("Patch",   StringComparison.OrdinalIgnoreCase) ||
                    name.EndsWith("Patches", StringComparison.OrdinalIgnoreCase) ||
                    // [HarmonyPatch] on the class itself or on any member inside it
                    decl.DescendantNodes().OfType<AttributeSyntax>()
                        .Any(a => a.Name.ToString()
                                   .Replace("Attribute", "")
                                   .EndsWith("HarmonyPatch", StringComparison.OrdinalIgnoreCase));

                result.Add(new TypeEntry(name, filePath, bases, kind, isStatic, isAbstract, isPatch));
            }

            // ── EnumDeclarationSyntax ─────────────────────────────────────────
            foreach (var decl in root.DescendantNodes().OfType<EnumDeclarationSyntax>())
            {
                string name = decl.Identifier.Text;
                result.Add(new TypeEntry(name, filePath, Array.Empty<string>(),
                    TypeKind.Enum, IsStatic: false, IsAbstract: false, IsPatch: false));
            }

            // ── DelegateDeclarationSyntax ─────────────────────────────────────
            foreach (var decl in root.DescendantNodes().OfType<DelegateDeclarationSyntax>())
            {
                string name = decl.Identifier.Text;
                result.Add(new TypeEntry(name, filePath, Array.Empty<string>(),
                    TypeKind.Delegate, IsStatic: false, IsAbstract: false, IsPatch: false));
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
    // first ancestor found in categoryRoots.  visited prevents infinite loops.
    private static string? ResolveCategory(
        string                                    typeName,
        Dictionary<string, IReadOnlyList<string>> graph,
        Dictionary<string, string>                categoryRoots,
        HashSet<string>                           visited)
    {
        if (!visited.Add(typeName)) return null;  // cycle guard

        // Direct hit
        if (categoryRoots.TryGetValue(typeName, out string? cat))
            return cat;

        // Not a known external root — look it up in the codebase graph
        if (!graph.TryGetValue(typeName, out var bases))
            return null;

        foreach (string baseName in bases)
        {
            // Check the base directly first (avoids one level of recursion in the common case)
            if (categoryRoots.TryGetValue(baseName, out string? baseCat))
                return baseCat;

            string? resolved = ResolveCategory(baseName, graph, categoryRoots, visited);
            if (resolved != null)
                return resolved;
        }

        return null;
    }

    // ── Types subfolder classification ───────────────────────────────────────

    // Subfolder names within the Types/ bucket for unclassified files.
    private const string SubEnums      = "Enums";
    private const string SubDelegates  = "Delegates";
    private const string SubInterfaces = "Interfaces";
    private const string SubStructs    = "Structs";
    private const string SubRecords    = "Records";
    private const string SubPatches    = "Patches";
    private const string SubUtils      = "Utils";
    private const string SubAbstracts  = "Abstracts";
    private const string SubEntities   = "Entities";
    private const string SubExposables = "Exposables";

    /// <summary>
    /// Returns the Types/ subfolder for a file that couldn't be classified by the
    /// main category roots.  Empty string = flat Types/ (no subfolder).
    /// Priority (declaration kind first, then modifiers, then chain inspection):
    ///   Enum → Delegate → Interface → Struct → Record →
    ///   Patches → Utils → Abstracts → Entities → Exposables → (flat)
    /// </summary>
    private static string GetUnsortedSubfolder(
        TypeEntry                                 entry,
        Dictionary<string, IReadOnlyList<string>> graph)
    {
        // Declaration-kind buckets — unambiguous, checked first
        switch (entry.Kind)
        {
            case TypeKind.Enum:      return SubEnums;
            case TypeKind.Delegate:  return SubDelegates;
            case TypeKind.Interface: return SubInterfaces;
            case TypeKind.Struct:    return SubStructs;
            case TypeKind.Record:    return SubRecords;
        }

        // Modifier / attribute buckets — Patches before Utils (a static patch class is
        // more specifically a patch than a generic utility)
        if (entry.IsPatch)    return SubPatches;
        if (entry.IsStatic)   return SubUtils;
        if (entry.IsAbstract) return SubAbstracts;

        // Inheritance-chain buckets (requires graph walk)
        if (ImplementsBase(entry.Name, "Entity",     graph)) return SubEntities;
        if (ImplementsBase(entry.Name, "IExposable", graph)) return SubExposables;

        return "";
    }

    /// <summary>
    /// Returns true if <paramref name="typeName"/> has <paramref name="target"/> anywhere
    /// in its inheritance/implementation chain (walks the local type graph only).
    /// </summary>
    private static bool ImplementsBase(
        string                                    typeName,
        string                                    target,
        Dictionary<string, IReadOnlyList<string>> graph)
        => ImplementsBaseInner(typeName, target, graph,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase));

    private static bool ImplementsBaseInner(
        string                                    typeName,
        string                                    target,
        Dictionary<string, IReadOnlyList<string>> graph,
        HashSet<string>                           visited)
    {
        if (!visited.Add(typeName)) return false;
        if (string.Equals(typeName, target, StringComparison.OrdinalIgnoreCase)) return true;
        if (!graph.TryGetValue(typeName, out var bases)) return false;
        foreach (string b in bases)
            if (ImplementsBaseInner(b, target, graph, visited)) return true;
        return false;
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
