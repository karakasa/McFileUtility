using McFileIo.Utility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Test.UtilityTests
{
    public class DynBitArrayTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GeneralVersionTest()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                DynBitArray.CreateFromByteArray(new byte[] { 1 }, 17);
            });

            var dyn = DynBitArray.CreateEmpty(12, 100);

            Assert.AreEqual(dyn.Length, 100);
            Assert.AreEqual(dyn.CellSize, 12);

            for (var i = 0; i < 100; i++)
                dyn[i] = i;

            for (var i = 0; i < 100; i++)
                Assert.AreEqual(i, dyn[i]);
        }

        [Test]
        public void Specialized4Test()
        {
            var dyn = DynBitArray.CreateEmpty(4, 100);
            for (var i = 0; i < 100; i++)
                dyn[i] = i % 16;

            for (var i = 0; i < 100; i++)
                Assert.AreEqual(i % 16, dyn[i]);
        }

        [Test]
        public void Specialized5Test()
        {
            var dyn = DynBitArray.CreateEmpty(5, 100);
            for (var i = 0; i < 100; i++)
                dyn[i] = i % 31;

            for (var i = 0; i < 100; i++)
                Assert.AreEqual(i % 31, dyn[i]);
        }

        [Test]
        public void ConversionTest()
        {
            var dyn = DynBitArray.CreateEmpty(5, 100);
            for (var i = 0; i < 100; i++)
                dyn[i] = i % 31;

            Assert.Throws<NotSupportedException>(() => {
                DynBitArray.CreateCloneFrom(3, dyn);
            });

            var dyn2 = DynBitArray.CreateCloneFrom(11, dyn);

            for (var i = 0; i < 100; i++)
                Assert.AreEqual(i % 31, dyn2[i]);
        }

        [Test]
        public void LongResultTest()
        {
            var longs = new[] { 2L, -1L, 2181234892347L, 10000L };
            var dyn = DynBitArray.CreateFromLongArray(longs, 4);
            var ordinalDyn = new DynBitArray(dyn, 4);

            Assert.AreEqual(DynBitArray.ToLongArray(ordinalDyn), longs);
        }
    }
}
