using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Utility
{
    public class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
        }
    }
}
