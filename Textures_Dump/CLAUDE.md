# TiberiumRim — CLAUDE.md

---

## The Developer

**Maxim** — pseudonym **Telefonmast**. Self-taught since 2018, starting with this very mod
(*TiberiumRim*) at 17 with zero prior coding experience. C# is his primary language, 8 years deep.
Formal credential: *Fachinformatiker für Anwendungsentwicklung* (German applied software dev degree).
Currently employed at **Ludeon Studios** (the RimWorld developer).

**Background worth knowing:**
- Modding is the origin — TiberiumRim, and many other RimWorld mods (look them up under Telefonmast)
- Rewrote IMGUI systems inside Unity and RimWorld; built an in-game animation tool
- Written custom HLSL shaders for RW mods
- Ongoing side project: a **C → C# transpiler** (working on simple files)
- Parallel project: **Remergence** — a MonoGame engine framework (separate repo, separate CLAUDE.md)
- Prefers DOD, structs, flat data, static managers — design emerges from writing

---

## Project Overview

**TiberiumRim** is a RimWorld mod bringing the **Command & Conquer Tiberium universe** into RimWorld.
Factions (GDI, Nod, Scrin), Tiberium crystal growth/harvesting/processing, veinholes, ion storms,
mechanical pawns, superweapons — the full C&C experience adapted to RimWorld's colony sim.

- **Package ID:** `telefonmast.tiberiumrim`
- **Authors:** Telefonmast, Nephlite
- **Target RimWorld versions:** 1.4, 1.6 (active development targets 1.6)
- **Status:** Development build — not publicly released in current state

---

## Code Reference: RimWorld Source

The RimWorld installation includes **partial source** at:
```
C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Source\
├── RimWorld\    ← Game-layer classes (FactionDef, IncidentWorkers, Building_Door, Plant, etc.)
└── Verse\       ← Engine-layer classes
```

~43 key `.cs` files covering core types (FactionDef, Building_Storage, Building_Door, Fire,
Plant, Bullet, CompArt, CompExplosive, Apparel, etc.). **Not the full source** — but useful
as direct reference for the classes it does include. For classes not present here, fall back
to the Krafs.Rimworld.Ref NuGet package metadata or ask Maxim.

---

## Repository Layout

```
OfficialTiberiumRim/
├── About/                    ← Mod metadata (About.xml)
├── Assemblies/               ← Compiled DLLs (dev — currently empty)
├── Defs/                     ← XML definitions (51 categories)
├── Languages/                ← Localization
├── Materials/                ← Shaders and materials
├── Patches/                  ← XML patches for cross-mod compat
├── RimThemes/                ← Theme definitions
├── Sounds/                   ← Audio assets
├── Source/TiberiumRim/       ← C# source (main project + TeleCore)
├── Textures/                 ← Image assets
├── 1.4/Assemblies/           ← RW 1.4 build output
├── 1.6/Assemblies/           ← RW 1.6 build output (active target)
└── CLAUDE.md                 ← this file
```

---

## Source Structure (Source/TiberiumRim/)

> **[EVOLVING — ARTIFACT RESOLUTION IN PROGRESS]**
> This structure is not fixed or final. The codebase is actively being cleaned up:
> duplicate class definitions, wrong namespaces, scattered implementations, and merge
> artifacts are being resolved incrementally. The table below reflects the *target* layout,
> not necessarily the current state of every file. Expect namespace moves, file deletions,
> and consolidations until this flag is removed.

### Main Mod — TiberiumRim.csproj
- **Target:** .NET Framework 4.8 (net48)
- **Output:** `../../1.6/Assemblies/`
- **Language version:** Latest
- **Platform:** x64
- **Dependencies:** Krafs.Rimworld.Ref 1.6.4633, Lib.Harmony 2.4.2, HugsLib 12.0.0

### Major Namespaces / Folders

