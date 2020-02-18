using fNbt;
using McFileIo.Blocks;
using McFileIo.Blocks.BlockProperties;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Test.BlockTests
{
    public class NamespacedBlockTest
    {
        private NamespacedBlock sourceBlock;
        private NamespacedBlock sourceBlock2;
        private NamespacedBlock sourceBlock3;

        [SetUp]
        public void Setup()
        {
            sourceBlock = new NamespacedBlock("TestBlock",
                NbtBlockProperty.CreateFromNbt("test", new NbtCompound()
                {
                    new NbtInt("base", 2)
                }));

            sourceBlock2 = new NamespacedBlock("EqualTest");
            sourceBlock3 = new NamespacedBlock("EqualTest");
        }

        [Test]
        public void EqualTest()
        {
            Assert.IsTrue(sourceBlock2 == sourceBlock3);
            Assert.IsTrue(sourceBlock2.Equals(sourceBlock3));
            Assert.IsTrue(sourceBlock2.Equals((object)sourceBlock3));
            Assert.IsFalse(sourceBlock2 == sourceBlock);
            Assert.IsFalse(sourceBlock2 == null);
            Assert.IsFalse(null == sourceBlock2);
        }

        [Test]
        public void CloneTest()
        {
            var newBlock = (NamespacedBlock)sourceBlock.Clone();

            Assert.IsInstanceOf<NbtBlockProperty>(newBlock.Properties);

            var nbt = newBlock.Properties as NbtBlockProperty;
            Assert.IsTrue(nbt.NbtSnapshot.Remove("base"));

            Assert.AreEqual(nbt.NbtSnapshot.Count, 0);
            Assert.AreEqual(((NbtBlockProperty)sourceBlock.Properties).NbtSnapshot.Count, 1);
        }
    }
}
