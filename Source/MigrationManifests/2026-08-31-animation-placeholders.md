# Animation placeholder migration — 2026-08-31

The archived animation sources remain under `Source/Org/_Outdated/` (including the original animation Def XML and serialized frame data). The active 1.6 recovery Defs now contain only minimal `TeleCore.RW.AnimationDataDef` placeholders for `ResearchCrane` and `VeinholeAnimation`, each with an empty `animationSets` list. This intentionally defers visual reimplementation to the current animation system while keeping dependent buildings loadable.
