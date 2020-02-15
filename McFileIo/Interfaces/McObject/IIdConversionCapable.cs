using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces.McObject
{
    /// <summary>
    /// By implementing this, your McObject may convert between Id and Namespaced names.
    /// </summary>
    public interface IIdConversionCapable
    {
        bool ConvertToIdName();
        bool ConvertToNamespacedName();
    }
}
