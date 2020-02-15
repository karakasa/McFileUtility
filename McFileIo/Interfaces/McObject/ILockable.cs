using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces.McObject
{
    public interface ILockable
    {
        string Lock { get; set; }
    }
}
