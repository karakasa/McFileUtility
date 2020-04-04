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
    public class BlockFactoryDispatcher : IContextAware
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
                    return Factory.FromClassic(block, entity);
                else
                    return FactoryMultiple.FromClassic(Index, block, entity);
            }
            public Block CreateBlock(NamespacedBlock block, BlockEntity entity)
            {
                if (Index == -1)
                    return Factory.FromNamespaced(block, entity);
                else
                    return FactoryMultiple.FromNamespaced(Index, block, entity);
            }

            public ClassicBlock GetClassicBlock(Block block, out BlockEntity entity)
            {
                if (Index == -1)
                    return Factory.ToClassic(block, out entity);
                else
                    return FactoryMultiple.ToClassic(Index, block, out entity);
            }

            public NamespacedBlock GetNamespacedBlock(Block block, out BlockEntity entity)
            {
                if (Index == -1)
                    return Factory.ToNamespaced(block, out entity);
                else
                    return FactoryMultiple.ToNamespaced(Index, block, out entity);
            }
        }

        private class CacheServer : IBlockDispatcherCache
        {
            private struct Entry
            {
                public bool HasClassicBlock;
                public ClassicBlock Classic;
                public NamespacedBlock Namespaced;
                public BlockEntity Entity;
            }
            private readonly List<Entry> _entries = new List<Entry>(512);

            public int RegisterDispatcherCache(bool hasClassicBlock, ClassicBlock classic, NamespacedBlock ns, BlockEntity entity)
            {
                if(!hasClassicBlock && ns is null)
                {
                    throw new ArgumentException($"{nameof(hasClassicBlock)} & {nameof(ns)} cannot be both empty.");
                }

                _entries.Add(new Entry()
                {
                    HasClassicBlock = hasClassicBlock,
                    Classic = classic,
                    Namespaced = ns,
                    Entity = entity
                });

                return _entries.Count - 1;
            }

            public bool HasClassic(int index)
            {
                if (index < 0 || index >= _entries.Count)
                    return false;
                return _entries[index].HasClassicBlock;
            }

            public ClassicBlock Classic(int index)
            {
                if (index < 0 || index >= _entries.Count)
                    return default;
                return _entries[index].Classic;
            }

            public NamespacedBlock Namespaced(int index)
            {
                if (index < 0 || index >= _entries.Count)
                    return null;
                return _entries[index].Namespaced;
            }

            public BlockEntity Entity(int index)
            {
                if (index < 0 || index >= _entries.Count)
                    return null;
                return _entries[index].Entity;
            }
        }

        private readonly CacheServer Cache = new CacheServer();

        private readonly Dictionary<int, BlockFactoryEntry> ClassicEntries = new Dictionary<int, BlockFactoryEntry>();
        private readonly Dictionary<string, BlockFactoryEntry> NamespacedEntries = new Dictionary<string, BlockFactoryEntry>();
        private readonly Dictionary<Guid, BlockFactoryEntry> ReversedEntries = new Dictionary<Guid, BlockFactoryEntry>();

        public bool UseCache = true;
        public BlockFactoryDispatcher(IInterpretContext context)
        {
            ActiveContext = context;
        }
        public IInterpretContext ActiveContext { get; set; } = null;

        public Block CreateBlock(ClassicBlock block, BlockEntity entity = null)
        {
            if (!ClassicEntries.TryGetValue(block.Id, out var entry))
                return CreateUnknownBlock(block, entity);

            return entry.CreateBlock(block, entity) ?? CreateUnknownBlock(block, entity);
        }
        public Block CreateBlock(NamespacedBlock block, BlockEntity entity = null)
        {
            if (!NamespacedEntries.TryGetValue(block.Name, out var entry))
                return CreateUnknownBlock(block, entity);

            return entry.CreateBlock(block, entity) ?? CreateUnknownBlock(block, entity);
        }

        public bool TryGetClassicBlock(Block block, out ClassicBlock lowlevel, out BlockEntity entity)
        {
            if (block.UniqueId == UnknownBlock.BuiltInUniqueId)
            {
                var unknown = block as UnknownBlock;
                lowlevel = unknown.ToClassic();
                entity = unknown.AssociatedEntity;
                return true;
            }

            if (UseCache)
            {
                var cacheId = block.CachedId;
                if (cacheId != -1 && Cache.HasClassic(cacheId))
                {
                    lowlevel = Cache.Classic(cacheId);
                    entity = Cache.Entity(cacheId);
                    return true;
                }
            }

            if (!ReversedEntries.TryGetValue(block.UniqueId, out var entry))
            {
                lowlevel = default;
                entity = null;
                return false;
            }

            lowlevel = entry.GetClassicBlock(block, out entity);
            return true;
        }

        public bool TryGetNamespacedBlock(Block block, out NamespacedBlock lowlevel, out BlockEntity entity)
        {
            if (block.UniqueId == UnknownBlock.BuiltInUniqueId)
            {
                var unknown = block as UnknownBlock;
                lowlevel = unknown.ToNamespaced();
                entity = unknown.AssociatedEntity;
                return true;
            }

            if (UseCache)
            {
                var cacheId = block.CachedId;
                if (cacheId != -1 && (lowlevel = Cache.Namespaced(cacheId)) is null)
                {
                    entity = Cache.Entity(cacheId);
                    return true;
                }
            }

            if (!ReversedEntries.TryGetValue(block.UniqueId, out var entry))
            {
                lowlevel = default;
                entity = null;
                return false;
            }

            lowlevel = entry.GetNamespacedBlock(block, out entity);
            return true;
        }

        private UnknownBlock CreateUnknownBlock(ClassicBlock block, BlockEntity entity)
        {
            return new UnknownBlock(block, entity);
        }
        private UnknownBlock CreateUnknownBlock(NamespacedBlock block, BlockEntity entity)
        {
            return new UnknownBlock(block, entity);
        }
        public void AddDispatcher<T>() where T : IBlockFactory, new()
        {
            AddDispatcherObject(new T());
        }
        public void AddDispatcherMultiple<T>() where T : IBlockFactoryMultiple, new()
        {
            AddDispatcherObject(new T());
        }

        public void AddDispatcher(Type type)
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

        private void AddDispatcherObject(IBlockFactory factory)
        {
            factory.ActiveContext = ActiveContext;
            var entry = new BlockFactoryEntry(factory);

            if (factory.ClassicIndex != Block.NotSupportClassicIndex)
                ClassicEntries[factory.ClassicIndex] = entry;

            if (factory.NamespacedName != Block.NotSupportNamespacedId)
                NamespacedEntries[factory.NamespacedName] = entry;

            ReversedEntries[factory.UniqueId] = entry;

            factory.RegisterCachedBlocks(this.Cache);
        }
        private void AddDispatcherObject(IBlockFactoryMultiple factory)
        {
            factory.ActiveContext = ActiveContext;

            var indexes = factory.ClassicIndexes.GetEnumerator();
            var names = factory.NamespacedNames.GetEnumerator();
            var uid = factory.UniqueIds.GetEnumerator();
            var index = 0;

            while (indexes.MoveNext())
            {
                names.MoveNext();
                uid.MoveNext();

                var entry = new BlockFactoryEntry(factory, index);

                if (indexes.Current != Block.NotSupportClassicIndex)
                    ClassicEntries[indexes.Current] = entry;

                if (names.Current != Block.NotSupportNamespacedId)
                    NamespacedEntries[names.Current] = entry;

                ReversedEntries[uid.Current] = entry;

                ++index;
            }

            factory.RegisterCachedBlocks(this.Cache);
        }

        private bool IsFactoryType(Type type)
        {
            if (type.IsAbstract || type.IsInterface || type.IsGenericType)
                return false;

            if (!typeof(IBlockFactory).IsAssignableFrom(type) &&
                !typeof(IBlockFactoryMultiple).IsAssignableFrom(type))
                return false;

            return type.GetConstructors().Any(ctor => ctor.GetParameters().Length == 0);
        }
        public void AddDispatcherFromAssembly(Assembly assembly)
        {
            foreach (var type in assembly.ExportedTypes.Where(IsFactoryType))
                AddDispatcher(type);
        }
    }
}
