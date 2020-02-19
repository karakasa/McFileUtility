using McFileIo.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Utility
{
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
