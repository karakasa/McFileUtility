using McFileIo.World;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Test.WorldTests
{
    public class PlayerDataTest
    {
        [Test]
        public void UuidTest()
        {
            var uuid = PlayerData.GetOfflineUuidByName("amamoyou");
        }
    }
}
