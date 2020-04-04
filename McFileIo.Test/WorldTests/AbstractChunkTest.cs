using McFileIo.Blocks;
using McFileIo.Blocks.Definitions;
using McFileIo.Blocks.LowLevel;
using McFileIo.Enum;
using McFileIo.World;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace McFileIo.Test.WorldTests
{
    public class AbstractChunkTest
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void UnderlyingClassicChunkTest()
        {
            const int IdGrass = 2;

            var baseChunk = ClassicChunk.CreateEmpty();
            baseChunk.SetBlock(4, 4, 4, new ClassicBlock(IdGrass));
            var dispatcher = new BlockFactoryDispatcher(null);
            dispatcher.AddDispatcherFromAssembly(typeof(BlockFactoryDispatcher).Assembly);

            var chunk1 = new Chunk(baseChunk, AccessMode.Write, dispatcher);
            Assert.IsTrue(chunk1.GetBlock(4, 4, 5) is Air);
            Assert.IsTrue(chunk1.GetBlock(4, 4, 4) is Grass);

            var blocks = chunk1.AllBlocks().ToArray();
            Assert.AreEqual(blocks.Length, 16 * 16 * 16);
            Assert.AreEqual(blocks[0].Block, blocks[blocks.Length - 1].Block);

            foreach (var it in chunk1.AllBlocks((index, name) => index == IdGrass))
            {
                Assert.AreEqual(it.X, 4);
                Assert.AreEqual(it.Y, 4);
                Assert.AreEqual(it.Z, 4);
                Assert.IsTrue(it.Block is Grass);
            }

            Assert.DoesNotThrow(() => chunk1.SetBlock(7, 7, 7, SimpleBlocks.Grass));
            Assert.DoesNotThrow(() => chunk1.SaveToLowLevelStorage());

            Assert.AreEqual(baseChunk.GetBlock(7, 7, 7).Id, IdGrass);
        }

        [Test]
        public void UnderlyingNamespacedChunkTest()
        {
            const string IdGrass = "minecraft:grass_block";

            var baseChunk = NamespacedChunk.CreateEmpty();
#pragma warning disable 0618
            baseChunk.SetBlock(4, 4, 4, new NamespacedBlock(IdGrass));
#pragma warning restore 0618
            var dispatcher = new BlockFactoryDispatcher(null);
            dispatcher.AddDispatcherFromAssembly(typeof(BlockFactoryDispatcher).Assembly);

            var chunk1 = new Chunk(baseChunk, AccessMode.Write, dispatcher);
            Assert.IsTrue(chunk1.GetBlock(4, 4, 5) is Air);
            Assert.IsTrue(chunk1.GetBlock(4, 4, 4) is Grass);

            var blocks = chunk1.AllBlocks().ToArray();
            Assert.AreEqual(blocks.Length, 16 * 16 * 16);
            Assert.AreEqual(blocks[0].Block, blocks[blocks.Length - 1].Block);

            foreach (var it in chunk1.AllBlocks((index, name) => name == IdGrass))
            {
                Assert.AreEqual(it.X, 4);
                Assert.AreEqual(it.Y, 4);
                Assert.AreEqual(it.Z, 4);
                Assert.IsTrue(it.Block is Grass);
            }

            Assert.DoesNotThrow(() => chunk1.SetBlock(7, 7, 7, SimpleBlocks.Grass));
            Assert.DoesNotThrow(() => chunk1.SaveToLowLevelStorage());

            Assert.AreEqual(baseChunk.GetBlock(7, 7, 7).Name, IdGrass);
        }
    }
}
