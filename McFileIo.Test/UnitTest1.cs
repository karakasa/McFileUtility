using NUnit.Framework;
using McFileIo.World;
using McFileIo.Utility;
using fNbt;
using System.Diagnostics;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var dimension = Dimension.CreateFromSave(@"C:\games2\BilicraftClassic\.minecraft\saves\新的世界", DimensionType.Surface);

            stopwatch.Stop();
            var t = stopwatch.ElapsedMilliseconds;

            System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced, true, true);
            Assert.Pass();
        }
    }
}