using System;
using System.Collections.Generic;
using System.Text;
using fNbt;
using McFileIo.Blocks;
using McFileIo.Interfaces;
using McFileIo.Utility;

namespace McFileIo.World
{
    /// <summary>
    /// Stores BlockId-based chunk (pre 1.13)
    /// </summary>
    public sealed class ClassicChunk : Chunk
    {
        private const string FieldY = "Y";
        private const string FieldBlocks = "Blocks";
        private const string FieldAdd = "Add";
        private const string FieldData = "Data";

        private readonly Dictionary<int, byte[]> _blocks = new Dictionary<int, byte[]>();
        private readonly Dictionary<int, byte[]> _data = new Dictionary<int, byte[]>();
        private readonly Dictionary<int, byte[]> _add = new Dictionary<int, byte[]>();

        internal ClassicChunk()
        {
        }

        protected override bool GetBlockData(NbtCompound section)
        {
            if (!section.TryGet(FieldY, out NbtByte y)) return false;
            if (y.Value == 255) return true;
            // Old format with numeric block ID
            if (!section.TryGet(FieldBlocks, out NbtByteArray blocks)) return false;
            if (blocks.Value.Length != 4096) return false;
            _blocks[y.Value] = blocks.Value;

            var addSuccess = section.TryGet(FieldAdd, out NbtByteArray add);
            if (addSuccess && add.Value.Length != 2048) return false;
            if (addSuccess) _add[y.Value] = add.Value;

            var dataSuccess = section.TryGet(FieldData, out NbtByteArray data);
            if (dataSuccess && data.Value.Length != 2048) return false;
            if (dataSuccess) _data[y.Value] = data.Value;

            return true;
        }

        private static readonly ClassicBlock AirBlock = new ClassicBlock() { Id = 0, Data = 0 };

        /// <summary>
        /// Get all blocks' world coordinates, block Id and block data.
        /// The blocks may not be ordered to maximize performance.
        /// </summary>
        /// <returns>Blocks</returns>
        public IEnumerable<(int x, int y, int z, ClassicBlock block)> AllBlocks()
        {
            foreach (var it in _blocks)
            {
                var dataAvailable = _data.TryGetValue(it.Key, out var data);
                var addAvailable = _add.TryGetValue(it.Key, out var add);
                var baseY = it.Key << 4;
                var index = 0;
                int blockId, blockData;

                for (var y = 0; y < 16; y++)
                    for (var z = 0; z < 16; z++)
                    {
                        for (var x = 0; x < 16; x += 2)
                        {
                            blockId = addAvailable ? ((EndianHelper.GetHalfIntEvenIndex(add, index) << 8) | it.Value[index]) : it.Value[index];
                            blockData = dataAvailable ? EndianHelper.GetHalfIntEvenIndex(data, index) : 0;
                            yield return (x, y + baseY, z, new ClassicBlock { Data = blockData, Id = blockId });

                            index += 2;
                        }

                        index -= 15;

                        for (var x = 1; x < 16; x += 2)
                        {
                            blockId = addAvailable ? ((EndianHelper.GetHalfIntOddIndex(add, index) << 8) | it.Value[index]) : it.Value[index];
                            blockData = dataAvailable ? EndianHelper.GetHalfIntOddIndex(data, index) : 0;
                            yield return (x, y + baseY, z, new ClassicBlock { Data = blockData, Id = blockId });

                            index += 2;
                        }

                        index -= 1;
                    }
            }
        }

        /// <summary>
        /// Get block's Id and data at a given coordinate
        /// </summary>
        /// <param name="x">World X</param>
        /// <param name="y">World Y</param>
        /// <param name="z">World Z</param>
        /// <returns>Block</returns>
        public ClassicBlock GetBlock(int x, int y, int z)
        {
            var sec = y / 16;
            var data = 0;
            if (!_blocks.TryGetValue(sec, out var blocks)) return AirBlock;

            var index = GetBlockIndexByCoord(x, y, z);
            var blockId = (int)blocks[index];
            if (_add.TryGetValue(sec, out var add))
                blockId += (EndianHelper.GetHalfInt(add, index) << 8);
            if (_data.TryGetValue(sec, out var damage))
                data = EndianHelper.GetHalfInt(damage, index);

            return new ClassicBlock
            {
                Id = blockId,
                Data = data
            };
        }

        /// <summary>
        /// Returns existing Y section indexes.
        /// </summary>
        /// <returns>Ys</returns>
        public IEnumerable<int> GetExistingYs()
        {
            return _blocks.Keys;
        }
    }
}