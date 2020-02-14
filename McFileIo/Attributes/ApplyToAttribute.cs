using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Attributes
{
    /// <summary>
    /// Determine what objects the current class will apply to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ApplyToAttribute : Attribute
    {
        public readonly string ApplyToObjectId = null;

        public ApplyToAttribute(string Id)
        {
            ApplyToObjectId = Id;
        }
    }
}