| Folder | Domain | Key Classes |
|---|---|---|
| `AI/` | Jobs & AI | `JobDriver_*`, `JobGiver_*` |
| `Comps/` | ThingComps | `CompDroneStation`, `CompMechStation`, etc. |
| `Data/` | Core data | DamageHandling, Enums, Environment, StatHandling, ThingClasses |
| `Defs/` | Custom Defs | `TRThingDef`, `MechRecipeDef`, `MechUpgradeDef`, DefOf classes |
| `Factions/` | C&C Factions | GDI, Nod, Scrin, RARelics, portal spawners |
| `GameParts/` | Game systems | MapComps, Designators, EVA, Networks, World gen |
| `Hediffs/` | Health | Tiberium exposure, mutations, hediff verbs |
| `Loading/` | Harmony patches | `TRPawnDefInject`, `TRThingPatches`, `TRUIPatches`, etc. |
| `MechanicalPawns/` | Mech system | `MechanicalPawn`, `MechGarage`, Vehicles |
| `Rendering/` | Visuals | Custom graphics, effecters, motes, section layers |
| `Research/` | Research UI | Custom research window and discovery table |
| `TiberiumEnvironment/` | Tiberium core | `GenTiberium`, `Grid_Tiberium`, `TiberiumField`, terrain conversion |
| `TiberiumObjects/` | Tiberium things | Crystals, chunks, blossoms, craters, producers |
| `TiberiumPawns/` | Creatures | Visceroids, animal mutations |
| `TiberiumProcessing/` | Processing | Refineries, pipes, power, harvesting zones |
| `Utilities/` | Helpers | `TRFind`, `TRColor`, `HediffUtils`, geometry |
| `VeinholeSystem/` | Veinholes | VeinHub, VeinChunk, VeinRoamer, VeinGasCloud |
| `Weaponry/` | Weapons | Turrets, beam hubs, obelisks, superweapons, verbs |
| `Weather/` | Weather | Ion storms, weather events |

### Domain Separation

**Tiberium concerns** — crystal growth, terrain conversion, harvesting, processing, pollution,
environmental spread. Core files live in `TiberiumEnvironment/`, `TiberiumObjects/`,
`TiberiumProcessing/`, and the MapComps (`TiberiumMapInfo`, `TiberiumFloraMapInfo`,
`TiberiumPollutionMapInfo`, `TiberiumTerrainInfo`, etc.).

**Faction concerns** — GDI, Nod, Scrin each have distinct buildings, research trees, and unit
rosters. Faction-specific logic lives in `Factions/` and faction-tagged Defs under
`Defs/Buildings/GDI/`, `Defs/Buildings/Nod/`, etc.

**Keep these separated.** Tiberium mechanics should not depend on faction logic and vice versa.
Cross-cutting concerns (e.g., a GDI harvester interacting with Tiberium fields) should flow
through well-defined interfaces, not direct coupling.

---

## TeleCore Framework (Source/TiberiumRim/TeleCoreSrc/)

TeleCore is Maxim's cross-project shared framework. It is embedded in this repo as source
(not a NuGet package) and builds alongside TiberiumRim.

| Sub-Project | Purpose |
|---|---|
| **TeleCore** (net481) | Main framework — core systems, RimWorld integrations, types |
| **TeleCore.Animations** | Animation composition |
| **TeleCore.Events** | Global event system (pawn, thing, cell, region events) |
| **TeleCore.FlowCore** | Pipe networks, flow simulation, IO, bills, volumes |
| **TeleCore.Lib** | Data structures (ImmutableArray, SparseSet, OrderedDictionary, MathG/MathT) |
| **TeleCore.Math** | Generic numeric system (`Numeric<T>`, `NumericLib`) |
| **TeleCore.RWLib** | RimWorld-specific patches and map components |
| **TeleCore.Shared** | `DefValue<T>`, `DefValueStack`, parsing helpers, TColor |
| **TeleCore.UI** | UI widgets and text utilities |
| **TeleCore.Patching** | Harmony patching utilities |
| **TeleCore.Loader** | Custom mod loading |
| **TeleCore.RWDevTools** | Dev/debug tools |
| + others | AssetLoader, BuildMenu, DGUI, Unsafe, Update |

