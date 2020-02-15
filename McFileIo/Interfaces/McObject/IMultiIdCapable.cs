using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces.McObject
{
    /// <summary>
    /// By implementing this, your McObject may support both Id and Namespace scheme
    /// </summary>
    public interface IMultiIdCapable
    {
        bool IsNamespacedNameUsed { get; }
    }
}
