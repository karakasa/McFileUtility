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
        internal Air() : base(BuiltInUniqueId)
        { 
        }

        public static readonly Guid BuiltInUniqueId = new Guid("{2AA83AEF-8089-4AB1-8010-92F96EFC887A}");
    }
}
