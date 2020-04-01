using McFileIo.Blocks.LowLevel;
using McFileIo.Blocks.LowLevel.BlockEntities;
using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

namespace McFileIo.Blocks
{
    public static class BlockFactoryDispatcher
    {
        private struct BlockFactoryEntry
        {
            public IBlockFactory Factory;
            public IBlockFactoryMultiple FactoryMultiple;
            public int Index;

            public BlockFactoryEntry(IBlockFactory factory)
            {
                Factory = factory;
                FactoryMultiple = null;
                Index = -1;
            }

            public BlockFactoryEntry(IBlockFactoryMultiple factory, int index)
            {
                FactoryMultiple = factory;
                Factory = null;
                Index = index;
            }

            public Block CreateBlock(ClassicBlock block, BlockEntity entity)
            {
                if (Index == -1)
                    return Factory.CreateFromClassic(block, entity);
                else
                    return FactoryMultiple.CreateFromClassic(Index, block, entity);
            }
            public Block CreateBlock(NamespacedBlock block, BlockEntity entity)
            {
                if (Index == -1)
                    return Factory.CreateFromNamespaced(block, entity);
                else
                    return FactoryMultiple.CreateFromNamespaced(Index, block, entity);
            }
        }

        private static readonly Dictionary<int, BlockFactoryEntry> ClassicEntries = new Dictionary<int, BlockFactoryEntry>();
        private static readonly Dictionary<string, BlockFactoryEntry> NamespacedEntries = new Dictionary<string, BlockFactoryEntry>();

        public static Block CreateBlock(ClassicBlock block, BlockEntity entity = null)
        {
            if (!ClassicEntries.TryGetValue(block.Id, out var entry))
                return CreateUnknownBlock(block, entity);

            return entry.CreateBlock(block, entity) ?? CreateUnknownBlock(block, entity);
        }

        public static Block CreateBlock(ClassicBlock block, DynamicIdMapper mapper, BlockEntity entity = null)
        {
            if (!ClassicEntries.TryGetValue(block.Id, out var entry))
            {
                var id = mapper.FromInChunkId(block.Id);
                if (!ClassicEntries.TryGetValue(id, out entry))
                    return CreateUnknownBlock(block, entity);
            }

            return entry.CreateBlock(block, entity) ?? CreateUnknownBlock(block, entity);
        }
        public static Block CreateBlock(NamespacedBlock block, BlockEntity entity = null)
        {
            if (!NamespacedEntries.TryGetValue(block.Name, out var entry))
                return CreateUnknownBlock(block, entity);

            return entry.CreateBlock(block, entity) ?? CreateUnknownBlock(block, entity);
        }

        private static UnknownBlock CreateUnknownBlock(ClassicBlock block, BlockEntity entity)
        {
            return new UnknownBlock(block, entity);
        }
        private static UnknownBlock CreateUnknownBlock(NamespacedBlock block, BlockEntity entity)
        {
            return new UnknownBlock(block, entity);
        }
        public static void AddDispatcher<T>() where T : IBlockFactory, new()
        {
            AddDispatcherObject(new T());
        }
        public static void AddDispatcherMultiple<T>() where T : IBlockFactoryMultiple, new()
        {
            AddDispatcherObject(new T());
        }

        public static void AddDispatcher(Type type)
        {
            if (typeof(IBlockFactory).IsAssignableFrom(type))
            {
                AddDispatcherObject(Activator.CreateInstance(type) as IBlockFactory);
                return;
            }

            if (typeof(IBlockFactoryMultiple).IsAssignableFrom(type))
            {
                AddDispatcherObject(Activator.CreateInstance(type) as IBlockFactoryMultiple);
                return;
            }

            throw new ArgumentException(nameof(type));
        }

        private static void AddDispatcherObject(IBlockFactory factory)
        {
            if (factory.ClassicIndex == -1)
                ClassicEntries[factory.ClassicIndex] = new BlockFactoryEntry(factory);

            if (!string.IsNullOrEmpty(factory.NamespacedName))
                NamespacedEntries[factory.NamespacedName] = new BlockFactoryEntry(factory);
        }
        private static void AddDispatcherObject(IBlockFactoryMultiple factory)
        {
            var indexes = factory.ClassicIndex.GetEnumerator();
            var names = factory.NamespacedName.GetEnumerator();
            var index = 0;

            while (indexes.MoveNext())
            {
                names.MoveNext();

                if (indexes.Current != -1)
                    ClassicEntries[indexes.Current] = new BlockFactoryEntry(factory, index);

                if (!string.IsNullOrEmpty(names.Current))
                    NamespacedEntries[names.Current] = new BlockFactoryEntry(factory, index);

                ++index;
            }
        }

        private static bool IsFactoryType(Type type)
        {
            if (type.IsAbstract || type.IsInterface || type.IsGenericType)
                return false;

            if (!typeof(IBlockFactory).IsAssignableFrom(type) &&
                !typeof(IBlockFactoryMultiple).IsAssignableFrom(type))
                return false;

            return type.GetConstructors().Any(ctor => ctor.GetParameters().Length == 0);
        }
        public static void AddDispatcherFromAssembly(Assembly assembly)
        {
            foreach (var type in assembly.ExportedTypes.Where(IsFactoryType))
                AddDispatcher(type);
        }

        static BlockFactoryDispatcher()
        {
            AddDispatcherFromAssembly(typeof(BlockFactoryDispatcher).Assembly);
        }
    }
}
