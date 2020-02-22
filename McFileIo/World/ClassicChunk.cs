using System;
using System.Collections.Generic;
using System.Text;
using fNbt;
using McFileIo.Blocks;
using McFileIo.Enum;
using McFileIo.Interfaces;
using McFileIo.Utility;

namespace McFileIo.World
{
    /// <summary>
    /// Stores BlockId-based Anvil chunk (pre 1.13)
    /// </summary>
    public sealed class ClassicChunk : Chunk, IBlockCollection<ClassicBlock>
    {
        private const string FieldY = "Y";
        private const string FieldBlocks = "Blocks";
        private const string FieldAdd = "Add";
        private const string FieldData = "Data";
        private const string FieldBlockLight = "BlockLight";
        private const string FieldSkyLight = "SkyLight";

        private readonly byte[][] _blocks = new byte[16][];
        private readonly byte[][] _data = new byte[16][];
        private readonly byte[][] _add = new byte[16][];
        private readonly byte[][] _skylight = new byte[16][];
        private readonly byte[][] _blocklight = new byte[16][];

        /// <summary>
        /// Creates an empty Id-based chunk
        /// </summary>
        internal ClassicChunk()
        {
        }

        public static ClassicChunk CreateEmpty()
        {
            var chunk = new ClassicChunk();
            chunk.CreateAnew(DataVersion.v1_12_2);
            return chunk;
        }

        protected override bool GetBlockData(NbtCompound section)
        {
            if (!section.TryGet(FieldY, out NbtByte y)) return false;
            if (y.Value == 255) return true;

            if (y.Value >= 16) return false;

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

            if(section.TryGet<NbtByteArray>(FieldBlockLight, out var bl))
                _blocklight[y.Value] = bl.Value;
            else
                _blocklight[y.Value] = new byte[2048];

            if (section.TryGet<NbtByteArray>(FieldSkyLight, out var sl))
                _skylight[y.Value] = sl.Value;
            else
                _skylight[y.Value] = new byte[2048];

            return true;
        }