**Be mindful:** TeleCore types (`DefValue`, `DefValueStack`, `FlowSystem`, `NetworkVolume`,
`CompNetwork`, `PipeNetwork`, etc.) are the backbone of many TiberiumRim systems. When modifying
network or flow-related code, understand the TeleCore layer first.

**TeleCore.FlowCore** is particularly critical — it implements the entire pipe network and fluid
flow simulation used by Tiberium processing buildings.

---

## Defs / XML Structure

51 definition categories under `Defs/`. Key ones:

| Category | Content |
|---|---|
| `Buildings/` | Subcategorized by faction (Common, GDI, Nod, Scrin) and function |
| `Factions/` | Faction definitions |
| `HediffDefs/` | Tiberium exposure, mutations |
| `Networks/` | Pipe network definitions |
| `Races/` | Custom race definitions |
| `Research/` | Research project definitions |
| `TiberiumCrystals/` | Crystal type definitions |
| `Weaponry/` | Weapon and turret definitions |
| `MapGeneration/` | Map gen steps for Tiberium placement |
| `WorldGeneration/` | World-layer Tiberium spread |

---

## Build & Run

```bash
cd "Source/TiberiumRim"
dotnet build
```

Output goes to `1.6/Assemblies/`. RimWorld loads assemblies from the version-matched folder
automatically. The solution (`TiberiumRim.sln`) contains 18 projects (1 main + 17 TeleCore).

---

## Tools (`Tools/TRTools/`)

A co-located .NET 9 console utility for development tasks. Build and run with:

```bash
cd "Tools/TRTools"
dotnet run                         # full overview: directory tree + duplicates with unified diffs
dotnet run -- --path <root>        # override mod root path
dotnet run -- --output <file>      # also write plain-text output to file
dotnet run -- --scope <subpath>    # restrict tree + dupe scan to subdirectory

dotnet run -- --stats              # counts only: files, duplicate names, identical/differing pairs
dotnet run -- --tree-only          # directory tree only, no duplicate scan
dotnet run -- --dupes-only         # duplicate names + paths only, no tree, no diffs
dotnet run -- --file <name>        # find all copies of filename and diff them, nothing else

dotnet run -- --depth <n>          # limit tree depth
dotnet run -- --ext <ext>          # filter duplicate scan by extension, e.g. --ext cs
dotnet run -- --skip-file <names>  # exclude filename(s) from dupe scan (comma-separated, e.g. AssemblyInfo.cs,GlobalUsings.cs)
dotnet run -- --no-tree            # skip directory tree
dotnet run -- --no-diff            # skip diffs (list duplicates only)
dotnet run -- --no-identical       # skip duplicate pairs that are byte-for-byte identical
dotnet run -- --identical-only     # show only groups where all pairs are identical
dotnet run -- --sample <n>         # cap output to first N duplicate groups
dotnet run -- --delete-identical   # dry-run: show what would be kept/deleted
dotnet run -- --delete-identical --confirm  # actually delete identical dupes
dotnet run -- --flatten-textures           # dry-run: show renamed files, summary of all images found
dotnet run -- --flatten-textures --confirm # copy all images into flat Textures_Dump/ folder
dotnet run -- --dest <name>                # override dump folder name (default: Textures_Dump)
dotnet run -- --verbose                    # (with --flatten-textures) list all files, not just renamed
```

**Available tools (v1):**
- **overview** (default): Full directory tree of the mod root, then all duplicate filenames
  detected across the repo, each followed by a unified diff between the copies.

**Adding new tools:** Add a new command handler in `Program.cs`. Keep each tool focused.
Use `--ext`, `--scope`, `--depth` and similar flags to narrow scope rather than building
separate entry points for minor variations.

