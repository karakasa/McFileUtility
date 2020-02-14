using McFileIo.Interfaces;
using McFileIo.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace McFileIo.World
{
    public enum ChunkCompressionType
    {
        /// <summary>
        /// Unused in paractice. RFC 1952
        /// </summary>
        GZip = 1,

        /// <summary>
        /// RFC 1950
        /// </summary>
        ZLib = 2
    }

    public class RegionFile : IDisposable, IChunkCollection
    {
        public enum LoadStrategy
        {
            /// <summary>
            /// Chunks will be loaded from Stream on demand.
            /// You must ensure the stream is not closed.
            /// </summary>
            InStream,

            /// <summary>
            /// All chunks are loaded into memory when the region file is loaded
            /// </summary>
            InMemory
        }

        private Dictionary<int, Chunk> _cachedChunks = new Dictionary<int, Chunk>();
        private Dictionary<int, (int Offset, int Length, ChunkCompressionType Compression)> _cachedEntries = new Dictionary<int, (int, int, ChunkCompressionType)>();
        private Stream _innerStream = null;

        private RegionFile()
        {
        }

        public Chunk GetChunkData(int x, int z)
        {
            return GetChunkData(GetChunkIndex(x, z));
        }

        public Chunk GetChunkData(int index)
        {
            if (index < 0 || index >= 32 * 32)
            {
                throw new IndexOutOfRangeException();
            }

            if (_cachedChunks.TryGetValue(index, out var entry))
            {
                return entry;
            }

            if (_innerStream == null)
                return null;

            if (_cachedEntries.TryGetValue(index, out var streamEntry))
            {
                _innerStream.Seek(streamEntry.Offset, SeekOrigin.Begin);
                var compression = streamEntry.Compression;

                return Chunk.CreateFromBytes(
                    _innerStream.ReadToArray(streamEntry.Length), compressionType: compression);
            }

            return null;
        }

        public static (int x, int z) GetChunkCoordinateByIndex(int index)
        {
            return (index % 32, index / 32);
        }

        public static (int x, int z) GetChunkCoordinateByIndex(int index, int rx, int rz)
        {
            return (index % 32 + rx * 32, index / 32 + rz * 32);
        }

        public static int GetChunkIndex(int cx, int cz)
        {
            return ((cx & 31) + (cz & 31) * 32);
        }

        public static RegionFile CreateFromFile(string regionPath)
        {
            using (var file = File.Open(regionPath, FileMode.Open))
                return CreateFromStream(file);
        }

        public static RegionFile CreateFromBytes(byte[] content)
        {
            using (var file = new MemoryStream(content, false))
                return CreateFromStream(file);
        }

        public static RegionFile CreateFromStream(Stream stream, LoadStrategy load = LoadStrategy.InMemory)
        {
            if (!stream.CanRead || !stream.CanSeek) throw new ArgumentException(nameof(stream));

            var regionFile = new RegionFile();

            var chunkLocations = new byte[4096];
            var chunkTimestamps = new byte[4096];

            stream.Read(chunkLocations, 0, 4096);
            stream.Read(chunkTimestamps, 0, 4096);

            if (load == LoadStrategy.InStream)
            {
                regionFile._innerStream = stream;
            }

            for (var i = 0; i < 32 * 32; i++)
            {
                var headerPosition = i * 4;

                var chunkOffsetInfo = new byte[4] { 0, 0, 0, 0 };
                Array.Copy(chunkLocations, headerPosition, chunkOffsetInfo, 1, 3);

                var chunkOffset = EndianHelper.ToInt32(chunkOffsetInfo);
                var chunkSectors = chunkLocations[headerPosition + 3];

                // chunkSectors are not used in this library, however it should be correctly filled.

                if (chunkOffset < 2 || chunkSectors < 1) continue;

                var chunkTimestampInfo = new byte[4];
                Array.Copy(chunkTimestamps, headerPosition, chunkTimestampInfo, 0, 4);
                var chunkTimestamp = EndianHelper.ToInt32(chunkTimestampInfo);

                var realOffset = chunkOffset * 4096;

                stream.Seek(realOffset, SeekOrigin.Begin);

                var chunkLength = EndianHelper.ToUInt32(stream.ReadToArray());
                var chunkCompressionType = (ChunkCompressionType)stream.ReadByte();

                if (load == LoadStrategy.InMemory)
                {
                    var compressedChunkData = stream.ReadToArray((int)chunkLength - 1);

                    regionFile._cachedChunks.Add(i, 
                        Chunk.CreateFromBytes(compressedChunkData, compressionType: chunkCompressionType));

                    compressedChunkData = null;
                }
                else
                {
                    regionFile._cachedEntries.Add(i, (realOffset + 1, (int)chunkLength - 1, chunkCompressionType));
                }
            }

            return regionFile;
        }

        public void Dispose()
        {
            _innerStream = null;
            _cachedChunks?.Clear();
            _cachedEntries?.Clear();
        }

        public void UnloadAllChunks()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Chunk> AllChunks(TraverseType type = TraverseType.AlreadyLoaded)
        {

            if (type == TraverseType.AlreadyLoaded)
            {
                foreach (var it in _cachedChunks)
                    yield return it.Value;
            }
            else if (type == TraverseType.All)
            {
                foreach (var it in AllChunks(TraverseType.AlreadyLoaded))
                    yield return it;

                foreach (var it in _cachedEntries)
                {
                    var (Offset, Length, Compression) = it.Value;

                    _innerStream.Seek(Offset, SeekOrigin.Begin);
                    var compression = Compression;

                    var chunk = Chunk.CreateFromBytes(
                        _innerStream.ReadToArray(Length), compressionType: compression);

                    try
                    {
                        yield return chunk;
                    }
                    finally
                    {
                        chunk = null;
                    }
                }
            }
        }

        public static (int rx, int rz) GetRegionCoordByChunk(int cx, int cz)
        {
            return (cx >> 5, cz >> 5);
        }

        public static (int rx, int rz) GetRegionCoordByWorld(int x, int z)
        {
            return (x >> 9, z >> 9);
        }
    }
}
