using fNbt;
using McFileIo.Attributes;
using McFileIo.Blocks.LowLevel.BlockProperties;
using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace McFileIo.Blocks.LowLevel
{
    public sealed class NamespacedBlock : INbtIoCapable, INbtCustomReader,
        IEquatable<NamespacedBlock>, ICloneable, INbtCustomWriter
    {
        public const string IdAirBlock = "minecraft:air";

        public static readonly NamespacedBlock AirBlock = new NamespacedBlock(IdAirBlock);

        public override string ToString()
        {
            if (ContainsProperty())
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

            // TODO: BlockProperty

            if (property != null)
                Properties = NbtBlockProperty.CreateFromNbt(Name, property);

            return true;
        }

        public object Clone()
        {
            if (Properties == null)
                return new NamespacedBlock(Name);
            else
                return new NamespacedBlock(Name, (BlockProperty)Properties.Clone());
        }

        public bool Equals(NamespacedBlock other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is NamespacedBlock nsb)) return false;
            return nsb == this;
        }

        public override int GetHashCode()
        {
            var hashCode = -1578535950;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<BlockProperty>.Default.GetHashCode(Properties);
            return hashCode;
        }

        public void Write(INbtIoContext context, NbtCompound activeNode)
        {
            activeNode.Add(new NbtString(nameof(Name), Name));
            if (Properties != null)
            {
                var prop = new NbtCompound(nameof(Properties));

                // TODO: Workaround. Should replace with NbtIoWriter implementation
                if (Properties is INbtCustomWriter customwriter)
                    customwriter.Write(context, activeNode);

                activeNode.Add(prop);
            }
        }

        public static bool operator ==(NamespacedBlock a, NamespacedBlock b)
        {
            if (a is null) return b is null;
            if (b is null) return a is null;

            if (ReferenceEquals(a, b)) return true;

            if (a.Name != b.Name) return false;

            if (a.Properties is null) return b.Properties is null;
            if (b.Properties is null) return a.Properties is null;

            return a.Properties.Equals(b.Properties);
        }

        public static bool operator !=(NamespacedBlock a, NamespacedBlock b)
        {
            return !(a == b);
        }
    }
}
