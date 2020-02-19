using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Misc
{
    public struct Uuid
    {
        public long Least;
        public long Most;

        public Uuid(long L, long M)
        {
            Least = L;
            Most = M;
        }

        public Uuid(NbtUuid nbtUuid)
        {
            Least = nbtUuid.L;
            Most = nbtUuid.M;
        }
    }
}
