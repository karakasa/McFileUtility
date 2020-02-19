using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Enum
{
    public enum ConcurrencyStrategy
    {
        OneTimeSnapshot,
        UpdateOtherSection,
        Ignore
    }
}
