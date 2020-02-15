using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NbtRequiredVerAttribute : Attribute
    {
        public readonly int Min;
        public readonly int Max;

        public NbtRequiredVerAttribute(int Min = 0, int Max = int.MaxValue)
        {
            this.Min = Min;
            this.Max = Max;
        }
    }
}
