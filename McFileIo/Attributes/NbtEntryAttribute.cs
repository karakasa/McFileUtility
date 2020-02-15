using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NbtEntryAttribute : Attribute
    {
        public readonly string TagName = null;
        public readonly bool Optional = false;

        public NbtEntryAttribute(string TagName = null, bool Optional = false)
        {
            this.TagName = TagName;
            this.Optional = Optional;
        }
    }
}
