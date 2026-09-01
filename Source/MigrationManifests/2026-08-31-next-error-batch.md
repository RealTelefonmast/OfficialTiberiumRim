# Migration manifest: next XML error batch

## Active migrations

- Restored the abstract `VisceralLimb` inheritance node and its complete inherited melee-tool/added-part data in `1.6/Defs/HediffDefs/Hediffs_TiberiumImplants.xml` from `_Outdated/Defs/HediffDefs/Hediffs_TiberiumImplants.xml`.
- Rebound all six mechanical/refinery job drivers to their canonical `TiberiumRim.AI.Jobs` namespace.
- Rebound `EventLetter` to its canonical `TeleCore.RW.Research.Events.EventLetter` runtime type.
- Converted legacy tend durations from ticks to RimWorld 1.6 `baseTendDurationHours` without changing duration: `60000 -> 24`, `120000 -> 48`.
- Converted `TiberiumAddiction.causesNeed` to RimWorld 1.6 stage-level `enablesNeeds`, targeting the actual `TiberiumNeed` def.
- Removed the invalid faction-level `hairTags` field while retaining its legacy `Urban` intent as an inline migration comment. RimWorld 1.6 accepts style tags only on pawn kinds.

## Reference removals after verification

- `_Outdated/Defs/JobDefs/Jobs_Mechs.xml`: all six defs are represented in the active file.
- `_Outdated/Defs/Jobs/Jobs_Mechs.xml`: duplicate two-def subset is represented in the active file.
- `_Outdated/Defs/Misc/LetterDefs.xml`: all three defs are represented in the active file.

The broader implant, Tiberium hediff, and faction reference files remain because they contain declarations outside this error batch. The alternate mutation-specific visceral-parts file also remains pending a declaration-level reconciliation of its custom def type and `isNaturalInsertion` field.

No texture is directly referenced by the handled job, letter, tend-duration, addiction-need, or visceral-limb declarations. `SovjetFaction` uses a vanilla `factionIconPath`, so this batch has no reference texture eligible for removal.

## Recovery payload for removed untracked reference files

The reference tree has no Git recovery history. The complete removed declaration payload is retained here so the pruning operation is reversible.

### `_Outdated/Defs/JobDefs/Jobs_Mechs.xml`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Defs>
  <JobDef><defName>RepairMechanicalPawn</defName><driverClass>TR.JobDriver_RepairDroneRepair</driverClass></JobDef>
  <JobDef><defName>ReturnFromRepair</defName><driverClass>TR.JobDriver_RepairDroneReturn</driverClass></JobDef>
  <JobDef><defName>WanderAtParent</defName><driverClass>TR.JobDriver_WanderAtParent</driverClass></JobDef>
  <JobDef><defName>HarvestTiberium</defName><driverClass>TR.JobDriver_HarvestTiberium</driverClass></JobDef>
  <JobDef><defName>UnloadAtRefinery</defName><driverClass>TR.JobDriver_UnloadAtRefinery</driverClass><reportString>unloading Tiberium.</reportString></JobDef>
  <JobDef><defName>IdleAtRefinery</defName><driverClass>TR.JobDriver_IdleAtRefinery</driverClass><reportString>idling.</reportString></JobDef>
</Defs>
```

### `_Outdated/Defs/Jobs/Jobs_Mechs.xml`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Defs>
  <JobDef><defName>RepairMechanicalPawn</defName><driverClass>TR.JobDriver_RepairDroneRepair</driverClass></JobDef>
  <JobDef><defName>ReturnFromRepair</defName><driverClass>TR.JobDriver_RepairDroneReturn</driverClass></JobDef>
</Defs>
```

### `_Outdated/Defs/Misc/LetterDefs.xml`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Defs>
  <LetterDef><defName>EventLetter</defName><letterClass>TR.EventLetter</letterClass><color>(150,255,150)</color><flashColor>(150,255,150)</flashColor><flashInterval>90</flashInterval></LetterDef>
  <LetterDef><defName>DiscoveryLetter</defName><letterClass>StandardLetter</letterClass><color>(0,255,190)</color><flashColor>(0,100,130)</flashColor><flashInterval>100</flashInterval></LetterDef>
  <LetterDef><defName>TiberiumLetter</defName><letterClass>StandardLetter</letterClass><color>(150,255,150)</color><flashColor>(150,255,150)</flashColor><flashInterval>90</flashInterval></LetterDef>
</Defs>
```
