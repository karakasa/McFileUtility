using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks
{
    public class DynamicIdMapper
    {
        private static int UniqueId = 32767;
        private static int NextUniqueFakeId => UniqueId++;

        private static readonly Dictionary<string, int> Identifiers = new Dictionary<string, int>();
        public static int AddDynamicId(string identifier)
        {
            var id = NextUniqueFakeId;
            Identifiers[identifier] = id;
            return id;
        }

        private readonly Dictionary<int, int> _inChunkRelations = new Dictionary<int, int>();
        public void AddMappingRelation(string identifier, int inChunkId)
        {
            if (Identifiers.TryGetValue(identifier, out var globalId))
                _inChunkRelations[inChunkId] = globalId;
        }
        public int FromInChunkId(int inChunkId)
        {
            if (_inChunkRelations.TryGetValue(inChunkId, out var globalId))
                return globalId;
            return -1;
        }
    }
}
