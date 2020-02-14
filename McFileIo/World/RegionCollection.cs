using McFileIo.Interfaces;
using McFileIo.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace McFileIo.World
{
    public class RegionCollection : IDisposable, IChunkCollection
    {
        public enum LocateStrategy
        {
            Automatic,
            FastByName,
            LookInsideChunk // Not supported yet
        }

        public enum CacheStrategy
        {
            UnloadAfterOperation,
            KeepInMemory,
            AllInMemoryImmediately
        }

        private static bool IsAcceptableNumber(string number)
        {
            return number.All(c => "-0123456789".IndexOf(c) != -1);
        }

        private static bool IsRegionFileName(string filename)
        {
            var names = filename.Split('.');
            if (names.Length != 3) return false;
            if (names[0] != "r") return false;
            return IsAcceptableNumber(names[1]) && IsAcceptableNumber(names[2]);
        }

        private static (int rx, int rz) GetRegionCoordinatesByName(string filename)
        {
            var names = filename.Split('.');
            return (Convert.ToInt32(names[1]), Convert.ToInt32(names[2]));
        }

        private static LocateStrategy DetermineLocateStrategy(IEnumerable<string> names)
        {
            if (names.Any(n => !IsRegionFileName(n)))
                return LocateStrategy.LookInsideChunk;
            return LocateStrategy.FastByName;
        }

        private static (int rx, int rz) GetRegionCoordinates(string filename, LocateStrategy locate)
        {
            return GetRegionCoordinatesByName(filename);
        }

        private Dictionary<(int rx, int ry), string> _regionFiles = new Dictionary<(int rx, int ry), string>();
        private Dictionary<string, byte[]> _cachedRegionContent = new Dictionary<string, byte[]>();
        private Dictionary<string, RegionFile> _cachedRegionFile = new Dictionary<string, RegionFile>();

        private void EnsureFileLoaded(string filepath)
        {
            if (!_cachedRegionContent.ContainsKey(filepath))
                _cachedRegionContent[filepath] = File.ReadAllBytes(filepath);
        }

        public CacheStrategy CacheApproach;

        public RegionCollection(IEnumerable<string> regionFiles,
            LocateStrategy locate = LocateStrategy.Automatic,
            CacheStrategy cache = CacheStrategy.UnloadAfterOperation)
        {
            var filenames = regionFiles.ToList();

            if(locate == LocateStrategy.Automatic)
                locate = DetermineLocateStrategy(filenames.Select(Path.GetFileNameWithoutExtension));

            if (locate == LocateStrategy.LookInsideChunk)
                throw new NotSupportedException();

            CacheApproach = cache;

            foreach(var it in filenames)
            {
                var coord = GetRegionCoordinates(Path.GetFileNameWithoutExtension(it), locate);
                _regionFiles[coord] = it;

                if (cache == CacheStrategy.AllInMemoryImmediately)
                    EnsureFileLoaded(it);
            }
        }

        public bool IsRegionAvailableInMemory(int rx, int rz)
        {
            if (!_regionFiles.TryGetValue((rx, rz), out var file))
                return false;
            return _cachedRegionContent.ContainsKey(file);
        }

        public bool IsRegionAvailable(int rx, int rz)
        {
            if (!_regionFiles.TryGetValue((rx, rz), out var file))
                return false;
            return _cachedRegionContent.ContainsKey(file) || File.Exists(file);
        }

        public byte[] GetRegionContent(int rx, int rz)
        {
            if (!_regionFiles.TryGetValue((rx, rz), out var file))
                return null;

            if (_cachedRegionContent.TryGetValue(file, out var content))
                return content;

            content = File.ReadAllBytes(file);

            if (CacheApproach != CacheStrategy.UnloadAfterOperation)
                _cachedRegionContent[file] = content;

            return content;
        }

        public RegionFile GetRegionFile(int rx, int rz)
        {
            if (!_regionFiles.TryGetValue((rx, rz), out var file))
                return null;

            if (_cachedRegionFile.TryGetValue(file, out var content))
                return content;

            if (_cachedRegionContent.TryGetValue(file, out var binary))
                content = RegionFile.CreateFromBytes(binary);
            else
                content = RegionFile.CreateFromBytes(File.ReadAllBytes(file));

            if (CacheApproach != CacheStrategy.UnloadAfterOperation)
                _cachedRegionFile[file] = content;

            return content;
        }

        public IEnumerable<(int rx, int rz)> GetRegionCoordinates()
        {
            return _regionFiles.Keys;
        }

        public IEnumerable<RegionFile> GetRegions()
        {
            var coords = GetRegionCoordinates().ToArray();
            foreach(var (rx, rz) in coords)
            {
                yield return GetRegionFile(rx, rz);
            }
        }

        public IEnumerable<RegionFile> GetCachedRegions()
        {
            return _cachedRegionFile.Values;
        }

        public void ClearContentCache()
        {
            _cachedRegionContent.Clear();
            foreach (var it in _cachedRegionFile)
                it.Value?.Dispose();
            _cachedRegionFile.Clear();
        }

        public void Dispose()
        {
            ClearContentCache();
            _regionFiles.Clear();
        }

        public IEnumerable<Chunk> AllChunks(TraverseType type = TraverseType.AlreadyLoaded)
        {
            foreach (var region in GetRegions())
                foreach (var it in region.AllChunks(type))
                    yield return it;
        }

        public void UnloadAllChunks()
        {
            throw new NotImplementedException();
        }
    }
}