---

## Critical Gotchas

| Gotcha | Detail |
|---|---|
| **RimWorld partial source available** | `../../../Source/RimWorld/` and `../../../Source/Verse/` (~43 key files) — read these when available, don't guess at base game behavior |
| **TeleCore is in-tree** | Lives under `TeleCoreSrc/` — changes here affect all mods using TeleCore |
| **FlowCore complexity** | The pipe network system has its own flow simulation, pressure workers, clamping — understand before touching |
| **MapComponent proliferation** | Many custom MapComponents (`TiberiumMapInfo`, `TiberiumFloraMapInfo`, `TiberiumPollutionMapInfo`, etc.) — each has a distinct responsibility |
| **Harmony patches** | All in `Loading/` — `TRThingPatches`, `TRUIPatches`, `TRPawnDefInject`, etc. Be careful with patch ordering |
| **net48 vs net481** | TiberiumRim targets net48, TeleCore targets net481 — no `INumber<T>` or modern generics |
| **DefValue generics** | `DefValue<T>` / `DefValueStack<T>` in TeleCore.Shared — used extensively for typed def-value pairs |
| **Event system** | `GlobalEventHandler` in both TeleCore.Events and TeleCore proper — cell changes, thing state, region updates all flow through here |

---

## Working with Claude on This Project

**What works well:**
- Scaffolding and boilerplate to build on — not finished solutions
- "Sitrep" summaries of the current state of a system or file
- Logical, code-excited affirmation of understanding (not flattery — correctness validation)
- Reflecting patterns seen in the existing codebase before proposing changes
- Reading RimWorld source to confirm base game behavior before suggesting patches

**Rules for Claude:**
- **Always ask before making large changes.** If a request is ambiguous or has multiple valid
  approaches, ask first. Maxim prefers more questions over more assumptions.
- Do not silently refactor or restructure — state the intent, get the go-ahead
- Prefer suggesting over doing on anything architectural
- Match the existing code style: static managers, MapComponents, ThingComps, Harmony prefix/postfix
- **Read the RimWorld source** when unsure about base game APIs — don't guess
- Keep Tiberium mechanics and Faction logic decoupled
- When touching TeleCore, consider impact on other consumers beyond TiberiumRim
- **Update this file** whenever a new structural discovery is made about the codebase layout —
  namespace reorganizations, new subsystems found, class locations clarified, merge artifacts
  resolved. Keep the Source Structure and Critical Gotchas sections current.
- **Tool-first for heavy work:** Before starting any multi-file reads, pattern searches, bulk
  edits, or structural analysis, assess whether a tool in `Tools/TRTools/` can automate and
  return the results faster. If yes — build or extend the tool first, run it, use its output
  to drive the work. Prefer C# for new tools; scripts (Python, PowerShell) are fine for
  one-offs that don't warrant a compiled tool.

**Merge system** (for duplicate/artifact resolution):
- Keep all differences — do not discard any feature or behaviour present in any version
- "Newer" wins on conflicts: prefer cleaner syntax, more complete implementation, and logically
  canonical execution. Indicators of newer: auto-properties over backing fields, `readonly` where
  applicable, `ExposeData` present, def-based lookup over inline switch, more specific namespaces
- If the difference is too large or intent is ambiguous, **stop and ask** before proceeding
- If resolvable by deduction (clear progression of refactoring), proceed and state what was chosen

**Tone:** Direct, technical, curious. Enthusiasm for the code is welcome. Sycophancy is not.

---

## Design Philosophy

> Inherited from the developer's broader philosophy:

- **DOD (Data-Oriented Design)** — reduce instances, prefer structs, keep data flat
- **Low-level where it matters** — don't abstract away control unnecessarily
- **Naive re-invention** — understanding deeply beats re-using others' black boxes
- **No upfront architecture** — design emerges from writing; refactor when the shape is clear
- **APIs need docs, logic should be self-documenting**
