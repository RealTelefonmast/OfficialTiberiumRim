# 2026-08-31 startup cross-reference batch

## Scope

The supplied startup log contained 58 unique missing Def names. Their archived source declarations were migrated into `1.6/Defs/Recovered/` with retired XML type names mapped to the current `TiberiumRim`, `TeleCore`, and `TeleCore.RW` owners.

## Families restored

- Tiberium exposure Hediffs, typed radiation data, terrain tags, gas, research, jobs, sounds, world objects, and main-button data.
- Tiberium producers, meteorite, drone platform, visceral pod, Tiberium pipe and Tiberium/Waste networks.
- Particle, mote, fleck, build-menu, PawnKind, and Tiberium-kind declarations.

## Deliberate retention

No archived XML or texture files were deleted in this batch. Several migrated declarations still use assets whose historical paths have not yet been mapped one-for-one to the reorganized texture tree; retain the sources until the next RimWorld reload proves the active forms and their asset paths.

## Verification

- All active `1.6/Defs` XML files parse.
- Every one of the 58 requested names now has a matching active Def type (with `TRThingDef` counted as the requested `ThingDef` subclass).
- Duplicate Def keys introduced by overlapping archived copies were removed from the active recovery set.
- `dotnet build TiberiumRim.csproj -c Release` succeeded with 0 warnings and 0 errors.
- The rebuilt `TiberiumRim.dll` and `.pdb` were copied to `1.6/Assemblies` and verified by SHA-256 equality.
