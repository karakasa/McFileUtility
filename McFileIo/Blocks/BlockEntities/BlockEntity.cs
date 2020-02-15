using fNbt;
using McFileIo.Attributes;
using McFileIo.Interfaces;
using McFileIo.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    /// <summary>
    /// A general version of BlockEntity. Specialized version will be automatically created if parser is available.
    /// To register custom parsers, see <see cref="RegisterParser"/> for more information.
    /// </summary>
    public class BlockEntity : INbtSnapshot, INbtMapperCapable
    {
        /// <summary>
        /// List of parsers, keyed by Ids.
        /// </summary>
        private static readonly Dictionary<string, Type> _typeref = new Dictionary<string, Type>();

        /// <summary>
        /// Register built-in parsers.
        /// </summary>
        static BlockEntity()
        {
            RegisterParser(typeof(BlockEntity).Assembly);
        }

        /// <summary>
        /// Register all BlockEntity parsers from an assembly.
        /// See <see cref="Sign"/> for example code.
        /// Use <see cref="ApplyToAttribute"/> to define supported blocks. Id must be lower-cased.
        /// </summary>
        /// <param name="assembly">Assembly to be imported</param>
        public static void RegisterParser(Assembly assembly)
        {
            RegisterParser(assembly.ExportedTypes);
        }

        /// <summary>
        /// Register BlockEntity parsers from a type list. Invalid parsers will be ignored.
        /// See <see cref="Sign"/> for example code.
        /// Use <see cref="ApplyToAttribute"/> to define supported blocks. Id must be lower-cased.
        /// </summary>
        /// <param name="type">Types to be imported</param>
        public static void RegisterParser(IEnumerable<Type> type)
        {
            var baseType = typeof(BlockEntity);
            foreach(var it in type.Where(t => baseType.IsAssignableFrom(t) && !baseType.Equals(t) && !t.IsAbstract))
            {
                foreach (var attr in it.GetCustomAttributes<ApplyToAttribute>())
                    if (attr.ApplyToObjectId != null)
                        _typeref[attr.ApplyToObjectId] = it;
            }
        }

        /// <summary>
        /// Register BlockEntity parsers from a type. If the type is invalid, it will be ignored.
        /// See <see cref="Sign"/> for example code.
        /// Use <see cref="ApplyToAttribute"/> to define supported blocks. Id must be lower-cased.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        public static void RegisterParser<T>() where T : class
        {
            RegisterParser(new[] { typeof(T) });
        }

        internal BlockEntity()
        {
        }

        public int X => x;
        public int Y => y;
        public int Z => z;
        public string Id => id;

        [NbtEntry]
        private int x = 0;

        [NbtEntry]
        private int y = 0;

        [NbtEntry]
        private int z = 0;

        [NbtEntry]
        private string id = null;

        private const string FieldId = "id";

        public NbtCompound NbtSnapshot { get; private set; }

        protected virtual void InitializeComponents(NbtCompound compound)
        {
            NbtClassMapper.ReadFromNbt(this, compound);
        }

        protected virtual void PostInitialization(NbtCompound compound)
        {
        }

        private void ReadFromNbtCompount(NbtCompound compound)
        {
            NbtSnapshot = compound;
            InitializeComponents(compound);
            PostInitialization(compound);
        }

        /// <summary>
        /// Get Id of BlockEntity from an Nbt storage. Returns <see langword="null"/> if malformed.
        /// This method will not create any BlockEntity.
        /// </summary>
        /// <param name="compound">Nbt storage</param>
        /// <returns>Id of the compound.</returns>
        public static string GetIdFromNbtCompound(NbtCompound compound)
        {
            if (compound.TryGet(FieldId, out NbtString blockid))
            {
                return blockid.Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Create a BlockEntity from an Nbt storage.
        /// Specialized version will be created if parsers are correctly registered.
        /// </summary>
        /// <param name="compound">Nbt storage</param>
        /// <returns>The created BlockEntity</returns>
        public static BlockEntity CreateFromNbtCompound(NbtCompound compound)
        {
            // TODO: Specialized initialization

            var blockid = GetIdFromNbtCompound(compound);
            if (blockid == null)
            {
                ExceptionHelper.ThrowParseMissingError(nameof(blockid));
                return null;
            }

            blockid = StringUtility.RemoveVanillaNamespace(blockid);

            BlockEntity entity;

            if (_typeref.TryGetValue(blockid.ToLowerInvariant(), out var type))
            {
                entity = (BlockEntity)Activator.CreateInstance(type);
            }
            else
            {
                entity = new BlockEntity();
            }

            entity.ReadFromNbtCompount(compound);

            return entity;
        }
    }
}
