using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Utility
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

    public static class ExceptionHelper
    {
        /// <summary>
        /// Any error equal or greater than StopLevel will stop parsing
        /// </summary>
        public static ParseErrorLevel StopLevel = ParseErrorLevel.Warning;

        public static void ThrowParseError(ParseException message, ParseErrorLevel level)
        {
            if (level >= StopLevel)
                throw message;
        }

        public static void ThrowParseError(string message, ParseErrorLevel level)
        {
            ThrowParseError(new ParseException(message), level);
        }

        public static void ThrowParseMissingError(string missingElement, ParseErrorLevel level = ParseErrorLevel.Exception)
        {
            ThrowParseError(new FieldMissingException(missingElement), level);
        }
    }
}
