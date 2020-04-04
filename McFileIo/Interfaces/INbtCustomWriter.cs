using fNbt;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces
{
    /// <summary>
    /// By implementing this, your Nbt-enabled class will be solely reponsible for writing.
    /// It is ideal for simple but massively-used objects, as performance is better.
    /// </summary>
    public interface INbtCustomWriter
    {
        void Write(IInterpretContext context, NbtCompound activeNode);
    }
}
