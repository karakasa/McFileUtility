using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Utility
{
    public class FieldMissingException : ParseException
    {
        public FieldMissingException(string elementIdentifier) : base($"{elementIdentifier} is missing")
        {
        }
    }
}
