using System;
using System.Collections.Generic;
using System.Text;
using McFileIo.Attributes;
using McFileIo.Interfaces;

namespace McFileIo.Misc
{
    public class NbtUuid : INbtIoCapable
    {
        [NbtEntry]
        public long L;

        [NbtEntry]
        public long M;
    }
}
