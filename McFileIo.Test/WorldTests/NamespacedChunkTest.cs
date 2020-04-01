using McFileIo.Blocks;
using McFileIo.Blocks.LowLevel;
using McFileIo.Enum;
using McFileIo.Interfaces;
using McFileIo.World;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                t.SetBlock(6, 16, 6, ns);
                t.SetBlock(6, 90, 6, ns);
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
                t.SetBlock(13, 40, 8, new NamespacedBlock("test_block"));

                t.CommitChanges();

                t.SetBlock(new[] {
                    new ChangeBlockRequest(1, 41, 3, 0),
                    new ChangeBlockRequest(1, 41, 2, 1),
                    new ChangeBlockRequest(1, 99, 2, 1),
                    new ChangeBlockRequest(1, 10, 2, 1),
                }, new List<NamespacedBlock>() {
                    new NamespacedBlock("test_block"),
                    new NamespacedBlock("test_block2")
                });

                t.CommitChanges();

                t.SetBlock(15, 2, 15, new NamespacedBlock("test_block"));

                t.Rollback();

                Assert.IsFalse(t.IsModified);
                Assert.IsTrue(t.IsAbandoned);
                Assert.Throws<InvalidOperationException>(() => t.SetBlock(1, 2, 3, null));
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
                t.SetBlock(13, 40, 8, new NamespacedBlock("test_block"));
                t.CommitChanges();

                t2.SetBlock(10, 40, 8, new NamespacedBlock("test_block"));
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
                t.SetBlock(13, 40, 8, new NamespacedBlock("test_block"));

                t.SetBlock(new [] {
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
                t.SetBlock(blocks, new[] {
                    new NamespacedBlock("SS"),
                    new NamespacedBlock("TT")
                });
                t.CommitChanges();
            }

            var map = chunk.HeightMap;
            map.Calculate(chunk);

            Assert.AreEqual(map.State, AttributeVersion.Post113);
            Assert.AreEqual(map.GetAt(0, 0, HeightmapType.MotionBlocking), -1);
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

        [Test]
        public void AdvancedConcurrencyTest()
        {
            var chunk = NamespacedChunk.CreateEmpty();
            using (var t = chunk.CreateChangeBlockTransaction())
            using (var t2 = chunk.CreateChangeBlockTransaction())
            {
                t.ConcurrencyMode = t2.ConcurrencyMode 
                    = ConcurrencyStrategy.UpdateOtherSection;

                t.SetBlock(1, 1, 1, new NamespacedBlock("test_block"));
                t.CommitChanges();

                t2.SetBlock(90, 90, 90, new NamespacedBlock("test_block2"));
                Assert.DoesNotThrow(() => t2.CommitChanges());
                Assert.AreEqual(t2.GetBlock(1, 1, 1).Name, "test_block");
            }

            Assert.AreEqual(chunk.GetBlock(1, 1, 1).Name, "test_block");
            Assert.AreEqual(chunk.GetBlock(90, 90, 90).Name, "test_block2");
        }

        [Test]
        public void CompactPaletteTest()
        {
            var chunk = NamespacedChunk.CreateEmpty();
            using (var t = chunk.CreateChangeBlockTransaction())
            {
                t.SetBlock(5, 7, 9, new NamespacedBlock("test_block"));
                t.CommitChanges();

                var infos = t.GetPaletteInformation(7 >> 4)
                    .OrderBy(info => info.Block.Name).ToArray();

                Assert.AreEqual(infos.Length, 2);
                Assert.AreEqual(infos[1].Block.Name, "test_block");
                Assert.AreEqual(infos[0].Count, 4095);
                Assert.AreEqual(infos[1].Count, 1);

                t.SetBlock(5, 7, 9, NamespacedBlock.AirBlock);
                t.CommitChanges();

                Assert.AreEqual(t.GetPaletteInformation(7 >> 4).Count(), 0);

                t.SetBlock(5, 7, 9, new NamespacedBlock("test_block"));
                t.SetBlock(5, 7, 10, new NamespacedBlock("test_block2"));
                t.CommitChanges();

                Assert.AreEqual(t.GetPaletteInformation(7 >> 4).Count(), 3);

                t.SetBlock(5, 7, 10, NamespacedBlock.AirBlock);
                t.CommitChanges();

                Assert.AreEqual(t.GetPaletteInformation(7 >> 4).Count(), 2);

                t.CompactBeforeCommit = false;

                t.SetBlock(5, 7, 9, new NamespacedBlock("test_block"));
                t.SetBlock(5, 7, 10, new NamespacedBlock("test_block2"));
                t.CommitChanges();

                Assert.AreEqual(t.GetPaletteInformation(7 >> 4).Count(), 3);

                t.SetBlock(5, 7, 10, NamespacedBlock.AirBlock);
                t.CommitChanges();

                Assert.AreEqual(t.GetPaletteInformation(7 >> 4).Count(), 3);
            }
        }

        [Test]
        public void ChunkCompactTest()
        {
            var chunk = NamespacedChunk.CreateEmpty();
            using (var t = chunk.CreateChangeBlockTransaction())
            {
                t.CompactBeforeCommit = false;

                t.SetBlock(5, 7, 9, new NamespacedBlock("test_block"));
                t.SetBlock(5, 7, 10, new NamespacedBlock("test_block2"));
                t.CommitChanges();

                Assert.AreEqual(t.GetPaletteInformation(7 >> 4).Count(), 3);

                t.SetBlock(5, 7, 10, NamespacedBlock.AirBlock);
                t.CommitChanges();

                Assert.AreEqual(t.GetPaletteInformation(7 >> 4).Count(), 3);
            }

            chunk.Compact();

            using (var t = chunk.CreateChangeBlockTransaction())
            {
                Assert.AreEqual(t.GetPaletteInformation(7 >> 4).Count(), 2);
            }
        }

        [Test]
        public void CompactBlockBitsTest()
        {
            var chunk = NamespacedChunk.CreateEmpty();
            using (var t = chunk.CreateChangeBlockTransaction())
            {
                t.SetBlock(1, 1, 0, new NamespacedBlock("test0"));
                t.SetBlock(1, 1, 1, new NamespacedBlock("test1"));
                t.SetBlock(1, 1, 2, new NamespacedBlock("test2"));
                t.SetBlock(1, 1, 3, new NamespacedBlock("test3"));
                t.SetBlock(1, 1, 4, new NamespacedBlock("test4"));
                t.SetBlock(1, 1, 5, new NamespacedBlock("test5"));
                t.SetBlock(1, 1, 6, new NamespacedBlock("test6"));
                t.SetBlock(1, 1, 7, new NamespacedBlock("test7"));
                t.SetBlock(1, 1, 8, new NamespacedBlock("test8"));
                t.SetBlock(1, 1, 9, new NamespacedBlock("test9"));
                t.SetBlock(1, 1, 10, new NamespacedBlock("test10"));
                t.SetBlock(1, 1, 11, new NamespacedBlock("test11"));
                t.SetBlock(1, 1, 12, new NamespacedBlock("test12"));
                t.SetBlock(1, 1, 13, new NamespacedBlock("test13"));
                t.SetBlock(1, 1, 14, new NamespacedBlock("test14"));
                t.SetBlock(1, 1, 15, new NamespacedBlock("test15"));
                t.SetBlock(2, 1, 0, new NamespacedBlock("test16"));
                t.SetBlock(2, 1, 1, new NamespacedBlock("test17"));

                t.CommitChanges();

                var blockstates = typeof(NamespacedChunk).GetField("_blockStates", BindingFlags.Instance | BindingFlags.NonPublic);

                Assert.AreEqual(((IDynBitArray[])blockstates.GetValue(chunk))[0].CellSize, 5);

                t.SetBlock(1, 1, 2, NamespacedBlock.AirBlock);
                t.SetBlock(1, 1, 3, NamespacedBlock.AirBlock);
                t.SetBlock(1, 1, 4, NamespacedBlock.AirBlock);
                t.SetBlock(1, 1, 5, NamespacedBlock.AirBlock);
                t.SetBlock(1, 1, 6, NamespacedBlock.AirBlock);
                t.SetBlock(1, 1, 7, NamespacedBlock.AirBlock);
                t.SetBlock(1, 1, 8, NamespacedBlock.AirBlock);
                t.SetBlock(1, 1, 9, NamespacedBlock.AirBlock);
                t.SetBlock(1, 1, 10, NamespacedBlock.AirBlock);
                t.SetBlock(1, 1, 11, NamespacedBlock.AirBlock);
                t.SetBlock(1, 1, 12, NamespacedBlock.AirBlock);
                t.SetBlock(1, 1, 13, NamespacedBlock.AirBlock);
                t.SetBlock(1, 1, 14, NamespacedBlock.AirBlock);
                t.SetBlock(1, 1, 15, NamespacedBlock.AirBlock);
                t.SetBlock(2, 1, 0, NamespacedBlock.AirBlock);
                t.SetBlock(2, 1, 1, NamespacedBlock.AirBlock);

                t.CompactBlockBitsIfPossible = false;
                t.CommitChanges();

                Assert.AreEqual(((IDynBitArray[])blockstates.GetValue(chunk))[0].CellSize, 5);

                chunk.Compact();

                Assert.AreEqual(((IDynBitArray[])blockstates.GetValue(chunk))[0].CellSize, 4);
            }
        }
    }
}
