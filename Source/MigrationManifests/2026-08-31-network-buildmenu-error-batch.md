# Migration manifest: network/build-menu XML error batch

## Active migrations

- Migrated `Network_Chemical.xml` from the retired `TeleCore.*` network type names to the current `TeleCore.RW.Networks` declarations.
- Migrated `CompProperties_Network` to `TeleCore.RW.Networks.Comps.Properties.CompProperties_Network`, `PlaceWorker_Pipe` to `TeleCore.RW.Networks.PlaceWorker_Pipe`, and `Graphic_LinkedWithSame` to `TeleCore.RW.Graphic_LinkedWithSame`.
- Added the current required `networkDef` cross-reference to every chemical `NetworkValueDef` while retaining its inherited `collectionDef` cross-reference.
- Repaired Copper's malformed color tuple from `(200,128,51` to `(200,128,51)`.
- Removed the retired RimWorld `placingDraggableDimensions` field from all loaded TiberiumRim and TeleCore base defs that still serialized it.
- Migrated all loaded TiberiumRim build-menu extensions from `TeleCore.RW.UI.SpecialSubMenu.SubMenuExtension` to `TeleCore.SubMenuExtension`.
- Rebound `ChronoVortexPortal` from the retired `TiberiumRim.ChronoVortex` name to `TiberiumRim.Portal_ChronoVortex`.

## Texture sorting

- Rebound the chemical pipe's base, overlay, and blueprint paths to their active location under `Textures/Things/Buildings/Network/Pipes`.
- Rebound the Chrono Vortex texture to `Textures/FX/Projectiles/ChronoVortex.png`.
- Migrated `Textures_Dump/VortexDistortionMask.png` into `Textures/FX/Projectiles/VortexDistortionMask.png` and rebound the shader parameter to that active path.
- The mask source and active copy are byte-identical, SHA-256 `BBE9C78532D2A0CCC280701823F9B171DE7F53995B47792ECFDF9237F7335F2E`.
- The reference tree contained no pipe or Chrono Vortex texture counterpart under its loadable `Textures` directory, so no additional related texture was eligible for removal.

## Reference removals after declaration-level verification

- `_Outdated/Defs/Networks/Network_Chemical.xml`
- `_Outdated/Defs/TiberiumCrystals/Base.xml`
- `_Outdated/Defs/Buildings/Building_Bases.xml`
- `_Outdated/Defs/Buildings/RedAlert/Misc/RA_Misc.xml`
- `_Outdated/Defs/Things.Buildings/RedAlert/Misc/RA_Misc.xml` (commented duplicate of the same Chrono Vortex declaration)
- `Textures_Dump/VortexDistortionMask.png`

The XML diffs contained namespace/schema/path migrations only; every original declaration and serialized value remains represented in the active files. The abstract base files have no directly owned texture paths.

## Recovery artifacts

The reference tree has no Git recovery history. Byte-identical, Git-tracked copies of all four removed XML files remain in `OfficialTiberiumRim/Source/Org`:

- `_Outdated/Defs/Networks/Network_Chemical.xml` — SHA-256 `3314FF125DA85A0BB6A27513D980A3BAC0CC8E25E8DE1371664EB2D0A359C467`
- `_Outdated/Defs/TiberiumCrystals/Base.xml` — SHA-256 `0F212624738D9BE78BEC8C1048D81FA428B833CE0F7F3DE3119C19DD268B1D70`
- `_Outdated/Defs/Buildings/Building_Bases.xml` — SHA-256 `6B75C8CC025D5F161F50404AA1859FCC1EC83F669F9CB7E5D1C5ADB8AA480065`
- `_Outdated/Defs/Buildings/RedAlert/Misc/RA_Misc.xml` — SHA-256 `ED2465B5B941EB2942418A531C881341A0AC2DB52054250812475FE09F11B53C`
- `_Outdated/Defs/Things.Buildings/RedAlert/Misc/RA_Misc.xml` — the Git-tracked `Source/Org` copy is content-identical after line-ending normalization; removed reference SHA-256 `FB8C2C53F64B06A2EE8BA0E1D64F287AF44081D08FE99CE70313A71E5FF04A68`

The removed mask's complete byte payload is retained by its hash-identical active copy at `OfficialTiberiumRim/Textures/FX/Projectiles/VortexDistortionMask.png`.
