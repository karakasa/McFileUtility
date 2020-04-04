using McFileIo.Blocks.Definitions;
using McFileIo.Blocks.LowLevel;
using McFileIo.Blocks.LowLevel.BlockEntities;
using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace McFileIo.Blocks
{
    public class SimpleBlocks : IBlockFactoryMultiple
    {
        public static readonly Air Air = new Air();
        public static readonly Grass Grass = new Grass();

        private struct BlockRegistryEntry
        {
            public Block Block;
            public int Index;
            public string Name;
        }

        private static BlockRegistryEntry Entry(Block block, int index, string id)
        {
            return new BlockRegistryEntry()
            {
                Block = block,
                Index = index,
                Name = id
            };
        }

        public SimpleBlocks() : this(null)
        {
        }
        public SimpleBlocks(IInterpretContext context)
        {
            ((IContextAware)this).ActiveContext = context;
            InitializeInternalRegistry();
        }

        private void InitializeInternalRegistry()
        {
            ListOfBlocks.Add(Entry(Air, 0, "minecraft:air"));
            ListOfBlocks.Add(Entry(Grass, 2, "minecraft:grass_block"));
        }

        private readonly List<BlockRegistryEntry> ListOfBlocks = new List<BlockRegistryEntry>(32);
        IEnumerable<int> IBlockFactoryMultiple.ClassicIndexes => ListOfBlocks.Select(block => block.Index);

        IEnumerable<string> IBlockFactoryMultiple.NamespacedNames => ListOfBlocks.Select(block => block.Name);

        IEnumerable<Guid> IBlockFactoryMultiple.UniqueIds => ListOfBlocks.Select(block => block.Block.UniqueId);

        IInterpretContext IContextAware.ActiveContext { get; set; }

        Block IBlockFactoryMultiple.FromClassic(int index, ClassicBlock block, BlockEntity entity) => ListOfBlocks[index].Block;

        Block IBlockFactoryMultiple.FromNamespaced(int index, NamespacedBlock block, BlockEntity entity) => ListOfBlocks[index].Block;

        private static readonly Dictionary<string, NamespacedBlock> 
            SimpleBlockCache = new Dictionary<string, NamespacedBlock>();
        private static NamespacedBlock QuerySimpleBlockCache(string name)
        {
            if (name == NamespacedBlock.IdAirBlock)
                return NamespacedBlock.AirBlock;

            if (SimpleBlockCache.TryGetValue(name, out var block))
                return block;

            return SimpleBlockCache[name] = new NamespacedBlock(name);
        }

        private ClassicBlock ToClassic(int index)
        {
            return new ClassicBlock(ListOfBlocks[index].Index);
        }
        private NamespacedBlock ToNamespaced(int index)
        {
            return QuerySimpleBlockCache(ListOfBlocks[index].Name);
        }
        ClassicBlock IBlockFactoryMultiple.ToClassic(int index, Block block, out BlockEntity entity)
        {
            entity = null;
            return ToClassic(index);
        }

        NamespacedBlock IBlockFactoryMultiple.ToNamespaced(int index, Block block, out BlockEntity entity)
        {
            entity = null;
            return ToNamespaced(index);
        }

        void IBlockFactoryMultiple.RegisterCachedBlocks(IBlockDispatcherCache cache)
        {
            Air.CachedId = cache.RegisterDispatcherCache(true, ToClassic(0), ToNamespaced(0));
        }
    }
}
