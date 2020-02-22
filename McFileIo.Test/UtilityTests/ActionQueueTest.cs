using McFileIo.Utility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace McFileIo.Test.UtilityTests
{
    public class ActionQueueTest
    {
        private class IntAccumlator
        {
            public int Value;
        }

        [Test]
        public void BasicTest()
        {
            var accu = new IntAccumlator() { Value = 0 };
            var queue = new ActionQueue<IntAccumlator>(accu);
            Assert.DoesNotThrow(() => Parallel.For(1, 101, x =>
            {
                var index = x;

                queue.Enqueue((acc, args) =>
                {
                    acc.Value += index;
                    Assert.AreEqual((int)args, 42);
                }, 42);
            }));

            var completed = queue.Perform();
            Assert.AreEqual(completed, 100);
            Assert.AreEqual(accu.Value, 5050);
        }

        [Test]
        public void BasicWithLimitTest()
        {
            var accu = new IntAccumlator() { Value = 0 };
            var queue = new ActionQueue<IntAccumlator>(accu);
            Assert.DoesNotThrow(() => Parallel.For(1, 101, x =>
            {
                var index = x;

                queue.Enqueue((acc, args) =>
                {
                    acc.Value += index;
                    Assert.AreEqual((int)args, 42);
                }, 42);
            }));

            var completed = queue.Perform(50);
            Assert.AreEqual(completed, 50);
            // There is no guarantee for the enqueue order

            completed = queue.Perform();

            Assert.AreEqual(completed, 50);
            Assert.AreEqual(accu.Value, 5050);
        }
    }
}
