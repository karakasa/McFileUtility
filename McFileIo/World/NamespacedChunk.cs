using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using fNbt;
using McFileIo.Blocks;
using McFileIo.Interfaces;
using McFileIo.Utility;

namespace McFileIo.World
{
    public sealed class NamespacedChunk : Chunk
    {
        public static bool IgnoreBlockProperty = false;

        private const string FieldY = "Y";
        private const string FieldPalette = "Palette";
        private const string FieldBlockStates = "BlockStates";

        private NbtList[] _paletteList = new NbtList[16];
        private long[][] _blockStateRaw = new long[16][];

        private List<NamespacedBlock>[] _palette = new List<NamespacedBlock>[16];
        private IDynBitArray[] _blockStates = new IDynBitArray[16];

        internal NamespacedChunk()
        {
        }

        private static List<NamespacedBlock> ToNamespacedBlockList(NbtList list)
        {
            return new List<NamespacedBlock>(list.OfType<NbtCompound>()
                .Select(c => NamespacedBlock.CreateFromNbt(c, IgnoreBlockProperty)));
        }

        protected override bool GetBlockData(NbtCompound section)
        {
            if (!section.TryGet(FieldY, out NbtByte nbtY)) return false;
            int y = nbtY.Value;

            if (y == 255) return true;
            if (y >= 16) return false;

            if (!section.TryGet(FieldPalette, out NbtList list)) return false;
            if (!section.TryGet(FieldBlockStates, out NbtLongArray blocks)) return false;

            if (SnapshotStrategy == NbtSnapshotStrategy.Enable)
            {
                _paletteList[y] = list;
                _blockStateRaw[y] = blocks.Value;
            }
            else
            {
                _palette[y] = ToNamespacedBlockList(list);
                _blockStates[y] = DynBitArray.CreateFromLongArray(blocks.Value);
            }

            return true;
        }

        private bool EnsureY(int y)
        {
            if (_blockStates[y] == null)
            {
                var longs = _blockStateRaw[y];

                if (longs != null)
                {
                    _palette[y] = ToNamespacedBlockList(_paletteList[y]);
                    _blockStates[y] = DynBitArray.CreateFromLongArray(longs);

                    _paletteList[y] = null;
                    _blockStateRaw[y] = null;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private void EnsureAllYs()
        {
            for (var sy = 0; sy < 16; sy++)
            {
                var y = sy;
                var longs = _blockStateRaw[sy];

                if (longs == null)
                    continue;

                _palette[y] = ToNamespacedBlockList(_paletteList[y]);
                _blockStates[y] = DynBitArray.CreateFromLongArray(longs);

                _paletteList[y] = null;
                _blockStateRaw[y] = null;
            }
        }

        private bool EnsureY(int y, out IDynBitArray bitarray, out List<NamespacedBlock> palette)
        {
            bitarray = _blockStates[y];
            if (bitarray == null)
            {
                var longs = _blockStateRaw[y];
                if (longs != null)
                {
                    palette = _palette[y] = ToNamespacedBlockList(_paletteList[y]);
                    bitarray = _blockStates[y] = DynBitArray.CreateFromLongArray(longs);

                    _paletteList[y] = null;
                    _blockStateRaw[y] = null;
                    return true;
                }
                else
                {
                    bitarray = null;
                    palette = null;
                    return false;
                }
            }

            palette = _palette[y];
            return true;
        }

        public override IEnumerable<int> GetExistingYs()
        {
            for (var y = 0; y < 16; y++)
                if(_blockStates[y] != null)
                yield return y;

            for (var y = 0; y < 16; y++)
                if (_blockStateRaw[y] != null)
                    yield return y;
        }

        public NamespacedBlock GetBlock(int x, int y, int z)
        {
            if (!EnsureY(y >> 4, out var blocks, out var palette))
                return NamespacedBlock.AirBlock;

            var index = blocks[GetBlockIndexByCoord(x, y, z)];
            if (index < 0 || index >= palette.Count)
            {
                ExceptionHelper.ThrowParseError("Palette: Index out of range", ParseErrorLevel.Exception);
                return default;
            }

            return palette[index];
        }
        
        public int GetBlockIndex(int x, int y, int z)
        {
            if (!EnsureY(y >> 4, out var blocks, out _))
                return -1;

            return blocks[GetBlockIndexByCoord(x, y, z)];
        }

        public IEnumerable<(int X, int Y, int Z, NamespacedBlock Block)> AllBlocks()
        {
            EnsureAllYs();

            for (var sy = 0; sy < 16; sy++)
            {
                var blocks = _blockStates[sy];
                if (blocks == null)
                    continue;

                var baseY = sy << 4;
                var index = 0;
                var palette = _palette[sy];

                for (var y = 0; y < 16; y++)
                    for (var z = 0; z < 16; z++)
                        for (var x = 0; x < 16; x++)
                        {
                            yield return (x, y + baseY, z, palette[blocks[index]]);
                            ++index;
                        }
            }
        }

        public IEnumerable<(int X, int Y, int Z, int Index)> AllBlockIndexes()
        {
            EnsureAllYs();

            for (var sy = 0; sy < 16; sy++)
            {
                var blocks = _blockStates[sy];
                if (blocks == null)
                    continue;

                var baseY = sy << 4;
                var index = 0;

                for (var y = 0; y < 16; y++)
                    for (var z = 0; z < 16; z++)
                        for (var x = 0; x < 16; x++)
                        {
                            yield return (x, y + baseY, z, blocks[index]);
                            ++index;
                        }
            }
        }

        public List<NamespacedBlock> GetPalette(int y)
        {
            EnsureY(y);
            return _palette[y];
        }
    }
}
