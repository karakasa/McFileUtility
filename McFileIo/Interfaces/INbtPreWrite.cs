using fNbt;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces
{
    public interface INbtPreWrite
    {
        void PreWrite(INbtIoContext context, NbtCompound activeNode);
    }
}
