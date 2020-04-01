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

        private static readonly List<Block> ListOfBlocks = new List<Block>()
        {
            Air
        };
        public IEnumerable<int> ClassicIndex => ListOfBlocks.Select(block => block.ClassicIndex);

        public IEnumerable<string> NamespacedName => ListOfBlocks.Select(block => block.NamespacedName);

        public Block CreateFromClassic(int index, ClassicBlock block, BlockEntity entity) => ListOfBlocks[index];

        public Block CreateFromNamespaced(int index, NamespacedBlock block, BlockEntity entity) => ListOfBlocks[index];

        private static readonly Dictionary<string, NamespacedBlock> 
            SimpleBlockCache = new Dictionary<string, NamespacedBlock>();
        internal static NamespacedBlock QuerySimpleBlockCache(string name)
        {
            if (name == NamespacedBlock.IdAirBlock)
                return NamespacedBlock.AirBlock;

            if (SimpleBlockCache.TryGetValue(name, out var block))
                return block;

            return SimpleBlockCache[name] = new NamespacedBlock(name);
        }
    }
}
