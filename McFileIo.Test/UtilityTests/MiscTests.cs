using McFileIo.Utility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Test.UtilityTests
{
    public class MiscTests
    {
        [Test]
        public void NumericTest()
        {
            Assert.AreEqual(NumericUtility.GetRequiredBitLength(2), 1);
            Assert.AreEqual(NumericUtility.GetRequiredBitLength(4), 2);
            Assert.AreEqual(NumericUtility.GetRequiredBitLength(8), 3);
            Assert.AreEqual(NumericUtility.GetRequiredBitLength(7), 3);
            Assert.AreEqual(NumericUtility.GetRequiredBitLength(9), 4);
        }
    }
}
