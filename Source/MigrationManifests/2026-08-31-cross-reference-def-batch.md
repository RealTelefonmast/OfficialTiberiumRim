# Migration manifest: cross-reference Def batch

## Active repairs

- Restored the complete Tiberium stat category and five-stat family. Runtime XML type names now use `TiberiumRim.*`; `StatPart_Tiberium.mode` is public again so the retained `<mode>` Def field binds in RimWorld 1.6.
- Restored all five `TeleCore.RW.DiscoveryDef` declarations, including `TiberiumCrystalLattice`.
- Restored the complete four-leather family and its `LeatherBase_TR` inheritance node.
- Restored the complete Tiberium/mutated-meat thought family and `TiberiumInfectionRevealed` tale, including the tale's commented expansion rules.
- Restored both Tiberium shard projectiles, their shared projectile base, both impact effecters, and both gas-particle flecks. Retired namespace and texture paths were rebound to current runtime owners and active assets.
- Restored `Bullet_ArmCannonSniper`, `Gas_ChemicalInterface`, `TiberiumGeyser`, and `TiberiumGeyserCrack` with current runtime types and active texture paths.
- Replaced temporary Tiberium build-menu/stat stubs with concrete current-type group/category/stat declarations. The broader retired build-menu file remains because its top-level menu and visibility-worker migration is not part of this batch.

## Reference XML removed after declaration-level verification

The following tracked files in `Source/Org` are fully represented by the active 1.6 XML. Their original payloads remain recoverable from the `Source/Org` Git `HEAD` recorded before deletion.

- `_Outdated/Defs/StatDefs/StatDef_Tiberium.xml` — `F5B627E9EDAD79F816A1CABD03098D571C9EB8C11DA5B7C5FBF686A234D2E17B`
- `_Outdated/Defs/StatDefs/StatCategoryDefs.xml` — `6D3D29455C7E5178DDB936E6F00F1A418CF5F8DB4A0ECC21BBC258FA7D369599`
- `_Outdated/Defs/Research/Discoveries.xml` — `F2E0832C02995B76FA34BDD3E0E7EFC0ADA1A5742E58865FD9B75A46589BD155`
- `_Outdated/Defs/ThingDefs/Items/Items_Leather.xml` and `_Outdated/Defs/ThingDefs_Item/Items_Leather.xml` — byte-identical, `442C69E34045EA4C863A3D04DCC59C5E06B299E22181A8521F3882FCE868186A`
- `_Outdated/Defs/TaleDefs/Tales_Tiberium.xml` — `1FF66B23F09ED3C6F92434AD36D78060E32B7B00AC36156BCFAF115644E72239`
- `_Outdated/Defs/ThoughtDefs/Thoughts_Consumption.xml` — `B4EC9BA02FFEE33306FF3C499C9150B519673FDF33B2C071F6FBC903C4EB39A9`
- `_Outdated/Defs/Misc/Thoughts_Memory_Eating.xml` — `7B62238C9DA6BEE638F8542797AE26D4334A854EE68D5AC39F96A5356E22E394`
- `_Outdated/Defs/Things.Items/Weapons/Forgotten_Bullets.xml` and `_Outdated/Defs/Weapons/Forgotten_Bullets.xml` — byte-identical, `C39CF2A40B90A5F58F855E252CB7E4FA22C51C214704C82C5D4D50FABD9881A2`

The corresponding untracked XML copies under `References/OfficialTiberiumRim-master` were also removed. XML comparison showed they differ from the tracked `Source/Org` versions only by line endings, so `Source/Org` `HEAD` is their recovery payload.

## Reference textures removed after hash verification

These untracked reference textures had byte-identical active counterparts and were removed from `References/OfficialTiberiumRim-master/Textures`:

- `FX/Projectiles/Bullet_TiberiumA.png` — `6E1FF3E790776358818C5BF294B89E9C1F88E86CB14FD6A8BC2E87200DC74C1C`
- `FX/Projectiles/Bullet_TiberiumB.png` — `4116A41D2E921D34810214C10AA9B8385EC4B601EEED4A56FD61E80385E541D3`
- `UI/FactionIcons/Forgotten.png` — `AD9EFB10CB4D1C1A0623B75B3A5BA6B99DC3C1749A9C34A610A05395EBEF9F05`
- `UI/FactionIcons/GDI.png` — `99712FF3EE1A90E6C492C908E9E8BF1784D4DD5383B736156504013782232050`
- `UI/FactionIcons/Nod.png` — `CECFE78AF25532E23EDE9CA5FA4DF2B1F779A2E86001E2C38653105C9B84328C`
- `UI/FactionIcons/Scrin.png` — `140D99CEBBF09864418D09FAE1BF1A5183289011A14301C8F5E60140B137EFB5`

Each hash-identical active file remains under `OfficialTiberiumRim/Textures`, providing complete byte recovery.

## Intentionally retained reference variants

- `Defs/Misc/StatCategoryDefs.xml` contains two unrelated stat categories.
- `Defs/Incidents/Tales_Tiberium.xml` contains the unrelated `TiberiumArrival` tale.
- `Defs/Weaponry/Forgotten_Bullets.xml` carries conflicting projectile values and remains traceable pending an explicit behavioral decision.
- The broader fleck, effecter, gas, natural-building, Soviet-projectile, item-base, and build-menu files contain declarations outside this error batch and remain in place.

## Verification

- Every active XML file parses as XML.
- No duplicate `(XML Def type, defName)` keys exist in the active 1.6 tree.
- All 17 missing cross-reference names from the supplied log now have declarations in the required Def databases.
- The canonical TiberiumRim Release build completed with 0 errors; the deployed DLL/PDB match their build outputs by SHA-256.
