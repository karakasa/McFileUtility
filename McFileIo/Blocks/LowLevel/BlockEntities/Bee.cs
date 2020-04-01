using McFileIo.Attributes;
using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.LowLevel.BlockEntities
{
    public class Bee : INbtIoCapable
    {
        [NbtEntry]
        public int MinOccupationTicks;

        [NbtEntry]
        public int TicksInHive;

        // TODO: EntityData to be implemented
    }
}
