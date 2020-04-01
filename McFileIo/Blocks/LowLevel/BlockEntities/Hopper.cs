using McFileIo.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.LowLevel.BlockEntities
{
    [ApplyTo("hopper")]
    public class Hopper : LootableContainer
    {
        [NbtEntry]
        public int TransferCooldown;
    }
}
