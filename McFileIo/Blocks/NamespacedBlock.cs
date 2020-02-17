using fNbt;
using McFileIo.Attributes;
using McFileIo.Blocks.BlockProperties;
using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace McFileIo.Blocks
{
    public class NamespacedBlock : INbtIoCapable, INbtCustomReader
    {
        public override string ToString()
        {
            if(ContainsProperty())
                return $"Block {Name} + Prop";
            else
                return $"Block {Name}";
        }

        public NamespacedBlock()
        {
        }

        public NamespacedBlock(string id, BlockProperty property = null)
        {
            Name = id;
            Properties = property;
        }

        public static NamespacedBlock CreateFromNbt(NbtCompound node, bool ignoreProperty = false)
        {
            var block = new NamespacedBlock();
            if (!block.CustomRead(node, ignoreProperty)) return null;
            return block;
        }

        [NbtEntry]
        public string Name;

        [NbtEntry(Optional: true)]
        public BlockProperty Properties = null;

        public bool ContainsProperty() => Properties != null;

        public void Read(INbtIoContext context, NbtCompound node)
        {
            CustomRead(node, false);
        }

        private bool CustomRead(NbtCompound node, bool ignoreProperty)
        {
            if (!node.TryGet<NbtString>(nameof(Name), out var str)) return false;

            NbtCompound property = null;
            if (!ignoreProperty)
                node.TryGet(nameof(Properties), out property);

            Name = str.Value;

            if (property != null)
                Properties = BlockProperty.CreateFromNbt(Name, property);

            return true;
        }

        public static readonly NamespacedBlock AirBlock = new NamespacedBlock("minecraft:air");
    }
}
