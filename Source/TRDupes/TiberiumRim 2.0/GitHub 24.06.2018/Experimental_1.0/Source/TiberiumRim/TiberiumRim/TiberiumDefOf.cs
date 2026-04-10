using Verse;
using RimWorld;

namespace TiberiumRim
{
    [DefOf]
    public static class TiberiumDefOf
    {
        //Jobs
        public static JobDef DoMissionObjective = DefDatabase<JobDef>.GetNamed("DoMissionObjective", true);

        public static JobDef RepairMechanicalPawn = DefDatabase<JobDef>.GetNamed("RepairMechanicalPawn", true);

        public static JobDef WanderAtParent = DefDatabase<JobDef>.GetNamed("WanderAtParent", true);

        public static JobDef SearchAndHarvestTiberium = DefDatabase<JobDef>.GetNamed("SearchAndHarvestTiberium", true);

        public static JobDef ReturnToRefineryToUnload = DefDatabase<JobDef>.GetNamed("ReturnToRefineryToUnload", true);

        public static JobDef ReturnToRefineryToIdle = DefDatabase<JobDef>.GetNamed("ReturnToRefineryToIdle", true);

        public static JobDef FormAmalgamation = DefDatabase<JobDef>.GetNamed("FormAmalgamation", true);

        public static JobDef TiberiumBath = DefDatabase<JobDef>.GetNamed("TiberiumBath", true);
        //Terrain
        public static TerrainDef TiberiumSoilGreen = TerrainDef.Named("TiberiumSoilGreen");

        public static TerrainDef TiberiumSoilPod = TerrainDef.Named("TiberiumSoilPod");

        public static TerrainDef TiberiumSoilBlue = TerrainDef.Named("TiberiumSoilBlue");

        public static TerrainDef TiberiumSoilRed = TerrainDef.Named("TiberiumSoilRed");

        public static TerrainDef TiberiumSoilVein = TerrainDef.Named("TiberiumSoilVein");

        public static TerrainDef TiberiumShallowWater = TerrainDef.Named("TiberiumShallowWater");

        public static TerrainDef TiberiumDeepWater = TerrainDef.Named("TiberiumDeepWater");

        public static TerrainDef TiberiumIce = TerrainDef.Named("TiberiumIce");

        public static TerrainDef TiberiumRockGreen = TerrainDef.Named("TiberiumRockGreen");

        public static TerrainDef TiberiumRockBlue = TerrainDef.Named("TiberiumRockBlue");

        public static TerrainDef TiberiumSandGreen = TerrainDef.Named("TiberiumSandGreen");

        public static TerrainDef TiberiumSandBlue = TerrainDef.Named("TiberiumSandBlue");

        public static TerrainDef TiberiumSandRed = TerrainDef.Named("TiberiumSandRed");

        public static TerrainDef DecrystallizedSoil = TerrainDef.Named("DecrystallizedSoil");

        public static TerrainDef DecrystallizedSand = TerrainDef.Named("DecrystallizedSand");

        // Plants
        public static ThingDef TiberiumGrass = ThingDef.Named("TiberiumGrass");

        public static ThingDef TiberiumBush = ThingDef.Named("TiberiumBush");

        public static ThingDef TiberiumShroom = ThingDef.Named("TiberiumShroom");

        public static ThingDef TiberiumTree = ThingDef.Named("TiberiumTree");

        //Crystals
        public static TiberiumCrystalDef TiberiumGreen = TiberiumCrystalDef.Named("TiberiumGreen");

        public static TiberiumCrystalDef TiberiumBlue = TiberiumCrystalDef.Named("TiberiumBlue");

        public static TiberiumCrystalDef TiberiumRed = TiberiumCrystalDef.Named("TiberiumRed");

        public static TiberiumCrystalDef TiberiumShardsGreen = TiberiumCrystalDef.Named("TiberiumShardsGreen");

        public static TiberiumCrystalDef TiberiumShardsBlue = TiberiumCrystalDef.Named("TiberiumShardsBlue");

        public static TiberiumCrystalDef TiberiumShardsRed = TiberiumCrystalDef.Named("TiberiumShardsRed");

        public static TiberiumCrystalDef TiberiumMossGreen = TiberiumCrystalDef.Named("TiberiumMossGreen");

        public static TiberiumCrystalDef TiberiumMossBlue = TiberiumCrystalDef.Named("TiberiumMossBlue");

        public static TiberiumCrystalDef TiberiumPod = TiberiumCrystalDef.Named("TiberiumPod");

        public static TiberiumCrystalDef TiberiumVein = TiberiumCrystalDef.Named("TiberiumVein");

        public static TiberiumCrystalDef TiberiumGlacier = TiberiumCrystalDef.Named("TiberiumGlacier");

        //Producers
        public static ThingDef TiberiumCraterGreen_TBNS = ThingDef.Named("TiberiumCraterGreen_TBNS");

        public static ThingDef BlossomTree_TBNS = ThingDef.Named("BlossomTree_TBNS");

        public static ThingDef TiberiumCraterBlue_TBNS = ThingDef.Named("TiberiumCraterBlue_TBNS");

        public static ThingDef TiberiumMonolith_TBNS = ThingDef.Named("TiberiumMonolith_TBNS");

        public static ThingDef TiberiumShard_TBNS = ThingDef.Named("TiberiumShard_TBNS");

        public static ThingDef Veinhole_TBNS = ThingDef.Named("Veinhole_TBNS");


        //Harmless
        public static ThingDef TiberiumMeteor_TBNS = ThingDef.Named("TiberiumMeteor_TBNS");

        public static ThingDef TiberiumTower_TBNS = ThingDef.Named("TiberiumTower_TBNS");

        //
    }
}
