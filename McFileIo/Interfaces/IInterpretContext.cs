using McFileIo.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces
{
    public interface IInterpretContext
    {
        RegistryType IdMapping { get; }
        int Version { get; }
    }
}
