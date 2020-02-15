using McFileIo.Attributes;
using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    public class Bee : INbtMapperCapable
    {
        [NbtEntry]
        public int MinOccupationTicks;

        [NbtEntry]
        public int TicksInHive;

        // TODO: EntityData to be implemented
    }
}
