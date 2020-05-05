using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public static class GenTiberiumPatterns
    {
        public static IntVec3[] TiberiumPattern_Lattice;
        public static IntVec3[] TiberiumPattern_Glacier;
        public static IntVec3[] TiberiumPattern_Pod;
        public static IntVec3[] TiberiumPattern_Shards;


        static GenTiberiumPatterns()
        {
            Set_Lattice();
            Set_Glacier();
            Set_Pod();
            Set_Shards();
        }

        private static void Set_Lattice()
        {
            TiberiumPattern_Lattice = new IntVec3[8];
            TiberiumPattern_Lattice[0] = new IntVec3( 0, 0, 1);
            TiberiumPattern_Lattice[1] = new IntVec3( 1, 0, 0);
            TiberiumPattern_Lattice[2] = new IntVec3( 0, 0,-1);
            TiberiumPattern_Lattice[3] = new IntVec3(-1, 0, 0);
            TiberiumPattern_Lattice[4] = new IntVec3( 1, 0,-1);
            TiberiumPattern_Lattice[5] = new IntVec3( 1, 0, 1);
            TiberiumPattern_Lattice[6] = new IntVec3(-1, 0, 1);
            TiberiumPattern_Lattice[7] = new IntVec3(-1, 0,-1);
        }

        private static void Set_Glacier()
        {
            TiberiumPattern_Glacier = new IntVec3[8];
            TiberiumPattern_Glacier[0] = new IntVec3( 0, 0, 1);
            TiberiumPattern_Glacier[1] = new IntVec3( 1, 0, 0);
            TiberiumPattern_Glacier[2] = new IntVec3( 0, 0,-1);
            TiberiumPattern_Glacier[3] = new IntVec3(-1, 0, 0);
            TiberiumPattern_Glacier[4] = new IntVec3( 1, 0,-1);
            TiberiumPattern_Glacier[5] = new IntVec3( 1, 0, 1);
            TiberiumPattern_Glacier[6] = new IntVec3(-1, 0, 1);
            TiberiumPattern_Glacier[7] = new IntVec3(-1, 0,-1);
        }

        private static void Set_Pod()
        {
            TiberiumPattern_Pod = new IntVec3[8];
            TiberiumPattern_Pod[0] = new IntVec3( 0, 0, 1);
            TiberiumPattern_Pod[1] = new IntVec3( 1, 0, 0);
            TiberiumPattern_Pod[2] = new IntVec3( 0, 0,-1);
            TiberiumPattern_Pod[3] = new IntVec3(-1, 0, 0);
            TiberiumPattern_Pod[4] = new IntVec3( 1, 0,-1);
            TiberiumPattern_Pod[5] = new IntVec3( 1, 0, 1);
            TiberiumPattern_Pod[6] = new IntVec3(-1, 0, 1);
            TiberiumPattern_Pod[7] = new IntVec3(-1, 0,-1);
        }

        private static void Set_Shards()
        {
            TiberiumPattern_Shards = new IntVec3[8];
            TiberiumPattern_Shards[0] = new IntVec3( 0, 0, 1);
            TiberiumPattern_Shards[1] = new IntVec3( 1, 0, 0);
            TiberiumPattern_Shards[2] = new IntVec3( 0, 0,-1);
            TiberiumPattern_Shards[3] = new IntVec3(-1, 0, 0);
            TiberiumPattern_Shards[4] = new IntVec3( 1, 0,-1);
            TiberiumPattern_Shards[5] = new IntVec3( 1, 0, 1);
            TiberiumPattern_Shards[6] = new IntVec3(-1, 0, 1);
            TiberiumPattern_Shards[7] = new IntVec3(-1, 0,-1);
        }

    }
}
