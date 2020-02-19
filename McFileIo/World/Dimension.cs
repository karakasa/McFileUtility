using McFileIo.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace McFileIo.World
{
    public class Dimension
    {
        private const string NameNether = "DIM-1";
        private const string NameEnd = "DIM1";
        private const string RegionDirectory = "region";

        private readonly string _directory;

        private Dimension(string directory)
        {
            _directory = directory;
        }

        private WorldData _worldData = null;

        public CacheStrategy RegionReaderCacheStrategy = CacheStrategy.UnloadAfterOperation;

        public WorldData WorldData {
            get
            {
                if (_worldData == null)
                    _worldData = WorldData.CreateFromRegionDirectory(Path.Combine(_directory, RegionDirectory));
                return _worldData;
            }
        }

        public static Dimension CreateFromSave(string directory, DimensionType type, string dimensionName = null)
        {
            if (type == DimensionType.Custom && string.IsNullOrEmpty(dimensionName))
                throw new ArgumentNullException(nameof(dimensionName));

            switch (type)
            {
                case DimensionType.Surface:
                    return CreateFromDirectory(directory);
                case DimensionType.Nether:
                    return CreateFromSave(directory, DimensionType.Custom, NameNether);
                case DimensionType.End:
                    return CreateFromSave(directory, DimensionType.Custom, NameEnd);
                case DimensionType.Custom:
                    return CreateFromDirectory(Path.Combine(directory, dimensionName));
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        public static Dimension CreateFromDirectory(string directory)
        {
            if (!Directory.Exists(directory)) throw new DirectoryNotFoundException();
            return new Dimension(directory);
        }
    }
}
