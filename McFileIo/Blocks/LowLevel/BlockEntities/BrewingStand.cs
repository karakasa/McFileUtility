using McFileIo.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.LowLevel.BlockEntities
{
    [ApplyTo("brewing_stand")]
    public class BrewingStand : Container
    {
        [NbtEntry]
        public short BrewTime;

        [NbtEntry]
        public byte Fuel;
    }
}
