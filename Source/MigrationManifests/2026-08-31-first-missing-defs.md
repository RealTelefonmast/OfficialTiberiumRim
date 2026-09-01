# First missing-def migration (2026-08-31)

The following reference definitions were removed after their declaration/data surface was migrated to the active 1.6 mod and XML/build validation passed:

- `_Outdated/Defs/TiberiumCrystals/ThingFilters.xml`
- `_Outdated/Defs/TerrainDefs/Terrain_Filters.xml`
- `_Outdated/Defs/Research/Discoveries.xml`
- `_Outdated/Defs/FX/Flecks/Flecks_WeaponFX.xml`
- `_Outdated/Defs/HediffDefs/Damages/Hediffs_Damage.xml`
- `_Outdated/Defs/Hediffs/Damages/Hediffs_Damage.xml` (duplicate source variant)
- `_Outdated/Defs/DamageDefs/Armor/DamageArmorCategories.xml`
- `_Outdated/Defs/StatDefs/StatDef_Armor.xml`
- `_Outdated/Defs/Filth/Filth_Tiberium.xml`

The reference `Textures/Tiberium/Filth` subtree contained 21 files, all with byte-identical counterparts at the same relative paths in the active mod. It was removed after hash verification. Unique SHA-256 payloads retained in the active mod:

- Blue dust 1: `9493DB3CFCBA4278127790FDE366A6C3EA7B99786A958A9C2853549984AFDCA6`
- Blue dust 2: `D710CD6EAB27C0A5F5CDA2B066B7C52836CAE1E1DC0230A05A5DC9159A96F715`
- Blue crystal overlay: `88066BB1A31D230B839062D04075ECBCC2C6617D0B4EE7668CF98D4A3BC29FAC`
- Green dust 1: `56685F0EBA72AA5DA0B0084DBA6EAA6A887D763F7A01DCBF3E439EBA954EEF9E`
- Green dust 2: `C231AA5A943016775C6E358DA6634F93A6733B9DD2266E3BE85250639402D935`
- Green overlay: `5BF21233BBAC9FE63D665A1475110319541717A863FF543B70A3EFCC5B0F86AB`
- Green crystal overlay: `650E48DFD612A29FC215A533132F059E7C6EB6A966567469B27E2E0436AA95BB`
- Red dust 1: `B28045DCB171F5B0BA7B956CA469B2986FE80033F2C690AB26075E78DC767B3A`
- Red dust 2: `6FD55F9477388D8B6DEEEA11669771122A4A39B368DDF7BD8F39464A2FB00D69`

Historical conversion rulesets, partial mote/item/menu families, and their assets remain in the reference project because they have not yet been migrated in full.
