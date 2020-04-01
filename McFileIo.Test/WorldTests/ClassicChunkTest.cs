using McFileIo.Blocks;
using McFileIo.World;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using fNbt;
using McFileIo.Enum;
using McFileIo.Blocks.LowLevel;

namespace McFileIo.Test.WorldTests
{
    public class ClassicChunkTest
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void GetSetTest()
        {
            var chunk = ClassicChunk.CreateEmpty();
            var blocks = new (int X, int Y, int Z, ClassicBlock Block)[] {
                (15, 255, -37, new ClassicBlock(4095, 0)),
                (13, 60, 0, new ClassicBlock(32, 14))
            };

            foreach(var it in blocks)
            {
                chunk.SetBlock(it.X, it.Y, it.Z, it.Block);
            }

            foreach (var it in blocks)
            {
                var block = chunk.GetBlock(it.X, it.Y, it.Z);
                Assert.AreEqual(block, it.Block);
            }

            Assert.AreEqual(chunk.GetExistingYs().OrderBy(t => t).ToArray(),
                new int[] { 60 >> 4, 255 >> 4 });

            var isAirBlock = typeof(ClassicChunk).GetMethod("IsAirBlock", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsTrue((bool)isAirBlock.Invoke(chunk, new object[] { 8, 8, 8 }));
            Assert.IsFalse((bool)isAirBlock.Invoke(chunk, new object[] { 13, 60, 0 }));
        }

        [Test]
        public void HeightMapTest()
        {
            var chunk = ClassicChunk.CreateEmpty();
            var blocks = new (int X, int Y, int Z, ClassicBlock Block)[] {
                (15, 255, 13, new ClassicBlock(4095, 0)),
                (7, 60, 0, new ClassicBlock(32, 14))
            };

            foreach (var it in blocks)
            {
                chunk.SetBlock(it.X, it.Y, it.Z, it.Block);
            }

            var map = chunk.HeightMap;
            map.Calculate(chunk);

            Assert.AreEqual(map.State, AttributeVersion.Pre113);
            Assert.Throws<NotSupportedException>(() => map.GetAt(0, 0, HeightmapType.MotionBlocking));
            Assert.AreEqual(map.GetAt(15, 13), 255);
            Assert.AreEqual(map.GetAt(7, 0), 60);
            Assert.AreEqual(map.GetAt(8, 9), 0);
        }

        [Test]
        public void AllBlocksTest()
        {
            var chunk = ClassicChunk.CreateEmpty();
            var blocks = new (int X, int Y, int Z, ClassicBlock Block)[] {
                (15, 255, 13, new ClassicBlock(4095, 0)),
                (7, 60, 0, new ClassicBlock(32, 14))
            };

            foreach (var it in blocks)
            {
                chunk.SetBlock(it.X, it.Y, it.Z, it.Block);
            }

            foreach(var (X, Y, Z, block) in chunk.AllBlocks())
            {
                Assert.AreEqual(chunk.GetBlock(X, Y, Z), block);
            }
        }

        [Test]
        public void SaveSectionsTest()
        {
            var chunk = ClassicChunk.CreateEmpty();
            chunk.SetBlock(1, 1, 1, 16);
            chunk.CommitChanges();

            var nbt = (NbtCompound)chunk.NbtSnapshot.Clone();
            var chunk2 = ClassicChunk.CreateEmpty();
            var method = typeof(LowLevelChunk).GetMethod("ReadFromNbt", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(chunk2, new object[] { nbt });

            Assert.AreEqual(chunk2.GetBlock(1, 1, 1).Id, 16);
        }
    }
}
