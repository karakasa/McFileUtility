using fNbt;
using McFileIo.Attributes;
using McFileIo.Interfaces;
using McFileIo.Utility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Test.UtilityTests
{
    public class NbtClassIoTest
    {
        private NbtCompound demoData;
        private NbtCompound demoData2;

        private class TestIoClass : INbtIoCapable
        {
            [NbtEntry]
            public int TestInt;

            [NbtEntry]
            public string TestString;

            [NbtEntry]
            public byte TestByte;

            [NbtEntry]
            public short TestShort;

            [NbtEntry]
            public long TestLong;

            [NbtEntry]
            public float TestFloat;

            [NbtEntry]
            public double TestDouble;

            [NbtEntry]
            public byte[] TestByteArray;

            [NbtEntry]
            public int[] TestIntArray;

            [NbtEntry]
            public long[] TestLongArray;

            [NbtEntry(Optional: true)]
            public int? OptionalTest;

            [NbtEntry("AnotherName")]
            public int AliasTest;

            [NbtEntry]
            public bool TestBooleanTrue;

            [NbtEntry]
            public bool TestBooleanFalse;

            [NbtEntry]
            public int PropertyTest { get; set; }
        }

        private class TestIoClassPostRead : TestIoClass, INbtPostRead
        {
            public void PostRead(IInterpretContext context, NbtCompound activeNode)
            {
                TestInt++;
            }
        }

        private class TestIoClassTypeMismatch : INbtIoCapable
        {
            [NbtEntry]
            public int[] TestInt;
        }

        private class TestIoClassMissing : INbtIoCapable
        {
            [NbtEntry]
            public string NonexistentField;
        }

        private class TestIoClassCustomReader : INbtIoCapable, INbtCustomReader
        {
            public int CustomInt;

            [NbtEntry]
            public string TestString;

            public void Read(IInterpretContext context, NbtCompound activeNode)
            {
                CustomInt = activeNode.Get<NbtInt>("TestInt").Value + 1;
            }
        }

        private class TestComplicatedIoClass : INbtIoCapable
        {
            [NbtEntry]
            public TestNestedIoClass Nested;

            [NbtEntry]
            public List<int> ListOfInts;

            [NbtEntry]
            public List<TestNestedIoClass> ListOfNested;

            [NbtEntry(Optional: true)]
            public List<int> NonexistentList;

            [NbtEntry]
            public List<List<int>> ListOfLists;
        }

        private class TestNestedIoClass : INbtIoCapable
        {
            [NbtEntry]
            public int NestedInt;
        }

        [SetUp]
        public void SetUp()
        {
            demoData = new NbtCompound
            {
                new NbtInt("TestInt", 42),
                new NbtString("TestString", "42"),
                new NbtByte("TestByte", 42),
                new NbtByte("TestBooleanTrue", 1),
                new NbtByte("TestBooleanFalse", 0),
                new NbtShort("TestShort", 42),
                new NbtLong("TestLong", 42),
                new NbtFloat("TestFloat", 42.0f),
                new NbtDouble("TestDouble", 42.0),
                new NbtByteArray("TestByteArray", new byte[] { 4, 2 }),
                new NbtIntArray("TestIntArray", new[] { 4, 2 }),
                new NbtLongArray("TestLongArray", new long[] { 4, 2 }),
                new NbtInt("AnotherName", 42),
                new NbtInt("PropertyTest", 42)
            };

            demoData2 = new NbtCompound()
            {
                new NbtCompound("Nested")
                {
                    new NbtInt("NestedInt", 42)
                },
                new NbtList("ListOfInts")
                {
                    new NbtInt(4),
                    new NbtInt(2)
                },
                new NbtList("ListOfNested")
                {
                    new NbtCompound()
                    {
                        new NbtInt("NestedInt", 4)
                    },
                    new NbtCompound()
                    {
                        new NbtInt("NestedInt", 2)
                    }
                },
                new NbtList("ListOfLists")
                {
                    new NbtList()
                    {
                        new NbtInt(5)
                    }
                }
            };
        }

        [Test]
        public void ReadBasicTypeTest()
        {
            var test = NbtClassIo.CreateAndReadFromNbt<TestIoClass>(demoData);
            
            Assert.AreEqual(test.TestInt, 42);
            Assert.AreEqual(test.TestString, "42");
            Assert.AreEqual(test.TestByte, 42);
            Assert.IsTrue(test.TestBooleanTrue);
            Assert.IsFalse(test.TestBooleanFalse);
            Assert.AreEqual(test.TestShort, 42);
            Assert.AreEqual(test.TestLong, 42);
            Assert.IsTrue(Math.Abs(test.TestFloat - 42.0f) < 1e-4f);
            Assert.IsTrue(Math.Abs(test.TestDouble - 42.0) < 1e-4);
            Assert.AreEqual(test.TestByteArray, new byte[] { 4, 2 });
            Assert.AreEqual(test.TestIntArray, new [] { 4, 2 });
            Assert.AreEqual(test.TestLongArray, new long[] { 4, 2 });

            Assert.AreEqual(test.AliasTest, 42);
            Assert.AreEqual(test.PropertyTest, 42);
            Assert.IsNull(test.OptionalTest);
        }

        [Test]
        public void ReadExceptionTest()
        {
            Assert.Throws<ParseException>(() =>
            NbtClassIo.CreateAndReadFromNbt<TestIoClassTypeMismatch>(demoData));

            Assert.Throws<FieldMissingException>(() =>
            NbtClassIo.CreateAndReadFromNbt<TestIoClassMissing>(demoData));
        }

        [Test]
        public void ReadCustomReaderTest()
        {
            var test = NbtClassIo.CreateAndReadFromNbt<TestIoClassCustomReader>(demoData);
            Assert.AreEqual(test.CustomInt, 43);
            Assert.IsNull(test.TestString);
        }

        [Test]
        public void ReadPostEventTest()
        {
            var test = NbtClassIo.CreateAndReadFromNbt<TestIoClassPostRead>(demoData);
            Assert.AreEqual(test.TestInt, 43);
        }

        [Test]
        public void ReadComplicatedTypeTest()
        {
            var test = NbtClassIo.CreateAndReadFromNbt<TestComplicatedIoClass>(demoData2);

            Assert.IsNull(test.NonexistentList);
            Assert.AreEqual(test.Nested.NestedInt, 42);
            Assert.AreEqual(test.ListOfInts.ToArray(), new[] { 4, 2 });

            Assert.AreEqual(test.ListOfNested.Count, 2);
            Assert.AreEqual(test.ListOfNested[0].NestedInt, 4);
            Assert.AreEqual(test.ListOfNested[1].NestedInt, 2);

            Assert.AreEqual(test.ListOfLists.Count, 1);
            Assert.AreEqual(test.ListOfLists[0].Count, 1);
            Assert.AreEqual(test.ListOfLists[0][0], 5);
        }
    }
}
