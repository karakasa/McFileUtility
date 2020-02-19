using NUnit.Framework;
using McFileIo.World;
using McFileIo.Utility;
using fNbt;
using System.Diagnostics;
using System.Reflection;

namespace McFileIo.Test.UtilityTests
{
    public class EndianHelperTest
    {
        public static readonly int[] BaseData = new int[] { 5, 7, 5, 14, 4, 2, 7, 2, 9, 0, 0, 9, 1, 12, 2, 5, 1, 2, 0, 9, 3, 8, 5, 4, 9, 6, 1, 2, 10, 1, 4, 9 };

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void HalfIntTest()
        {
            var bytes = new byte[16];

            for (var i = 0; i < 32; i++)
            {
                EndianHelper.SetHalfInt(bytes, i, BaseData[i]);
            }

            for (var i = 0; i < 32; i++)
            {
                Assert.AreEqual(EndianHelper.GetHalfInt(bytes, i), BaseData[i]);
            }
        }

        [Test]
        public void HalfIntMalformedDataTest()
        {
            var bytes = new byte[16];

            for (var i = 0; i < 32; i++)
            {
                EndianHelper.SetHalfInt(bytes, i, BaseData[i]);
            }

            const int indexUnderflow = 5;
            const int indexOverflow = 9;

            const int underflow = -999;
            const int overflow = 999;

            EndianHelper.SetHalfInt(bytes, indexUnderflow, underflow);
            EndianHelper.SetHalfInt(bytes, indexOverflow, overflow);

            for (var i = 0; i < 32; i++)
            {
                if (i == indexUnderflow)
                {
                    Assert.AreEqual(EndianHelper.GetHalfInt(bytes, i), underflow & 0xf);
                }
                else if (i == indexOverflow)
                {
                    Assert.AreEqual(EndianHelper.GetHalfInt(bytes, i), overflow & 0xf);
                }
                else
                {
                    Assert.AreEqual(EndianHelper.GetHalfInt(bytes, i), BaseData[i]);
                }
            }
        }

        [Test]
        public void HalfIntInternalMethodTest()
        {
            var bytes = new byte[16];

            for (var i = 0; i < 32; i++)
            {
                EndianHelper.SetHalfInt(bytes, i, BaseData[i]);
            }

            var methodOdd = typeof(EndianHelper).GetMethod("GetHalfIntOddIndex", BindingFlags.Static | BindingFlags.NonPublic);
            var methodEven = typeof(EndianHelper).GetMethod("GetHalfIntEvenIndex", BindingFlags.Static | BindingFlags.NonPublic);

            for (var i = 0; i < 32; i++)
            {
                var result = (int)((i % 2 == 0) ? methodEven : methodOdd).Invoke(null, new object[] { bytes, i });
                Assert.AreEqual(EndianHelper.GetHalfInt(bytes, i), result);
            }
        }

        [Test]
        public void DynBitArrayEquivalentTest()
        {
            var bytes = new byte[16];

            for (var i = 0; i < 32; i++)
            {
                EndianHelper.SetHalfInt(bytes, i, BaseData[i]);
            }

            var dynarray = DynBitArray.CreateFromByteArray(bytes, 4);
            Assert.AreEqual(32, dynarray.Length);

            for (var i = 0; i < 32; i++)
            {
                Assert.AreEqual(dynarray[i], EndianHelper.GetHalfInt(bytes, i));
            }
        }

        [Test]
        public void BytesLongArrayConversionTest()
        {
            var longs = new[] { 2L, -1L, 2181234892347L, 10000L };
            var bytes = EndianHelper.LongArrayToBytes(longs);
            Assert.AreEqual(EndianHelper.BytesToLongArray(bytes), longs);

            bytes = new byte[] { 47, 55, 60, 12, 0, 254, 80, 3, 7,
                12, 0, 254, 80, 3, 254, 80, 3, 7, 12, 0, 254, 80, 3, 254 };
            longs = EndianHelper.BytesToLongArray(bytes);
            Assert.AreEqual(EndianHelper.LongArrayToBytes(longs), bytes);
        }
    }
}