using McFileIo.Attributes;
using McFileIo.Interfaces.McObject;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.LowLevel.BlockEntities
{
    [ApplyTo("enchanting_table")]
    public class EnchantingTable : BlockEntity, ICustomNameCapable
    {
        [NbtEntry(Optional: true)]
        public string CustomName { get; set; } = null;
    }
}
