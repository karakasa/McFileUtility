using McFileIo.Blocks;
using McFileIo.Interfaces;
using McFileIo.World;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using static McFileIo.World.NamespacedChunk;

namespace McFileIo.Test.WorldTests
{
    public class NamespacedChunkTest
    {
        [Test]
        public void AllBlocksTest()
        {
            var chunk = CreateEmpty();
            var ns = new NamespacedBlock("test");
            using (var t = chunk.CreateChangeBlockTransaction())
            {
                t.Set(6, 16, 6, ns);
                t.Set(6, 90, 6, ns);
                t.CommitChanges();
            }

            foreach (var it in chunk.AllBlocks())
            {
                if (it.X == 6 && it.Z == 6 && (it.Y == 16 || it.Y == 90))
                {
                    Assert.IsTrue(it.Block == ns);
                }
                else
                {
                    Assert.IsTrue(it.Block == NamespacedBlock.AirBlock);
                }
            }
        }

        [Test]
        public void MultipleCommitTest()
        {
            var chunk = NamespacedChunk.CreateEmpty();
            using (var t = chunk.CreateChangeBlockTransaction())
            {
                t.Set(13, 40, 8, new NamespacedBlock("test_block"));

                t.CommitChanges();

                t.Set(new[] {
                    new ChangeBlockRequest(1, 41, 3, 0),
                    new ChangeBlockRequest(1, 41, 2, 1),
                    new ChangeBlockRequest(1, 99, 2, 1),
                    new ChangeBlockRequest(1, 10, 2, 1),
                }, new List<NamespacedBlock>() {
                    new NamespacedBlock("test_block"),
                    new NamespacedBlock("test_block2")
                });

                t.CommitChanges();

                t.Set(15, 2, 15, new NamespacedBlock("test_block"));

                t.Rollback();

                Assert.IsFalse(t.IsModified);
                Assert.IsTrue(t.IsAbandoned);
                Assert.Throws<InvalidOperationException>(() => t.Set(1, 2, 3, null));
                Assert.IsFalse(t.IsValid);
            }

            Assert.AreEqual(chunk.GetBlock(13, 40, 8).Name, "test_block");
            Assert.AreEqual(chunk.GetBlock(1, 41, 3).Name, "test_block");
            Assert.AreEqual(chunk.GetBlock(1, 41, 2).Name, "test_block2");
            Assert.AreEqual(chunk.GetBlock(1, 99, 2).Name, "test_block2");
            Assert.AreEqual(chunk.GetBlock(1, 10, 2).Name, "test_block2");

            Assert.AreSame(chunk.GetBlock(1, 41, 3), chunk.GetBlock(13, 40, 8));
        }

        [Test]
        public void MultipleVersionTest()
        {
            var chunk = NamespacedChunk.CreateEmpty();
            using (var t = chunk.CreateChangeBlockTransaction())
            using (var t2 = chunk.CreateChangeBlockTransaction())
            {
                t.Set(13, 40, 8, new NamespacedBlock("test_block"));
                t.CommitChanges();

                t2.Set(10, 40, 8, new NamespacedBlock("test_block"));
                Assert.IsTrue(t2.IsUpdatedOutside);
                Assert.Throws<InvalidOperationException>(() => t2.CommitChanges());
            }
        }

        [Test]
        public void GetSetTest()
        {
            var chunk = NamespacedChunk.CreateEmpty();
            using (var t = chunk.CreateChangeBlockTransaction())
            {
                t.Set(13, 40, 8, new NamespacedBlock("test_block"));

                t.Set(new [] {
                    new ChangeBlockRequest(1, 41, 3, 0),
                    new ChangeBlockRequest(1, 41, 2, 1),
                    new ChangeBlockRequest(1, 99, 2, 1),
                    new ChangeBlockRequest(1, 10, 2, 1),
                }, new List<NamespacedBlock>() {
                    new NamespacedBlock("test_block"),
                    new NamespacedBlock("test_block2")
                });

                Assert.IsTrue(t.IsValid);

                t.CommitChanges();
            }

            Assert.AreEqual(chunk.GetBlock(13, 40, 8).Name, "test_block");
            Assert.AreEqual(chunk.GetBlock(1, 41, 3).Name, "test_block");
            Assert.AreEqual(chunk.GetBlock(1, 41, 2).Name, "test_block2");
            Assert.AreEqual(chunk.GetBlock(1, 99, 2).Name, "test_block2");
            Assert.AreEqual(chunk.GetBlock(1, 10, 2).Name, "test_block2");

            Assert.AreSame(chunk.GetBlock(1, 41, 3), chunk.GetBlock(13, 40, 8));
        }

        [Test]
        public void HeightMapTest()
        {
            var chunk = NamespacedChunk.CreateEmpty();
            var blocks = new ChangeBlockRequest[] {
                new ChangeBlockRequest(15, 255, 13, 0),
                new ChangeBlockRequest(7, 60, 0, 1)
            };

            using (var t = chunk.CreateChangeBlockTransaction())
            {
                t.Set(blocks, new[] {
                    new NamespacedBlock("SS"),
                    new NamespacedBlock("TT")
                });
                t.CommitChanges();
            }

            var map = chunk.HeightMap;
            map.Calculate(chunk);

            Assert.AreEqual(map.State, HeightMap.StorageType.Post113);
            Assert.AreEqual(map.GetAt(0, 0, HeightMap.Type.MotionBlocking), -1);
            Assert.AreEqual(map.GetAt(15, 13), 255);
            Assert.AreEqual(map.GetAt(7, 0), 60);
            Assert.AreEqual(map.GetAt(8, 9), 0);
        }

        [Test]
        public void NaiveSetBlockTest()
        {
            var chunk = NamespacedChunk.CreateEmpty();

#pragma warning disable 0618
            chunk.SetBlock(13, 40, 8, new NamespacedBlock("test_block"));
#pragma warning restore 0618

            chunk.SetBlock(new[] {
                    new ChangeBlockRequest(1, 41, 3, 0),
                    new ChangeBlockRequest(1, 41, 2, 1),
                    new ChangeBlockRequest(1, 99, 2, 1),
                    new ChangeBlockRequest(1, 10, 2, 1),
                }, new List<NamespacedBlock>() {
                    new NamespacedBlock("test_block"),
                    new NamespacedBlock("test_block2")
                });

            Assert.AreEqual(chunk.GetBlock(13, 40, 8).Name, "test_block");
            Assert.AreEqual(chunk.GetBlock(1, 41, 3).Name, "test_block");
            Assert.AreEqual(chunk.GetBlock(1, 41, 2).Name, "test_block2");
            Assert.AreEqual(chunk.GetBlock(1, 99, 2).Name, "test_block2");
            Assert.AreEqual(chunk.GetBlock(1, 10, 2).Name, "test_block2");

            Assert.AreSame(chunk.GetBlock(1, 41, 3), chunk.GetBlock(13, 40, 8));
        }
    }
}
