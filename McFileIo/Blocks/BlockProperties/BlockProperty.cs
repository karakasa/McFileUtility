using fNbt;
using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockProperties
{
    public class BlockProperty : INbtIoCapable, INbtSnapshot
    {
        private BlockProperty()
        {
        }

        public NbtCompound NbtSnapshot { get; private set; }

        public BlockProperty(NbtCompound nbt)
        {
            NbtSnapshot = nbt;
        }

        public static BlockProperty CreateFromNbt(string id, NbtCompound compound)
        {
            return new BlockProperty(compound);
        }
    }
}
