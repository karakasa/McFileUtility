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
        private const string FieldY = "Y";
        private const string FieldPalette = "Palette";
        private const string FieldBlockStates = "BlockStates";

        private Dictionary<int, NbtList> _paletteList = new Dictionary<int, NbtList>();
        private Dictionary<int, long[]> _blockStateRaw = new Dictionary<int, long[]>();

        private Dictionary<int, List<PaletteBlock>> _palette = new Dictionary<int, List<PaletteBlock>>();
        private Dictionary<int, IDynBitArray> _blockStates = new Dictionary<int, IDynBitArray>();

        internal NamespacedChunk()
        {
        }

        protected override bool GetBlockData(NbtCompound section)
        {
            if (!section.TryGet(FieldY, out NbtByte nbtY)) return false;
            if (nbtY.Value == 255) return true;

            int y = nbtY.Value;

            if (!section.TryGet(FieldPalette, out NbtList list)) return false;
            if (!section.TryGet(FieldBlockStates, out NbtLongArray blocks)) return false;

            if (SnapshotStrategy == NbtSnapshotStrategy.Enable)
            {
                _paletteList[y] = list;
                _blockStateRaw[y] = blocks.Value;
            }
            else
            {
                _palette[y] = list.ToFrameworkList<PaletteBlock>();
                _blockStates[y] = DynBitArray.CreateFromLongArray(blocks.Value);
            }

            return true;
        }

        private bool EnsureY(int y)
        {
            if (!_blockStates.ContainsKey(y))
            {
                if (_blockStateRaw.TryGetValue(y, out var longs))
                {
                    _palette[y] = _paletteList[y].ToFrameworkList<PaletteBlock>();
                    _blockStates[y] = DynBitArray.CreateFromLongArray(longs);

                    _paletteList.Remove(y);
                    _blockStateRaw.Remove(y);
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
            foreach (var it in _blockStateRaw)
            {
                var y = it.Key;
                var longs = it.Value;

                _palette[y] = _paletteList[y].ToFrameworkList<PaletteBlock>();
                _blockStates[y] = DynBitArray.CreateFromLongArray(longs);
            }

            _paletteList.Clear();
            _blockStateRaw.Clear();
        }

        private bool EnsureY(int y, out IDynBitArray bitarray, out List<PaletteBlock> palette)
        {
            if (!_blockStates.TryGetValue(y, out bitarray))
            {
                if (_blockStateRaw.TryGetValue(y, out var longs))
                {
                    palette = _palette[y] = _paletteList[y].ToFrameworkList<PaletteBlock>();
                    bitarray = _blockStates[y] = DynBitArray.CreateFromLongArray(longs);

                    _paletteList.Remove(y);
                    _blockStateRaw.Remove(y);
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
            foreach (var it in _blockStates.Keys)
                yield return it;

            foreach (var it in _blockStateRaw.Keys)
                yield return it;
        }

        public PaletteBlock GetBlock(int x, int y, int z)
        {
            if (!EnsureY(y >> 4, out var blocks, out var palette))
                return PaletteBlock.AirBlock;

            var index = blocks[GetBlockIndexByCoord(x, y, z)];
            if (index < 0 || index >= palette.Count)
            {
                ExceptionHelper.ThrowParseError("Palette: Index out of range", ParseErrorLevel.Exception);
                return null;
            }

            return palette[index];
        }
        
        public int GetBlockIndex(int x, int y, int z)
        {
            if (!EnsureY(y >> 4, out var blocks, out _))
                return -1;

            return blocks[GetBlockIndexByCoord(x, y, z)];
        }

        public IEnumerable<(int x, int y, int z, PaletteBlock block)> AllBlocks()
        {
            EnsureAllYs();

            foreach (var it in _blockStates)
            {
                var baseY = it.Key << 4;
                var index = 0;
                var palette = _palette[it.Key];

                for (var y = 0; y < 16; y++)
                    for (var z = 0; z < 16; z++)
                        for (var x = 0; x < 16; x++)
                        {
                            yield return (x, y + baseY, z, palette[it.Value[index]]);
                            ++index;
                        }
            }
        }

        public IEnumerable<(int x, int y, int z, int index)> AllBlockIndexes()
        {
            EnsureAllYs();

            foreach (var it in _blockStates)
            {
                var baseY = it.Key << 4;
                var index = 0;

                for (var y = 0; y < 16; y++)
                    for (var z = 0; z < 16; z++)
                        for (var x = 0; x < 16; x++)
                        {
                            yield return (x, y + baseY, z, it.Value[index]);
                            ++index;
                        }
            }
        }
    }
}
