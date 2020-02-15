using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NbtSkipWriteIfEqualAttribute : Attribute
    {
        public readonly int SkipedValue;

        public NbtSkipWriteIfEqualAttribute(int Value = 0)
        {
            SkipedValue = Value;
        }
    }
}
