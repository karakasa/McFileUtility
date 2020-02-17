using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace McFileIo.Blocks
{
    public struct ClassicBlock
    {
        public override string ToString()
        {
            return $"Block {Id}:{Data}";
        }

        public int Id;
        public int Data;

        public static readonly ClassicBlock AirBlock = new ClassicBlock() { Id = 0, Data = 0 };
    }
}
