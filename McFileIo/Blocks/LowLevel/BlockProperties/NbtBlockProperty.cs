using fNbt;
using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.LowLevel.BlockProperties
{
    public class NbtBlockProperty : BlockProperty, INbtSnapshot, INbtCustomWriter
    {
        private NbtBlockProperty()
        {
        }

        public NbtCompound NbtSnapshot { get; private set; }

        public NbtBlockProperty(NbtCompound nbt)
        {
            NbtSnapshot = nbt;
        }

        public static NbtBlockProperty CreateFromNbt(string id, NbtCompound compound)
        {
            return new NbtBlockProperty(compound);
        }

        public override object Clone()
        {
            return new NbtBlockProperty((NbtCompound)NbtSnapshot.Clone());
        }

        public override bool Equals(BlockProperty other)
        {
            if (!(other is NbtBlockProperty nbt)) return false;

            return NbtSnapshot.IsSameContentWith(nbt.NbtSnapshot);
        }

        public void Write(IInterpretContext context, NbtCompound activeNode)
        {
            foreach (var it in NbtSnapshot)
                activeNode.Add((NbtTag)it.Clone());
        }
    }
}
