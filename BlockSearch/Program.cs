using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McFileIo.World;
using McFileIo.Utility;
using System.Diagnostics;
using System.Collections.Concurrent;
using McFileIo.Blocks;
using McFileIo.Blocks.BlockEntities;

namespace BlockSearch
{
    class Program
    {
        public static int? val = 5;

        public static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                PrintHelp();
                return;
            }

            var directory = args[0];

            if(directory == "?" || directory == "/?" || directory == "-?" || directory == "help")
            {
                PrintHelp();
                return;
            }

            var cmd = args[1];

            if (cmd.StartsWith("/") || cmd.StartsWith("-"))
                cmd = cmd.Substring(1);

            var world = WorldData.CreateFromRegionDirectory(directory, RegionCollection.CacheStrategy.UnloadAfterOperation);

            switch (cmd)
            {
                case "id":
                    {
                        int x, y, z;
                        x = Convert.ToInt32(args[2]);
                        y = Convert.ToInt32(args[3]);
                        z = Convert.ToInt32(args[4]);

                        if (!world.TryGetClassicBlock(x, y, z, out var block))
                        {
                            Console.WriteLine("Not an id-based block or doesn't exist.");
                            return;
                        }

                        Console.WriteLine($"{block.Id}:{block.Data}");
                        return;
                    }

                case "filter":
                    {
                        CreateFilter(args[2]);
                        
                        var coords = world.Regions.GetRegionCoordinates().ToArray();
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        OptForEach(coords, c =>
                        {
                            foreach (var it in world.Regions.GetRegionFile(c.rx, c.rz).AllChunks(TraverseType.All).OfType<ClassicChunk>())
                                ProcessChunk(it);
                        });

                        stopwatch.Stop();

                        foreach (var (x, y, z) in output.OrderBy(c => c.x).ThenBy(c => c.z).ThenBy(c => c.y))
                        {
                            Console.WriteLine($"{x}, {y}, {z}");
                        }

                        Console.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds / 1000.0}");
                        return;
                    }

                case "blockentity":
                    {
                        var ruleAny = args[2] == "*";

                        var rules = args[2].ToLowerInvariant().Split(',');

                        var coords = world.Regions.GetRegionCoordinates().ToArray();
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        OptForEach(coords, c =>
                        {
                            foreach (var it in world.Regions.GetRegionFile(c.rx, c.rz).AllChunks(TraverseType.All).OfType<ClassicChunk>())
                            {
                                var be = (IEnumerable<BlockEntity>)it.BlockEntities;
                                if (!ruleAny)
                                    be = be.Where(e =>
                                    {
                                        var e2 = e.Id.ToLowerInvariant();
                                        return rules.Any(r => r == e2);
                                    });

                                foreach (var it2 in be)
                                {
                                    outputEntities.Add((it2.X, it2.Y, it2.Z, it2.NbtSnapshot.ToString()));
                                }
                            }
                        });

                        foreach (var (x, y, z, str) in outputEntities.OrderBy(c => c.x).ThenBy(c => c.z).ThenBy(c => c.y))
                        {
                            Console.WriteLine($"{x}, {y}, {z}: {str}");
                        }

                        stopwatch.Stop();
                        Console.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds / 1000.0}");
                        return;
                    }

                case "sign":
                    {
                        var coords = world.Regions.GetRegionCoordinates().ToArray();
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        OptForEach(coords, c =>
                        {
                            foreach (var it in world.Regions.GetRegionFile(c.rx, c.rz).AllChunks(TraverseType.All).OfType<ClassicChunk>())
                            {
                                foreach (var it2 in it.GetBlockEntitiesById(id => id == "Sign" || id == "minecraft:sign").OfType<Sign>())
                                {
                                    outputEntities.Add((it2.X, it2.Y, it2.Z, it2.Text1 + it2.Text2 + it2.Text3 + it2.Text4));
                                }
                            }
                        });

                        foreach (var (x, y, z, str) in outputEntities.OrderBy(c => c.x).ThenBy(c => c.z).ThenBy(c => c.y))
                        {
                            Console.WriteLine($"{x}\t{y}\t{z}\t{str}");
                        }

                        stopwatch.Stop();
                        Console.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds / 1000.0}");
                        return;
                    }

                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }
        }

        private static void OptForEach<T>(ICollection<T> data, Action<T> procedure)
        {
            if(data.Count <= 4 || Environment.ProcessorCount == 1)
            {
                Console.WriteLine("Using regular search...");
                foreach (var it in data)
                    procedure(it);
            }
            else
            {
                Console.WriteLine("Using parallel search...");
                Parallel.ForEach(data, procedure);
            }
        }

        private static ConcurrentBag<(int x, int y, int z, string summary)> outputEntities = new ConcurrentBag<(int x, int y, int z, string summary)>();

        private static void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine();
            Console.WriteLine("blocksearch <region directory> id x y z");
            Console.WriteLine("Identity the Id of a given block");
            Console.WriteLine();
            Console.WriteLine("blocksearch <region directory> filter id1,id2,id3:data3,...");
            Console.WriteLine("Look for block coordinates satisfying Id filters.");
            Console.WriteLine();
            Console.WriteLine("blocksearch <region directory> blockentity id1,id2,...");
            Console.WriteLine("Dump block entitiy info with specific id.");
            Console.WriteLine();
            Console.WriteLine("blocksearch <region directory> sign");
            Console.WriteLine("Dump all signs.");
            Console.WriteLine();
        }

        public static void ProcessChunk(ClassicChunk it)
        {
            var cx = it.X << 4;
            var cz = it.Z << 4;

            foreach(var block in it.AllBlocks())
            {
                if (IsWantedBlock(block.Block))
                    AddBlock(block.Block, block.X + cx, block.Y, block.Z + cz);
            }
        }

        public static List<(int id, bool filterData, int data)> filters = new List<(int, bool, int)>();

        public static void CreateFilter(string filter)
        {
            var rules = filter.Split(',');
            foreach(var rule in rules)
            {
                var index = rule.IndexOf(":");
                if (index != -1)
                {
                    filters.Add((Convert.ToInt32(rule.Substring(0, index)), true, Convert.ToInt32(rule.Substring(index + 1))));
                }
                else
                {
                    filters.Add((Convert.ToInt32(rule), false, 0));
                }
            }
        }

        public static bool IsWantedBlock(ClassicBlock block)
        {
            return filters.Any(f => block.Id == f.id && (!f.filterData || (block.Data == f.data)));
        }

        private static ConcurrentBag<(int x, int y, int z)> output = new ConcurrentBag<(int x, int y, int z)>();

        public static void AddBlock(ClassicBlock block, int x, int y, int z)
        {
            output.Add((x, y, z));
        }
    }
}
