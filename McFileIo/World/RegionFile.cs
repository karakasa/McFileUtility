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

    public sealed class RegionFile : IDisposable, IChunkCollection
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
            InMemory,

            /// <summary>
            /// Do not attempt to load chunks. For region-specific information only.
            /// </summary>
            ForProbing
        }

        private Dictionary<int, Chunk> _cachedChunks = new Dictionary<int, Chunk>();
        private Dictionary<int, (int Offset, int Length, ChunkCompressionType Compression, uint Timestamp)> _cachedEntries = new Dictionary<int, (int, int, ChunkCompressionType, uint)>();
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

                var chunk = Chunk.CreateFromBytes(
                    _innerStream.ReadToArray(streamEntry.Length), compressionType: compression);

                chunk.Timestamp = streamEntry.Timestamp;

                return chunk;
            }

            return null;
        }

        public static (int X, int Z) GetChunkCoordinateByIndex(int index)
        {
            return (index & 31, index >> 5);
        }

        public static (int X, int Z) GetChunkCoordinateByIndex(int index, int rx, int rz)
        {
            return ((index & 31) + (rx << 5), (index >> 5) + (rz << 5));
        }

        public static int GetChunkIndex(int cx, int cz)
        {
            return (cx & 31) + ((cz & 31) << 5);
        }

        public static RegionFile CreateFromFile(string regionPath,
            int? rx = null, int? rz = null, LoadStrategy load = LoadStrategy.InMemory)
        {
            using (var file = File.Open(regionPath, FileMode.Open))
                return CreateFromStream(file, rx, rz, load);
        }

        public static RegionFile CreateFromBytes(byte[] content,
            int? rx = null, int? rz = null, LoadStrategy load = LoadStrategy.InMemory)
        {
            using (var file = new MemoryStream(content, false))
                return CreateFromStream(file, rx, rz, load);
        }

        public int? X { get; private set; } = null;
        public int? Z { get; private set; } = null;

        public static RegionFile CreateFromStream(Stream stream,
            int? rx = null, int? rz = null, LoadStrategy load = LoadStrategy.InMemory)
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

            var inferChunkCoord = (rx == null) || (rz == null);
            if (!inferChunkCoord)
            {
                regionFile.X = rx.Value;
                regionFile.Z = rz.Value;
            }

            // The following loop contains derived code from Matthew Blaine's Topographer which is released under the terms of the MIT License.
            // For more information, please refer to the copyright notice

            // The original code is in: https://github.com/Banane9/Topographer/blob/master/Minecraft/McRegion.cs
            // The original code is altered to
            //     (1) incooperate with the structure of this project
            //     (2) fix bugs
            //     (3) better performance

            for (var i = 0; i < 32 * 32; i++)
            {
                int cx = 0, cz = 0;
                if (!inferChunkCoord)
                {
                    (cx, cz) = GetChunkCoordinateByIndex(i);
                    cx += rx.Value << 5;
                    cz += rz.Value << 5;
                }

                var headerPosition = i << 2;

                var chunkOffset = (chunkLocations[headerPosition] << 16) 
                    | (chunkLocations[headerPosition + 1] << 8) | chunkLocations[headerPosition + 2];
                var chunkSectors = chunkLocations[headerPosition + 3];

                // the forth byte is the sector count of the chunk, at most 256 sectors (1MB).
                // the value is not used, given the chunk is probably larger than 1MB
                // and the chunk length is later included at the chunk entry.

                if (chunkOffset < 2 || chunkSectors == 0) continue;

                var chunkTimestamp = unchecked((uint)((chunkTimestamps[headerPosition] << 24)
                    | (chunkTimestamps[headerPosition + 1] << 16)
                    | (chunkTimestamps[headerPosition + 2] << 8)
                    | (chunkTimestamps[headerPosition + 3])));

                var realOffset = chunkOffset << 12;

                stream.Seek(realOffset, SeekOrigin.Begin);

                var chunkLength = EndianHelper.ToUInt32(stream);
                var chunkCompressionType = (ChunkCompressionType)stream.ReadByte();

                if (load == LoadStrategy.InMemory)
                {
                    var compressedChunkData = stream.ReadToArray((int)chunkLength - 1);
                    Chunk chunk;

                    if (inferChunkCoord)
                    {
                        chunk = Chunk.CreateFromBytes(compressedChunkData, compressionType: chunkCompressionType);
                    }
                    else
                    {
                        chunk = Chunk.CreateFromBytes(compressedChunkData,
                            ChunkX: cx, ChunkZ: cz,
                            compressionType: chunkCompressionType);
                    }

                    chunk.Timestamp = chunkTimestamp;

                    regionFile._cachedChunks.Add(i, chunk);

                    compressedChunkData = null;
                }
                else
                {
                    regionFile._cachedEntries.Add(i, (realOffset + 1,
                        (int)chunkLength - 1, chunkCompressionType, chunkTimestamp));
                }
            }

            return regionFile;
        }

        public IEnumerable<(int CX, int CZ, int InFileOffset,
            int InFileLength, ChunkCompressionType Compression, uint Timestamp)> GetInFileMetadata()
        {
            foreach(var it in _cachedEntries)
            {
                var (x, z) = GetChunkCoordinateByIndex(it.Key);
                yield return (x, z, it.Value.Offset, it.Value.Length, it.Value.Compression, it.Value.Timestamp);
            }
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

                if (_cachedEntries.Count > 0 && _innerStream == null)
                    throw new NotSupportedException();

                foreach (var it in _cachedEntries)
                {
                    var (Offset, Length, Compression, Timestamp) = it.Value;

                    _innerStream.Seek(Offset, SeekOrigin.Begin);
                    var compression = Compression;

                    var chunk = Chunk.CreateFromBytes(
                        _innerStream.ReadToArray(Length), compressionType: compression);

                    chunk.Timestamp = Timestamp;

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

        public static (int RX, int RZ) GetRegionCoordByChunk(int cx, int cz)
        {
            return (cx >> 5, cz >> 5);
        }

        public static (int RX, int RZ) GetRegionCoordByWorld(int x, int z)
        {
            return (x >> 9, z >> 9);
        }
    }
}
