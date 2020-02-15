using System;
using System.Collections.Generic;
using System.Text;
using McFileIo.Attributes;
using McFileIo.Interfaces;

namespace McFileIo.Misc
{
    public class NbtUuid : INbtMapperCapable
    {
        [NbtEntry]
        public long L;

        [NbtEntry]
        public long M;
    }
}
