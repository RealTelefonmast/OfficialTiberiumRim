// TRTools - development utilities for OfficialTiberiumRim
//
// Usage:
//   trtool                            full overview: tree + duplicates with diffs
//   trtool --path <dir>               override mod root (default: walk up from CWD)
//   trtool --output <file>            also write plain-text output to file
//   trtool --scope <subpath>          restrict tree + dupe scan to subdirectory
//
//   trtool --stats                    counts only: files scanned, duplicate names, identical/differing pairs
//   trtool --tree-only                directory tree only, no duplicate scan
//   trtool --dupes-only               duplicate names + paths only, no tree, no diffs
//   trtool --file <name>              find all copies of filename and diff them, nothing else
//
//   trtool --depth <n>                limit tree depth (default: unlimited)
//   trtool --ext <ext>                filter duplicate scan by extension, e.g. --ext cs
//   trtool --sample <n>               limit duplicate output to first N groups
//   trtool --skip-file <names>        exclude filename(s) from duplicate scan (comma-separated, e.g. AssemblyInfo.cs,GlobalUsings.cs)
//   trtool --no-tree                  skip directory tree
//   trtool --no-diff                  skip diffs (show paths only)
//   trtool --no-identical             skip duplicate pairs that are byte-for-byte identical
//   trtool --identical-only           show only duplicate groups where all pairs are identical
//   trtool --delete-identical         dry-run: show which identical dupes would be deleted and which kept
//   trtool --delete-identical --confirm   actually delete them
//   trtool --flatten-textures         dry-run: copy all images into a flat Textures_Dump folder
//   trtool --flatten-textures --confirm   actually perform the copy
//   trtool --dest <name>              override dump folder name (default: Textures_Dump)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace TRTools
{
    internal static class Program
    {
        private static readonly HashSet<string> SkippedDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".git", "bin", "obj", ".vs", ".idea", "node_modules"
        };

        private static readonly HashSet<string> BinaryExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tga", ".dds",
            ".wav", ".mp3", ".ogg",
            ".dll", ".exe", ".pdb",
            ".zip", ".7z", ".rar"
        };

        private static bool UseColors => !Console.IsOutputRedirected;
        private static StreamWriter _fileOut = null;
        private static readonly HashSet<string> _skipFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        internal static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            string rootPath   = ResolveModRoot(args);
            string outputPath = ResolveArg(args, "--output");
            string scope      = ResolveArg(args, "--scope");
            string fileFilter = ResolveArg(args, "--file");
            string extFilter  = ResolveArg(args, "--ext");
            string skipArg    = ResolveArg(args, "--skip-file");
            string depthArg   = ResolveArg(args, "--depth");
            string sampleArg  = ResolveArg(args, "--sample");

            int maxDepth = int.MaxValue;
            if (depthArg != null && int.TryParse(depthArg, out int d))
                maxDepth = d;

            int sampleLimit = int.MaxValue;
            if (sampleArg != null && int.TryParse(sampleArg, out int s))
                sampleLimit = s;

            if (extFilter != null)
                extFilter = extFilter.TrimStart('.');

            if (skipArg != null)
                foreach (string sf in skipArg.Split(','))
                    _skipFiles.Add(sf.Trim());

            bool stats              = args.Contains("--stats");
            bool dupesOnly          = args.Contains("--dupes-only");
            bool treeOnly           = args.Contains("--tree-only");
            bool deleteIdentical    = args.Contains("--delete-identical");
            bool flattenTextures    = args.Contains("--flatten-textures");
            bool confirm            = args.Contains("--confirm");
            bool verbose            = args.Contains("--verbose");
            bool noTree             = stats || dupesOnly || fileFilter != null || deleteIdentical || flattenTextures || args.Contains("--no-tree");
            bool noDiff             = dupesOnly || args.Contains("--no-diff");
            bool noIdentical        = args.Contains("--no-identical");
            bool identicalOnly      = args.Contains("--identical-only");
            string destName         = ResolveArg(args, "--dest") ?? "Textures_Dump";

            if (outputPath != null)
                _fileOut = new StreamWriter(outputPath, append: false, Encoding.UTF8);

            string scanRoot = scope != null
                ? Path.GetFullPath(Path.Combine(rootPath, scope))
                : rootPath;

            // --file: focused mode — find all copies and diff, nothing else
            if (fileFilter != null)
            {
                RunFileMode(rootPath, scanRoot, fileFilter);
                Finish();
                return;
            }

            // --stats: counts only, no detail
            if (stats)
            {
                RunStats(rootPath, scanRoot, extFilter);
                Finish();
                return;
            }

            // --delete-identical: dry-run (or live with --confirm)
            if (deleteIdentical)
            {
                RunDeleteIdentical(rootPath, scanRoot, extFilter, sampleLimit, confirm);
                Finish();
                return;
            }

            // --flatten-textures: copy all images into a flat dump folder
            if (flattenTextures)
            {
                RunFlattenTextures(rootPath, scanRoot, destName, sampleLimit, verbose, confirm);
                Finish();
                return;
            }

            if (!noTree)
            {
                PrintSection("Directory Overview" + (scope != null ? " (" + scope + ")" : ""));
                Write(Path.GetFileName(scanRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) + "/");
                PrintTree(scanRoot, "", 0, maxDepth);
                Write("");
            }

            if (!treeOnly)
            {
                string dupeLabel = "Duplicate Filenames";
                if (extFilter != null)  dupeLabel += " (." + extFilter + ")";
                if (scope != null)      dupeLabel += " in " + scope;
                PrintSection(dupeLabel);

                Dictionary<string, List<string>> duplicates = FindDuplicates(scanRoot, extFilter);

                if (duplicates.Count == 0)
                {
                    Colored(ConsoleColor.DarkGray, "No duplicate filenames found.");
                }
                else
                {
                    PrintDuplicates(rootPath, duplicates, noDiff, noIdentical, identicalOnly, sampleLimit);
                }
            }

            Finish();
        }

        // ── Modes ───────────────────────────────────────────────────────────

        private static void RunFileMode(string rootPath, string scanRoot, string filename)
        {
            // Normalise: strip path separators so bare names and relative paths both work
            filename = Path.GetFileName(filename);

            Dictionary<string, List<string>> all = FindDuplicates(scanRoot, extFilter: null);

            List<string> paths;
            if (!all.TryGetValue(filename, out paths))
            {
                // Not a duplicate — check if it exists at all
                var found = Directory.EnumerateFiles(scanRoot, filename, SearchOption.AllDirectories)
                    .Where(f => !IsInSkippedDir(scanRoot, f))
                    .ToList();

                if (found.Count == 0)
                    Colored(ConsoleColor.Red, "Not found: " + filename);
                else
                {
                    Colored(ConsoleColor.DarkGray, "Only one copy found (no duplicate):");
                    Write("  " + Path.GetRelativePath(rootPath, found[0]));
                }
                return;
            }

            PrintSection("File: " + filename);
            for (int i = 0; i < paths.Count; i++)
                Write("  " + (char)('A' + i) + ": " + Path.GetRelativePath(rootPath, paths[i]));

            for (int i = 0; i < paths.Count; i++)
            for (int j = i + 1; j < paths.Count; j++)
            {
                if (paths.Count > 2)
                {
                    Write("");
                    Colored(ConsoleColor.DarkCyan, "Diff " + (char)('A' + i) + " vs " + (char)('A' + j) + ":");
                }
                Write("");
                PrintUnifiedDiff(
                    Path.GetRelativePath(rootPath, paths[i]),
                    Path.GetRelativePath(rootPath, paths[j]),
                    paths[i],
                    paths[j]);
            }
        }

        private static void RunStats(string rootPath, string scanRoot, string extFilter)
        {
            int totalFiles = 0;
            foreach (string f in Directory.EnumerateFiles(scanRoot, "*", SearchOption.AllDirectories))
            {
                if (!IsInSkippedDir(scanRoot, f))
                    totalFiles++;
            }

            Dictionary<string, List<string>> duplicates = FindDuplicates(scanRoot, extFilter);

            int identicalPairs = 0;
            int differingPairs = 0;
            int identicalNames = 0;
            int differingNames = 0;

            foreach (List<string> paths in duplicates.Values)
            {
                bool anyDiffer = false;
                for (int i = 0; i < paths.Count; i++)
                for (int j = i + 1; j < paths.Count; j++)
                {
                    if (AreFilesIdentical(paths[i], paths[j]))
                        identicalPairs++;
                    else
                    {
                        differingPairs++;
                        anyDiffer = true;
                    }
                }
                if (anyDiffer) differingNames++;
                else           identicalNames++;
            }

            PrintSection("Stats" + (extFilter != null ? " (." + extFilter + ")" : ""));
            Write("Root:              " + rootPath);
            Write("Files scanned:     " + totalFiles.ToString("N0"));
            Write("Duplicate names:   " + duplicates.Count);
            Write("  All identical:   " + identicalNames + "  (" + identicalPairs + " pair(s) — safe to delete one)");
            Write("  Have diffs:      " + differingNames + "  (" + differingPairs + " pair(s) — need merging)");
        }

        private static void RunDeleteIdentical(string rootPath, string scanRoot, string extFilter, int sampleLimit, bool confirm)
        {
            Dictionary<string, List<string>> duplicates = FindDuplicates(scanRoot, extFilter);

            var deletions = new List<(string keep, List<string> remove)>();
            int skippedGroups = 0;

            foreach (List<string> paths in duplicates.Values)
            {
                // Skip any group that has at least one differing pair
                bool allIdentical = true;
                for (int i = 0; i < paths.Count && allIdentical; i++)
                    for (int j = i + 1; j < paths.Count && allIdentical; j++)
                        if (!AreFilesIdentical(paths[i], paths[j]))
                            allIdentical = false;

                if (!allIdentical) { skippedGroups++; continue; }

                string keep = paths
                    .OrderByDescending(p => PathScore(rootPath, p))
                    .ThenBy(p => Path.GetRelativePath(rootPath, p).Length)
                    .ThenBy(p => Path.GetRelativePath(rootPath, p), StringComparer.OrdinalIgnoreCase)
                    .First();

                List<string> remove = paths.Where(p => p != keep).ToList();
                deletions.Add((keep, remove));
            }

            int totalToDelete = deletions.Sum(d => d.remove.Count);

            PrintSection("Delete Identical Duplicates" + (confirm ? "" : " [DRY RUN]"));
            Write("Identical groups:        " + deletions.Count);
            Write("Files to delete:         " + totalToDelete);
            Write("Differing groups (skip): " + skippedGroups);

            if (!confirm)
            {
                Write("");
                Colored(ConsoleColor.DarkGray, "Dry run — add --confirm to actually delete.");
            }

            bool truncated = deletions.Count > sampleLimit;
            IEnumerable<(string keep, List<string> remove)> toShow = truncated
                ? deletions.Take(sampleLimit)
                : deletions;

            foreach (var (keep, remove) in toShow)
            {
                Write("");
                Colored(ConsoleColor.Green,  "  KEEP   " + Path.GetRelativePath(rootPath, keep));
                foreach (string del in remove)
                {
                    if (confirm)
                    {
                        try
                        {
                            File.Delete(del);
                            Colored(ConsoleColor.Red, "  DELETE " + Path.GetRelativePath(rootPath, del));
                        }
                        catch (Exception ex)
                        {
                            Colored(ConsoleColor.Red, "  FAILED " + Path.GetRelativePath(rootPath, del) + " — " + ex.Message);
                        }
                    }
                    else
                    {
                        Colored(ConsoleColor.DarkGray, "  DEL    " + Path.GetRelativePath(rootPath, del));
                    }
                }
            }

            if (truncated)
            {
                Write("");
                Colored(ConsoleColor.DarkGray, "... " + (deletions.Count - sampleLimit) + " more group(s) not shown (--sample " + sampleLimit + ")");
            }

            if (confirm)
            {
                Write("");
                Write("Done. Deleted " + totalToDelete + " file(s).");
            }
        }

        // Extensions to skip when flattening — everything else gets copied
        private static readonly HashSet<string> FlattenSkipExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".xml", ".cs", ".csproj", ".sln", ".props", ".targets", ".config"
        };

        private static void RunFlattenTextures(string rootPath, string scanRoot, string destName, int sampleLimit, bool verbose, bool confirm)
        {
            string destDir = Path.Combine(rootPath, destName);

            // Collect all non-code files, skipping dest folder and skipped dirs
            var files = new List<string>();
            foreach (string f in Directory.EnumerateFiles(scanRoot, "*", SearchOption.AllDirectories))
            {
                if (IsInSkippedDir(scanRoot, f)) continue;
                string rel = Path.GetRelativePath(rootPath, f);
                if (rel.StartsWith(destName + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) continue;
                if (!FlattenSkipExtensions.Contains(Path.GetExtension(f)))
                    files.Add(f);
            }

            // Build copy plan — resolve conflicts with _CopyN suffix
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var plan = new List<(string src, string destFile, bool renamed)>();

            foreach (string src in files.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                string baseName  = Path.GetFileNameWithoutExtension(src);
                string ext       = Path.GetExtension(src);
                string candidate = baseName + ext;
                int copy = 1;

                while (usedNames.Contains(candidate))
                {
                    candidate = baseName + "_Copy" + copy + ext;
                    copy++;
                }

                usedNames.Add(candidate);
                bool renamed = !string.Equals(Path.GetFileName(src), candidate, StringComparison.OrdinalIgnoreCase);
                plan.Add((src, candidate, renamed));
            }

            int conflicts = plan.Count(p => p.renamed);

            PrintSection("Flatten Textures" + (confirm ? "" : " [DRY RUN]"));
            Write("Source: " + scanRoot);
            Write("Dest:   " + destDir);
            Write("Images found:  " + plan.Count);
            Write("Renamed:       " + conflicts);

            if (!confirm)
            {
                Write("");
                Colored(ConsoleColor.DarkGray, "Dry run — add --confirm to copy. Use --verbose to list all files.");
            }

            Write("");

            if (confirm)
                Directory.CreateDirectory(destDir);

            bool truncated = false;
            int shown = 0;

            foreach (var (src, destFile, renamed) in plan)
            {
                string srcRel = Path.GetRelativePath(rootPath, src);

                if (confirm)
                {
                    try
                    {
                        File.Copy(src, Path.Combine(destDir, destFile), overwrite: false);
                    }
                    catch (Exception ex)
                    {
                        Colored(ConsoleColor.Red, "  FAILED " + srcRel + " — " + ex.Message);
                        continue;
                    }
                }

                // Output: always show renamed, only show clean copies in --verbose
                bool shouldPrint = renamed || verbose;
                if (!shouldPrint) continue;

                if (shown >= sampleLimit) { truncated = true; continue; }
                shown++;

                if (renamed)
                    Colored(ConsoleColor.Yellow, "  " + srcRel + " → " + destFile);
                else
                    Colored(ConsoleColor.DarkGray, "  " + srcRel);
            }

            if (truncated)
            {
                Write("");
                Colored(ConsoleColor.DarkGray, "... output truncated (--sample " + sampleLimit + ")");
            }

            if (confirm)
            {
                Write("");
                Write("Done. Copied " + plan.Count + " file(s) to " + destDir);
            }
        }

        // Score: higher = more canonical = keep. Lower = delete.
        // Non-versioned live files: MaxValue
        // Versioned dirs (1.4 > 1.3 > 1.2): major*100 + minor
        // Source\TRDupes: -1
        private static int PathScore(string rootPath, string absPath)
        {
            string rel = Path.GetRelativePath(rootPath, absPath);
            string[] parts = rel.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string top = parts[0];

            // Archive directory
            if (parts.Length > 1 &&
                top.Equals("Source", StringComparison.OrdinalIgnoreCase) &&
                parts[1].Equals("TRDupes", StringComparison.OrdinalIgnoreCase))
                return -1;

            // Versioned directory e.g. "1.4"
            if (top.Length > 0 && char.IsDigit(top[0]))
            {
                string[] v = top.Split('.');
                if (v.Length >= 2 &&
                    int.TryParse(v[0], out int major) &&
                    int.TryParse(v[1], out int minor))
                    return major * 100 + minor;
            }

            // Live file — highest priority
            return int.MaxValue;
        }

        // ── Duplicate output ────────────────────────────────────────────────

        private static void PrintDuplicates(
            string rootPath,
            Dictionary<string, List<string>> duplicates,
            bool noDiff,
            bool noIdentical,
            bool identicalOnly,
            int sampleLimit)
        {
            var groups = duplicates
                .OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (noIdentical)
            {
                groups = groups.Where(kvp =>
                {
                    List<string> paths = kvp.Value;
                    for (int i = 0; i < paths.Count; i++)
                        for (int j = i + 1; j < paths.Count; j++)
                            if (!AreFilesIdentical(paths[i], paths[j]))
                                return true;
                    return false;
                }).ToList();
            }
            else if (identicalOnly)
            {
                groups = groups.Where(kvp =>
                {
                    List<string> paths = kvp.Value;
                    for (int i = 0; i < paths.Count; i++)
                        for (int j = i + 1; j < paths.Count; j++)
                            if (!AreFilesIdentical(paths[i], paths[j]))
                                return false;
                    return true;
                }).ToList();
            }

            if (groups.Count == 0)
            {
                Colored(ConsoleColor.DarkGray, noIdentical
                    ? "No differing duplicates found (all pairs are identical)."
                    : "No identical duplicates found.");
                return;
            }

            bool truncated = groups.Count > sampleLimit;
            if (truncated)
                groups = groups.Take(sampleLimit).ToList();

            Write(groups.Count + (truncated ? " (sampled)" : "") + " duplicate filename(s):");

            int idx = 0;
            foreach (KeyValuePair<string, List<string>> entry in groups)
            {
                idx++;
                string name = entry.Key;
                List<string> paths = entry.Value;

                Write("");
                Colored(ConsoleColor.Yellow, "[" + idx + "/" + groups.Count + "] " + name);

                for (int i = 0; i < paths.Count; i++)
                    Write("  " + (char)('A' + i) + ": " + Path.GetRelativePath(rootPath, paths[i]));

                if (noDiff)
                    continue;

                for (int i = 0; i < paths.Count; i++)
                for (int j = i + 1; j < paths.Count; j++)
                {
                    if (noIdentical && AreFilesIdentical(paths[i], paths[j]))
                        continue;

                    if (paths.Count > 2)
                    {
                        Write("");
                        Colored(ConsoleColor.DarkCyan, "  Diff " + (char)('A' + i) + " vs " + (char)('A' + j) + ":");
                    }

                    Write("");
                    PrintUnifiedDiff(
                        Path.GetRelativePath(rootPath, paths[i]),
                        Path.GetRelativePath(rootPath, paths[j]),
                        paths[i],
                        paths[j]);
                }
            }
        }

        // ── File system helpers ─────────────────────────────────────────────

        private static string ResolveModRoot(string[] args)
        {
            string path = ResolveArg(args, "--path") ?? ResolveArg(args, "-p");
            if (path != null)
                return Path.GetFullPath(path);

            string dir = Directory.GetCurrentDirectory();
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir, "About", "About.xml")))
                    return dir;
                dir = Path.GetDirectoryName(dir);
            }

            return @"C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\OfficialTiberiumRim";
        }

        private static string ResolveArg(string[] args, string flag)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == flag)
                    return args[i + 1];
            }
            return null;
        }

        private static bool IsInSkippedDir(string root, string filePath)
        {
            string rel = Path.GetRelativePath(root, filePath);
            string[] parts = rel.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return parts.Any(p => SkippedDirs.Contains(p));
        }

        private static bool AreFilesIdentical(string pathA, string pathB)
        {
            try
            {
                var infoA = new FileInfo(pathA);
                var infoB = new FileInfo(pathB);
                if (infoA.Length != infoB.Length)
                    return false;
                return File.ReadAllBytes(pathA).SequenceEqual(File.ReadAllBytes(pathB));
            }
            catch
            {
                return false;
            }
        }

        // ── Directory tree ──────────────────────────────────────────────────

        private static void PrintTree(string dir, string prefix, int depth, int maxDepth)
        {
            if (depth >= maxDepth)
                return;

            string[] entries;
            try
            {
                entries = Directory.GetFileSystemEntries(dir);
            }
            catch
            {
                return;
            }

            List<string> sorted = entries
                .Where(e => !SkippedDirs.Contains(Path.GetFileName(e)))
                .OrderBy(e => Directory.Exists(e) ? 0 : 1)
                .ThenBy(e => Path.GetFileName(e), StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                string entry     = sorted[i];
                bool last        = (i == sorted.Count - 1);
                string connector = last ? "\u2514\u2500\u2500 " : "\u251c\u2500\u2500 ";
                string childPfx  = prefix + (last ? "    " : "\u2502   ");
                string name      = Path.GetFileName(entry);

                if (Directory.Exists(entry))
                {
                    Colored(ConsoleColor.Blue, prefix + connector + name + "/");
                    PrintTree(entry, childPfx, depth + 1, maxDepth);
                }
                else
                {
                    Write(prefix + connector + name);
                }
            }
        }

        // ── Duplicate detection ─────────────────────────────────────────────

        private static Dictionary<string, List<string>> FindDuplicates(string root, string extFilter)
        {
            Dictionary<string, List<string>> map = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (string file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                if (IsInSkippedDir(root, file))
                    continue;

                if (extFilter != null)
                {
                    string fileExt = Path.GetExtension(file).TrimStart('.');
                    if (!string.Equals(fileExt, extFilter, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                string name = Path.GetFileName(file);
                if (_skipFiles.Count > 0 && _skipFiles.Contains(name))
                    continue;
                if (!map.TryGetValue(name, out List<string> list))
                {
                    list = new List<string>();
                    map[name] = list;
                }
                list.Add(file);
            }

            return map
                .Where(kvp => kvp.Value.Count > 1)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        // ── Unified diff ────────────────────────────────────────────────────

        private static void PrintUnifiedDiff(string relA, string relB, string absA, string absB)
        {
            if (IsBinaryPath(absA) || IsBinaryPath(absB))
            {
                Colored(ConsoleColor.DarkGray, "  (binary - skipping diff)");
                return;
            }

            string textA, textB;
            try
            {
                textA = File.ReadAllText(absA);
                textB = File.ReadAllText(absB);
            }
            catch (Exception ex)
            {
                Colored(ConsoleColor.Red, "  (read error: " + ex.Message + ")");
                return;
            }

            if (textA == textB)
            {
                Colored(ConsoleColor.DarkGray, "  (identical)");
                return;
            }

            DiffPaneModel diff = InlineDiffBuilder.Diff(textA, textB, ignoreWhiteSpace: false, ignoreCase: false);
            if (!diff.HasDifferences)
            {
                Colored(ConsoleColor.DarkGray, "  (identical after normalization)");
                return;
            }

            IList<DiffPiece> lines = diff.Lines;

            Colored(ConsoleColor.DarkGray, "--- " + relA);
            Colored(ConsoleColor.DarkGray, "+++ " + relB);

            List<int> changed = lines
                .Select((l, i) => new { l, i })
                .Where(x => x.l.Type == ChangeType.Inserted || x.l.Type == ChangeType.Deleted)
                .Select(x => x.i)
                .ToList();

            foreach (Tuple<int, int> hunk in BuildHunks(changed, lines.Count, 3))
            {
                int start = hunk.Item1;
                int end   = hunk.Item2;

                int oldStart = lines.Take(start).Count(l => l.Type == ChangeType.Unchanged || l.Type == ChangeType.Deleted) + 1;
                int newStart = lines.Take(start).Count(l => l.Type == ChangeType.Unchanged || l.Type == ChangeType.Inserted) + 1;
                int oldCount = lines.Skip(start).Take(end - start + 1).Count(l => l.Type == ChangeType.Unchanged || l.Type == ChangeType.Deleted);
                int newCount = lines.Skip(start).Take(end - start + 1).Count(l => l.Type == ChangeType.Unchanged || l.Type == ChangeType.Inserted);

                Colored(ConsoleColor.Cyan, "@@ -" + oldStart + "," + oldCount + " +" + newStart + "," + newCount + " @@");

                for (int i = start; i <= end; i++)
                {
                    DiffPiece line = lines[i];
                    if (line.Type == ChangeType.Unchanged)
                        Write(" " + line.Text);
                    else if (line.Type == ChangeType.Deleted)
                        Colored(ConsoleColor.Red, "-" + line.Text);
                    else if (line.Type == ChangeType.Inserted)
                        Colored(ConsoleColor.Green, "+" + line.Text);
                }
            }
        }

        private static IEnumerable<Tuple<int, int>> BuildHunks(List<int> changed, int total, int context)
        {
            if (changed.Count == 0)
                yield break;

            int s = Math.Max(0, changed[0] - context);
            int e = Math.Min(total - 1, changed[0] + context);

            for (int i = 1; i < changed.Count; i++)
            {
                int ns = Math.Max(0, changed[i] - context);
                int ne = Math.Min(total - 1, changed[i] + context);
                if (ns <= e + 1)
                    e = ne;
                else
                {
                    yield return Tuple.Create(s, e);
                    s = ns;
                    e = ne;
                }
            }
            yield return Tuple.Create(s, e);
        }

        private static bool IsBinaryPath(string path)
        {
            return BinaryExtensions.Contains(Path.GetExtension(path));
        }

        // ── Output helpers ──────────────────────────────────────────────────

        private static void Write(string text)
        {
            Console.WriteLine(text);
            _fileOut?.WriteLine(text);
        }

        private static void Colored(ConsoleColor color, string text)
        {
            if (UseColors) Console.ForegroundColor = color;
            Console.WriteLine(text);
            if (UseColors) Console.ResetColor();
            _fileOut?.WriteLine(text);
        }

        private static void PrintSection(string title)
        {
            Write("");
            Colored(ConsoleColor.Green, "=== " + title + " ===");
            Write("");
        }

        private static void Finish()
        {
            _fileOut?.Flush();
            _fileOut?.Dispose();
        }
    }
}
