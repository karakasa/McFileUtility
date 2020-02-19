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

        public static WorldData CreateFromRegionDirectory(string directory, CacheStrategy cache = CacheStrategy.UnloadAfterOperation)
        {
            if (!Directory.Exists(directory)) throw new DirectoryNotFoundException();
            var files = Directory.EnumerateFiles(directory, "r.*.*.mca", SearchOption.TopDirectoryOnly).ToArray();
            if (files.Length == 0) throw new FileNotFoundException("No applicable files");

            return new WorldData(new RegionCollection(files, RegionLocateStrategy.FastByName, cache));
        }

        public bool TryGetClassicBlock(int x, int y, int z, out ClassicBlock block)
        {
            var (rx, rz) = RegionFile.GetRegionCoordByWorld(x, z);
            var (cx, cz) = Chunk.GetChunkCoordByWorld(x, z);
            if (!Regions.IsRegionAvailable(rx, rz))
            {
                block = default;
                return false;
            }

            var region = Regions.GetRegionFile(rx, rz);
            var chunk = region.GetChunkData(cx, cz);

            if (chunk is ClassicChunk classic)
            {
                block = classic.GetBlock(x, y, z);
                return true;
            }

            block = default;
            return false;
        }
    }
}
