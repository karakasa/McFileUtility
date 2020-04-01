using McFileIo.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.LowLevel.BlockEntities
{
    [ApplyTo("beehive")]
    public class Beehive : BlockEntity
    {
        [NbtEntry]
        public Coordinate FlowerPos;

        [NbtEntry]
        public List<Bee> Bees;
    }
}
