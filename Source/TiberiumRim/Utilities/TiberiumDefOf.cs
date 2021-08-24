using RimWorld;
using Verse;

namespace TiberiumRim
{
    [DefOf]
    public static class TiberiumDefOf
    {
        //NETWORKS
        public static NetworkDef TiberiumNetwork;
        public static NetworkDef AtmosphericNetwork;

        public static NetworkValueDef TibGreen;
        public static NetworkValueDef TibBlue;
        public static NetworkValueDef TibRed;
        public static NetworkValueDef TibSludge;
        public static NetworkValueDef TibGas;

        //Factions

        //Pawns

        //World Objects
        //public static WorldObjectDef;
        public static WorldObjectDef TiberiumTile;

        //MainButton
        public static TRMainButtonDef TiberiumTab;

        //DesignationCategorx
        public static DesignationCategoryDef Tiberium;

        // TiberiumCrystals
        //Green
        public static TiberiumCrystalDef TiberiumGreen;
        public static TiberiumCrystalDef TiberiumAboreus;
        public static TiberiumCrystalDef TiberiumPod;
        public static TiberiumCrystalDef TiberiumShardsGreen;
        public static TiberiumCrystalDef TiberiumMossGreen;
        //Blue
        public static TiberiumCrystalDef TiberiumBlue;
        public static TiberiumCrystalDef TiberiumPodBlue;
        public static TiberiumCrystalDef TiberiumShardsBlue;
        public static TiberiumCrystalDef TiberiumMossBlue;
        //Red
        public static TiberiumCrystalDef TiberiumRed;
        public static TiberiumCrystalDef TiberiumShardsRed;

        public static TiberiumCrystalDef TiberiumVein;
        public static TiberiumCrystalDef TiberiumGlacier;

        // Plants
        public static ThingDef TiberiumGrass;
        public static ThingDef TiberiumBush;
        public static ThingDef TiberiumShroom_Blue;
        public static ThingDef TiberiumShroom_Yellow;
        public static ThingDef TiberiumShroom_Purple;
        public static ThingDef TiberiumTree;

        public static GameConditionDef TiberiumBiome;
        public static RoomRoleDef TR_AirLock;

        // AllProducers
        public static TRThingDef TiberiumMeteoriteChunk;
        public static TiberiumProducerDef TiberiumCraterGreen;
        public static TiberiumProducerDef TiberiumCraterHybrid;
        public static TiberiumProducerDef TiberiumCraterBlue;
        public static TiberiumProducerDef TiberiumMonolith;
        public static TiberiumProducerDef BlossomTree;
        public static TiberiumProducerDef BlueBlossomTree;
        public static TiberiumProducerDef AlocasiaBlossom;
        public static TiberiumProducerDef SmallBlossom;
        public static TiberiumProducerDef TiberiumCruentus;
        public static TiberiumProducerDef RedTiberiumShard;
        public static TiberiumProducerDef Veinhole;


        public static ThingDef TiberiumGeyser;
        public static ThingDef TiberiumGeyserCrack;

        public static ThingDef VisceralPod;

        //Chunks
        public static ThingDef GreenTiberiumChunk;
        public static ThingDef BlueTiberiumChunk;
        public static ThingDef RedTiberiumChunk;
        public static ThingDef VeinTiberiumChunk;
        public static TiberiumKindDef VeinChunk;

        //Buildings
        public static ThingDef ScrinDronePlatform;
        public static ThingDef TiberiumResearchCrane;
        public static ThingDef TiberiumPipe;

        //Particles
        /*
        public static ParticleDef BlossomSpore;
        public static ParticleDef TiberiumSpore;
        public static ParticleDef TiberiumDustSpore;
        public static ParticleDef TiberiumParticle;
        */

        //Flecks
        public static FleckDef TiberiumAirPuff;
        public static FleckDef TiberiumSmoke;

        //Motes
        public static ThingDef Mote_TiberiumLeak;
        public static ThingDef TiberiumSmokeMote;
        public static ThingDef TiberiumGas;
        public static ThingDef Mote_Beam;
        public static ThingDef Mote_Arc;

        //public static ThingDef FilthTibLiquid;

        //Jobs
        public static JobDef IdleAtRefinery;
        public static JobDef HarvestTiberium;
        public static JobDef UnloadAtRefinery;

        public static JobDef TiberiumResearch;
        public static JobDef UseAirlock;

        public static JobDef TiberiumBill;

        //
        public static RadiationFallOffDef RadiationResistances;

        //
        public static ThingDef PortableContainer;

        //Letter
        public static LetterDef EventLetter;
        public static LetterDef DiscoveryLetter;

        //FleshTypes
        public static FleshTypeDef Mechanical;
        public static FleshTypeDef TiberiumFlesh;

        //StatDefs
        public static StatDef TiberiumInfectionResistance;
        public static StatDef TiberiumGasResistance;
        public static StatDef TiberiumRadiationResistance;
        public static StatDef TiberiumDamageResistance;

        public static StatDef ExtraCarryWeight;

        //Sounds
        public static SoundDef RadiationClick;

        // Skyfallers
        public static ThingDef GreenTiberiumMeteorIncoming;
        public static ThingDef BlueTiberiumMeteorIncoming;
        public static ThingDef RedTiberiumShardIncoming;
        public static ThingDef TiberiumMeteorIncoming;
        public static ThingDef ScrinDronePlatformIncoming;

        //TerrainTags
        public static TerrainFilterDef TerrainFilter_Soil;
        public static TerrainFilterDef TerrainFilter_Sand;
        public static TerrainFilterDef TerrainFilter_Moss;
        //public static TerrainFilterDef TerrainFilter_Water;
        public static TerrainFilterDef TerrainFilter_Stone;

        //Research
        public static TResearchDef MineralAnalysis;
    }
}
