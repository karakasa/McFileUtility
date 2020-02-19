using McFileIo.Misc;
using McFileIo.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.World
{
    public class PlayerData
    {
        public static Uuid GetOfflineUuidByName(string name)
        {
            return StringUtility.JavaUuidFromBytes(
                Encoding.UTF8.GetBytes("OfflinePlayer:" + name));
        }
    }
}