        /// <summary>
        /// Get all blocks' world coordinates, block Id and block data.
        /// The blocks may not be ordered to maximize performance.
        /// </summary>
        /// <returns>Blocks</returns>
        public IEnumerable<(int X, int Y, int Z, ClassicBlock Block)> AllBlocks()
        {
            for (var sy = 0; sy < 16; sy++)
            {
                var data = _data[sy];
                var add = _add[sy];
                var blocks = _blocks[sy];

                if (blocks == null)
                    continue;

                var dataAvailable = data != null;
                var addAvailable = add != null;
                var baseY = sy << 4;
                var index = 0;
                int blockId, blockData;

                for (var y = 0; y < 16; y++)
                    for (var z = 0; z < 16; z++)
                    {
                        for (var x = 0; x < 16; x += 2)
                        {
                            blockId = addAvailable ? ((EndianHelper.GetHalfIntEvenIndex(add, index) << 8) | blocks[index]) : blocks[index];
                            blockData = dataAvailable ? EndianHelper.GetHalfIntEvenIndex(data, index) : 0;
                            yield return (x, y + baseY, z, new ClassicBlock { Data = blockData, Id = blockId });

                            index += 2;
                        }

                        index -= 15;

                        for (var x = 1; x < 16; x += 2)
                        {
                            blockId = addAvailable ? ((EndianHelper.GetHalfIntOddIndex(add, index) << 8) | blocks[index]) : blocks[index];
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
            var sec = y >> 4;
            var data = 0;
            var blocks = _blocks[sec];
            if (blocks == null) return ClassicBlock.AirBlock;

            var index = GetBlockIndexByCoord(x, y, z);
            var blockId = (int)blocks[index];
            if (_add[sec] != null)
                blockId += (EndianHelper.GetHalfInt(_add[sec], index) << 8);
            if (_data[sec] != null)
                data = EndianHelper.GetHalfInt(_data[sec], index);

            return new ClassicBlock
            {
                Id = blockId,
                Data = data
            };
        }

        public void SetBlock(int x, int y, int z, ClassicBlock block)
        {
            var sec = y >> 4;
            var blocks = _blocks[sec];
            if (blocks == null)
                _blocks[sec] = blocks = new byte[4096];

            var index = GetBlockIndexByCoord(x, y, z);

            blocks[index] = unchecked((byte)(block.Id & 0xff));

            if (block.Id >= 256)
            {
                if (_add[sec] == null)
                    _add[sec] = new byte[2048];

                EndianHelper.SetHalfInt(_add[sec], index, block.Id >> 8);
            }

            if (block.Data != 0)
            {
                if (_data[sec] == null)
                    _data[sec] = new byte[2048];

                EndianHelper.SetHalfInt(_data[sec], index, block.Data);
            }
        }

        public void SetBlock(int x, int y, int z, int blockId)
        {
            var sec = y >> 4;
            var blocks = _blocks[sec];
            if (blocks == null)
                _blocks[sec] = blocks = new byte[4096];

            var index = GetBlockIndexByCoord(x, y, z);

            blocks[index] = unchecked((byte)(blockId & 0xff));

            if (blockId >= 256)
            {
                if (_add[sec] == null)
                    _add[sec] = new byte[2048];

                EndianHelper.SetHalfInt(_add[sec], index, blockId >> 8);
            }
        }

        public override IEnumerable<int> GetExistingYs()
        {
            for (var sy = 0; sy < 16; sy++)
            {
                if (_blocks[sy] != null)
                    yield return sy;
            }
        }

        internal override bool IsAirBlock(int x, int y, int z)
        {
            var sec = y >> 4;
            var blocks = _blocks[sec];
            if (blocks == null) return true;

            var index = GetBlockIndexByCoord(x, y, z);

            if (blocks[index] != 0)
                return false;

            if (_add[sec] != null)
                if (EndianHelper.GetHalfInt(_add[sec], index) != 0)
                    return false;

            return true;
        }

        public void SetBlock(IEnumerable<ChangeBlockRequest> requests, IList<ClassicBlock> customPalette)
        {
            foreach (var rq in requests)
            {
                SetBlock(rq.X, rq.Y, rq.Z, customPalette[rq.InListIndex]);
            }
        }

        private static readonly byte[] emptyLight = new byte[2048];

        protected override void WriteSections()
        {
            if (NbtSnapshot == null) throw new InvalidOperationException();
            var sections = NbtSnapshot.Get<NbtCompound>(FieldLevel).Get<NbtList>(FieldSections);
            sections.Clear();
            sections.ListType = NbtTagType.Compound;

            for (var i = 0; i < 16; i++)
            {
                if (_blocks[i] == null) continue;

                var sec = new NbtCompound()
                {
                    new NbtByte(FieldY, (byte)i),
                    new NbtByteArray(FieldBlocks, _blocks[i])
                };

                if (LightingMode == LightingStrategy.RemoveExisting || (_blocklight[i] == null || _skylight[i] == null))
                {
                    sec.Add(new NbtByteArray(FieldBlockLight, emptyLight));
                    sec.Add(new NbtByteArray(FieldSkyLight, emptyLight));

                }
                else if (LightingMode == LightingStrategy.CopyFromOldData)
                {
                    sec.Add(new NbtByteArray(FieldBlockLight, _blocklight[i]));
                    sec.Add(new NbtByteArray(FieldSkyLight, _skylight[i]));
                }

                if (_add[i] != null)
                    sec.Add(new NbtByteArray(FieldAdd, _add[i]));

                if (_data[i] != null)
                    sec.Add(new NbtByteArray(FieldData, _data[i]));

                sections.Add(sec);
            }
        }

        protected override LightingStrategy DefaultLightingMode => LightingStrategy.CopyFromOldData;
    }
}