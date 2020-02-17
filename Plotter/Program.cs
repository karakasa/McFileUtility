using McFileIo.World;
using McFileIo.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace Plotter
{
    class Program
    {
        static void Main(string[] args)
        {
            var cmd = args[0];
            var dumpinfo = cmd == "dumpchunkinfo";
            var heightmap = cmd == "heightmap";

            var regions = args[1];
            string output = null;
            if(!dumpinfo)
                output = args[2];

            var customRange = false;

            for (var i = 3; i < args.Length - 1; i+=2)
            {
                switch(args[i])
                {
                    case "-range":
                        customRange = true;
                        (minX, minZ) = Get2DCoord(args[i + 1]);
                        (maxX, maxZ) = Get2DCoord(args[i + 2]);
                        ++i;
                        break;
                }
            }

            if (dumpinfo) customRange = false;

            var sw = new Stopwatch();

            WorldData world = WorldData.CreateFromRegionDirectory(regions);

            if (!customRange)
            {
                if (!dumpinfo)
                    Console.Write("Analyzing range... ");

                sw.Restart();

                foreach (var it in world.Regions.GetRegionCoordinates())
                {
                    var file = world.Regions.GetRegionFile(it.rx, it.rz, true);

                    var baseX = it.rx << 5;
                    var baseZ = it.rz << 5;

                    foreach (var (CX, CZ, _, _, _) in file.GetInFileMetadata())
                    {
                        AddToRange(CX | baseX, CZ | baseZ);
                        if (dumpinfo)
                        {
                            Console.WriteLine($"{CX | baseX}, {CZ | baseZ}");
                        }
                    }
                }

                sw.Stop();

                if (dumpinfo)
                {
                    return;
                }
                else
                {
                    Console.WriteLine($"done in {sw.ElapsedMilliseconds / 1000.0:0.000} s");
                }
            }

            minChunkX = minX;
            minChunkZ = minZ;
            maxChunkX = maxX;
            maxChunkZ = maxZ;

            minX <<= 4;
            minZ <<= 4;
            maxX <<= 4;
            maxZ <<= 4;

            maxX += 16;
            maxZ += 16;

            Console.WriteLine($"Plot range: {minX}，{minZ} to {maxX}, {maxZ}. continue? y/n");
            if (Console.ReadLine() != "y") return;

            Console.Write("Plotting... ");

            sw.Restart();

            using (var bitmap = new DirectBitmap(maxX - minX, maxZ - minZ))
            {
                Parallel.ForEach(world.Regions.AllChunks(TraverseType.All), it =>
                {
                    if (customRange)
                    {
                        if (it.X > maxChunkX || it.X < minChunkX || it.Z > maxChunkZ || it.Z < minChunkZ)
                            return;
                    }

                    var baseX = (it.X << 4) - minX;
                    var baseZ = (it.Z << 4) - minZ;

                    var map = it.HeightMap;
                    if (map.State == HeightMap.StorageType.NotCalculated)
                    {
                        return;
                    }

                    foreach (var height in map.AllHeights())
                    {
                        var v = height.Height & 0xff;

                        var color = Color.FromArgb(v, v, v);

                        bitmap.SetPixel(baseX + height.X, baseZ + height.Z, color);

                        //var heightV = it.HeightMap.GetAt(height.X, height.Z);
                        // var block = it.GetBlock(height.X, height.Height + 1, height.Z);
                        //if (!IsTransparentId(block.Id))
                        //return;
                    }
                });

                bitmap.Bitmap.Save(output, ImageFormat.Png);
            }

            sw.Stop();
            Console.WriteLine($"done in {sw.ElapsedMilliseconds / 1000.0:0.000} s");

#if DEBUG
            Console.Read();
#endif
        }

        private static (int, int) Get2DCoord(string coordStr)
        {
            var coord = coordStr.Split(',');
            return (Convert.ToInt32(coord[0]), Convert.ToInt32(coord[1]));
        }

        private static bool IsTransparentId(int blockId)
        {
            return blockId == 0 || blockId == 20 || blockId == 95 || blockId == 102 || blockId == 160 || blockId == 31 || blockId == 175 || blockId == 11 || blockId == 65 || blockId == 83;
        }

        private static int minX = int.MaxValue;
        private static int minZ = int.MaxValue;
        private static int maxX = int.MinValue;
        private static int maxZ = int.MinValue;

        private static int minChunkX = int.MaxValue;
        private static int minChunkZ = int.MaxValue;
        private static int maxChunkX = int.MaxValue;
        private static int maxChunkZ = int.MaxValue;

        private static void AddToRange(int x, int z)
        {
            if (x < minX) minX = x;
            if (x > maxX) maxX = x;

            if (z < minZ) minZ = z;
            if (z > maxZ) maxZ = z;
        }
    }
}
