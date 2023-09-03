﻿using RimWorld.Planet;
using TeleCore;
using Verse;

namespace TR
{
    public class WorldComponent_TR : WorldComponent
    {
        //Discovery
        public ResearchDiscoveryTable ResearchDiscoveryTable;

        //Infos
        public TiberiumWorldInfo TiberiumInfo;
        public GroundZeroInfo GroundZeroInfo;
        public SuperWeaponInfo SuperWeaponInfo;
        public SatelliteInfo SatelliteInfo;
        public WorldDataInfo WorldDataInfo;
        
        //
        public GameSettingsInfo GameSettings;

        //Incident Locks
        public bool AllowTRInit => GroundZeroInfo.HasGroundZero;
        public bool AllowNewMeteorites => TiberiumDefOf.MineralAnalysis.IsFinished;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref ResearchDiscoveryTable, "ResearchDiscoveryTable");
            Scribe_Deep.Look(ref TiberiumInfo, "TiberiumInfo", world);
            Scribe_Deep.Look(ref GroundZeroInfo, "GroundZeroInfo", world);
            Scribe_Deep.Look(ref SuperWeaponInfo, "SuperWeaponInfo", world);
            Scribe_Deep.Look(ref SatelliteInfo, "SatelliteInfo", world);
            Scribe_Deep.Look(ref GameSettings, "GameSettings", world);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                GenerateInfos();
            }
        }

        private void GenerateInfos()
        {
            ResearchDiscoveryTable ??= new ResearchDiscoveryTable();
            TiberiumInfo ??= new TiberiumWorldInfo(world);
            GroundZeroInfo ??= new GroundZeroInfo(world);
            SuperWeaponInfo ??= new SuperWeaponInfo(world);
            SatelliteInfo ??= new SatelliteInfo(world);
            GameSettings ??= new GameSettingsInfo(world);
            WorldDataInfo ??= new WorldDataInfo(world);
        }

        public WorldComponent_TR(World world) : base(world)
        {
            GenerateInfos();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }

        public override void WorldComponentUpdate()
        {
            SimpleWorldView.Self?.Update();
        }

        public void Notify_RegisterNewObject(GlobalTargetInfo worldObjectOrThing)
        {
            WorldDataInfo.RegisterMapWatcher(worldObjectOrThing);
        }

        public void Notify_TiberiumArrival(Map map)
        {
            TiberiumInfo.SpawnTiberiumTile(map.Tile);
        }
    }
}
