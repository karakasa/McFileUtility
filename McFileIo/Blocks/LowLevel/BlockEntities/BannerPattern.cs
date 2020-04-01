using McFileIo.Attributes;
using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.LowLevel.BlockEntities
{
    public class BannerPattern : INbtIoCapable
    {
        [NbtEntry]
        public int Color;

        [NbtEntry]
        public string Pattern;
    }
}
