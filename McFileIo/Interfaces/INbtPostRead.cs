using fNbt;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces
{
    public interface INbtPostRead
    {
        void PostRead(INbtIoContext context, NbtCompound activeNode);
    }
}
