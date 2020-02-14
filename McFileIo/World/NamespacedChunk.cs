using System;
using System.Collections.Generic;
using System.Text;
using fNbt;
using McFileIo.Utility;

namespace McFileIo.World
{
    public class NamespacedChunk : Chunk
    {
        private const string FieldY = "Y";
        private const string FieldPalette = "Palette";
        private const string FieldBlockStates = "BlockStates";

        internal NamespacedChunk()
        {
        }

        protected override bool GetBlockData(NbtCompound section)
        {
            if (!section.TryGet(FieldY, out NbtByte y)) return false;
            if (y.Value == 255) return true;

            if (!section.TryGet(FieldPalette, out NbtList list)) return false;

            // New format with palette and namespaced block ID
            section.TryGet("BlockStates", out NbtLongArray blocks);
            var bytes = EndianHelper.LongArrayToBytes(blocks.Value);
            var bits = new DynBitArray(bytes, blocks.Value.Length / 64);
            var val = bits[3905];

            return true;
        }
    }
}
