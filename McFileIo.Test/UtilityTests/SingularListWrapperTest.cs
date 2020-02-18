using McFileIo.Utility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace McFileIo.Test.UtilityTests
{
    public class SingularListWrapperTest
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void BasicTest()
        {
            var list = new SingularListWrapper<int>();

            Assert.AreEqual(list.Count, 0);
            Assert.IsTrue(list.IsEmpty);
            Assert.Throws<ArgumentOutOfRangeException>(() => { var v = list[0]; });
            Assert.Throws<ArgumentOutOfRangeException>(() => { list[0] = 1; });
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(0));
            Assert.IsFalse(list.Contains(1));
            Assert.IsFalse(list.Remove(1));
            Assert.AreEqual(list.IndexOf(1), -1);

            list.Add(1);

            Assert.Throws<NotSupportedException>(() => list.Add(5));

            Assert.AreEqual(list.Count, 1);
            Assert.IsFalse(list.IsEmpty);
            Assert.AreEqual(list[0], 1);
            Assert.DoesNotThrow(() => { list[0] = 2; });
            Assert.Throws<NotSupportedException>(() => list.RemoveAt(1));
            Assert.DoesNotThrow(() => list.RemoveAt(0));
            Assert.Throws<NotSupportedException>(() => list.Insert(1, 6));
            Assert.DoesNotThrow(() => list.Insert(0, 6));
            Assert.IsTrue(list.Contains(6));
            Assert.AreEqual(list.IndexOf(6), 0);

            var arr = list.Select(t => t).ToArray();
            Assert.AreEqual(arr.Length, 1);
            Assert.AreEqual(arr[0], 6);

            Assert.IsTrue(list.Remove(6));
        }
    }
}
