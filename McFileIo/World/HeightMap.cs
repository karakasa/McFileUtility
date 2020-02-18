using fNbt;
using McFileIo.Interfaces;
using McFileIo.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace McFileIo.World
{
    public sealed class HeightMap : INbtIoCapable, INbtCustomReader
    {
        public enum StorageType
        {
            Pre113,
            Post113,
            NotCalculated
        }

        public StorageType State { get; private set; }

        private const string FieldHeightMap = "HeightMap";
        private const string FieldHeightMaps = "Heightmaps";

        public static class TypeString
        {
            public const string LightBlocking = "LIGHT_BLOCKING";
            public const string MotionBlocking = "MOTION_BLOCKING";
            public const string MotionBlockingNoLeaves = "MOTION_BLOCKING_NO_LEAVES";
            public const string OceanFloor = "OCEAN_FLOOR";
            public const string OceanFloorWg = "OCEAN_FLOOR_WG";
            public const string WorldSurface = "WORLD_SURFACE";
            public const string WorldSurfaceWg = "WORLD_SURFACE_WG";
        }

        public enum Type
        {
            Default = -1,
            LightBlocking = 0,
            MotionBlocking = 1,
            MotionBlockingNoLeaves = 2,
            OceanFloor = 3,
            OceanFloorWg = 4,
            WorldSurface = 5,
            WorldSurfaceWg = 6
        }

        public HeightMap()
        {
            State = StorageType.NotCalculated;
        }

        private static readonly string[] HeightMapTypes = new string[] {
            TypeString.LightBlocking, TypeString.MotionBlocking, TypeString.MotionBlockingNoLeaves,
        TypeString.OceanFloor, TypeString.OceanFloorWg, TypeString.WorldSurface, TypeString.WorldSurfaceWg};

        public IDynBitArray[] HeightMaps { get; private set; } = new IDynBitArray[HeightMapTypes.Length];
        public int[] ClassicHeightMap { get; private set; }

        public int GetTypeId(string type)
        {
            for (var i = 0; i < HeightMapTypes.Length; i++)
                if (HeightMapTypes[i].Equals(type))
                    return i;

            return -1;
        }

        public void Read(INbtIoContext context, NbtCompound level)
        {
            var oldversion = level.TryGet<NbtIntArray>(FieldHeightMap, out var heightmap);
            var newversion = level.TryGet<NbtCompound>(FieldHeightMaps, out var heightmaps);

            if (oldversion)
            {
                State = StorageType.Pre113;
                ReadFromClassicFormat(heightmap.Value);
            }
            else if (newversion)
            {
                State = StorageType.Post113;
                ReadFromNewFormat(heightmaps);
            }
            else
            {
                State = StorageType.NotCalculated;
            }
        }

        private void ReadFromClassicFormat(int[] heightmap)
        {
            ClassicHeightMap = heightmap;
        }

        private void ReadFromNewFormat(NbtCompound heightmaps)
        {
            for (var id = 0; id < HeightMapTypes.Length; id++)
            {
                if (heightmaps.TryGet<NbtLongArray>(HeightMapTypes[id], out var longarray))
                    HeightMaps[id] = DynBitArray.CreateFromLongArray(longarray.Value, 9);
            }
        }

        public void Calculate(Chunk chunk)
        {
            var maxHeight = (chunk.GetExistingYs().Max() << 4) + 15;
            var heightMap = new int[256];
            for (var z = 0; z < 16; z++)
                for (var x = 0; x < 16; x++)
                {
                    for (var y = maxHeight; y >= 0; y--)
                    {
                        if (!chunk.IsAirBlock(x, y, z))
                        {
                            heightMap[GetIndexByXZ(x, z)] = y;
                            break;
                        }
                    }
                }

            if (chunk is NamespacedChunk)
            {
                var arr = DynBitArray.CreateEmpty(9, 256);
                for (var i = 0; i < 256; i++)
                    arr[i] = heightMap[i];

                HeightMaps[(int)Type.WorldSurface] = arr;
                State = StorageType.Post113;
            }
            else
            {
                ClassicHeightMap = heightMap;
                State = StorageType.Pre113;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexByXZ(int x, int z)
        {
            return (z << 4) | x;
        }

        public ISequenceAccessor<int> GetAccessor(Type type = Type.Default)
        {
            switch (State)
            {
                case StorageType.Pre113:
                    if (type != Type.LightBlocking && type != Type.Default)
                        throw new NotSupportedException();

                    return new ArraySeqAccessor<int>(ClassicHeightMap);
                case StorageType.Post113:
                    if (type == Type.Default)
                        type = Type.WorldSurface;

                    if (HeightMaps[(int)type] == null)
                        return null;

                    return HeightMaps[(int)type];
                case StorageType.NotCalculated:
                    return null;

                default:
                    throw new NotSupportedException();
            }
        }

        public IEnumerable<(int X, int Z, int Height)> AllHeights(Type type = Type.Default)
        {
            var index = 0;

            switch (State)
            {
                case StorageType.Pre113:
                    if (type != Type.LightBlocking && type != Type.Default)
                        throw new NotSupportedException();

                    for (var z = 0; z < 16; z++)
                        for (var x = 0; x < 16; x++)
                        {
                            yield return (x, z, ClassicHeightMap[index++]);
                        }

                    yield break;

                case StorageType.Post113:
                    if (type == Type.Default)
                        type = Type.WorldSurface;

                    if (HeightMaps[(int)type] == null)
                        yield break;

                    var map = HeightMaps[(int)type];

                    for (var z = 0; z < 16; z++)
                        for (var x = 0; x < 16; x++)
                        {
                            yield return (x, z, map[index++]);
                        }

                    yield break;

                case StorageType.NotCalculated:
                    yield break;

                default:
                    throw new NotSupportedException();
            }
        }

        public int GetAt(int x, int z, Type type = Type.Default)
        {
            switch (State)
            {
                case StorageType.Pre113:
                    if (type != Type.LightBlocking && type != Type.Default)
                        throw new NotSupportedException();

                    return ClassicHeightMap[GetIndexByXZ(x, z)];
                case StorageType.Post113:
                    if (type == Type.Default)
                        type = Type.WorldSurface;

                    if (HeightMaps[(int)type] == null)
                        return -1;

                    return HeightMaps[(int)type][GetIndexByXZ(x, z)];
                case StorageType.NotCalculated:
                    return -1;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
