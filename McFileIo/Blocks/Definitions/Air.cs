using McFileIo.Blocks.LowLevel;
using McFileIo.Blocks.LowLevel.BlockEntities;
using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.Definitions
{
    public class Air : Block
    {
        public Air() : base(0, NamespacedBlock.IdAirBlock)
        {
        }
    }
}
