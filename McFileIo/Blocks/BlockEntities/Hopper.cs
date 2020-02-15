using McFileIo.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    [ApplyTo("hopper")]
    public class Hopper : LootableContainer
    {
        [NbtEntry]
        public int TransferCooldown;
    }
}
