using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Utility
{
    internal static class StringUtility
    {
        public static string RemoveVanillaNamespace(string id)
        {
            if (id.StartsWith("minecraft:"))
                return id.Substring(10);
            return id;
        }
    }
}
