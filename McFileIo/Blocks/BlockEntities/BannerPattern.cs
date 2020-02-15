using McFileIo.Attributes;
using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    public class BannerPattern : INbtMapperCapable
    {
        [NbtEntry]
        public int Color;

        [NbtEntry]
        public string Pattern;
    }
}
