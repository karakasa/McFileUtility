using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Enum
{
    /// <summary>
    /// Define different levels of parsing errors
    /// </summary>
    public enum ParseErrorLevel
    {
        /// <summary>
        /// Minor discrepancy that can be corrected automatically. However, the file may be rejected by other programs.
        /// </summary>
        Information = 0,

        /// <summary>
        /// Error that may prevent normal parsing of current object. As a result, the object may be partially parsed.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Error that stops normal parsing of current object, which makes the object unusable.
        /// </summary>
        Exception = 2,

        /// <summary>
        /// Error that may corrupt the module or the caller. McFileIo's internal procedure will never use this.
        /// </summary>
        Fatal = 3
    }
}
