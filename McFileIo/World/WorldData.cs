using McFileIo.Blocks;
using McFileIo.Enum;
using McFileIo.Interfaces;
using McFileIo.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static McFileIo.World.RegionCollection;

namespace McFileIo.World
{
    public class WorldData
    {
        public RegionCollection Regions { get; private set; }

        internal WorldData(RegionCollection regions)
        {
            Regions = regions;
        }

        public static WorldData CreateFromRegions(RegionCollection reader)
        {
            // Factory method for future compatibility
            return new WorldData(reader);
        }
    }
}
