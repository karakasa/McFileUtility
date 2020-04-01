using McFileIo.Attributes;
using McFileIo.Interfaces.McObject;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.LowLevel.BlockEntities
{
    [ApplyTo("banner")]
    public class Banner : BlockEntity, ICustomNameCapable
    {
        [NbtEntry]
        public string CustomName { get; set; }

        [NbtEntry]
        public List<BannerPattern> Patterns = null;
    }
}
