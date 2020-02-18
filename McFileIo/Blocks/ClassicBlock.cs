using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace McFileIo.Blocks
{
    public struct ClassicBlock : IEquatable<ClassicBlock>
    {
        public override string ToString()
        {
            return $"Block {Id}:{Data}";
        }

        public ClassicBlock(int id, int data = 0)
        {
            Id = id;
            Data = data;
        }

        public int Id;
        public int Data;

        public static readonly ClassicBlock AirBlock = new ClassicBlock() { Id = 0, Data = 0 };

        public static bool operator== (ClassicBlock a, ClassicBlock b)
        {
            return a.Id == b.Id && a.Data == b.Data;
        }

        public static bool operator!=(ClassicBlock a, ClassicBlock b)
        {
            return a.Id != b.Id || a.Data != b.Data;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(ClassicBlock other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return (Id << 4) | Data;
        }
    }
}
