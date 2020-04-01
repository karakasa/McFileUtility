using McFileIo.World;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using McFileIo.Blocks;
using McFileIo.Enum;
using McFileIo.Blocks.LowLevel;

namespace McFileIo.Test.WorldTests
{
    public class RegionFileTest
    {
        [Test]
        public void SimpleClassicChunkSLTest()
        {
            var region = (RegionFile)Activator.CreateInstance(typeof(RegionFile), true);
            var dict = (Dictionary<int, LowLevelChunk>)(typeof(RegionFile)
                .GetField("_cachedChunks", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(region));

            var chunk = ClassicChunk.CreateEmpty();
            chunk.SetBlock(1, 1, 1, new ClassicBlock(700, 14));
            chunk.CommitChanges();
            dict[5] = chunk;

            using (var stream = new MemoryStream())
            {
                Assert.DoesNotThrow(() => region.SaveToStream(stream));
                stream.Seek(0, SeekOrigin.Begin);

                var region2 = RegionFile.CreateFromStream(stream, 0, 0, RegionLoadApproach.InMemory);
                var block = (region2.GetChunkData(5) as ClassicChunk).GetBlock(1, 1, 1);
                Assert.AreEqual(block.Id, 700);
                Assert.AreEqual(block.Data, 14);
            }
        }

        [Test]
        public void SimpleNamespacedChunkSLTest()
        {
            var region = (RegionFile)Activator.CreateInstance(typeof(RegionFile), true);
            var dict = (Dictionary<int, LowLevelChunk>)(typeof(RegionFile)
                .GetField("_cachedChunks", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(region));

            var chunk = NamespacedChunk.CreateEmpty();

#pragma warning disable 0618
            chunk.SetBlock(1, 1, 1, new NamespacedBlock("test_block"));
#pragma warning restore 0618
            chunk.CommitChanges();
            dict[5] = chunk;

            using (var stream = new MemoryStream())
            {
                Assert.DoesNotThrow(() => region.SaveToStream(stream));
                stream.Seek(0, SeekOrigin.Begin);

                var region2 = RegionFile.CreateFromStream(stream, 0, 0, RegionLoadApproach.InMemory);
                var block = (region2.GetChunkData(5) as NamespacedChunk).GetBlock(1, 1, 1);
                Assert.AreEqual(block.Name, "test_block");
            }
        }
    }
}
