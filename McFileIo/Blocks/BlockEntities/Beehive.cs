using McFileIo.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
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
