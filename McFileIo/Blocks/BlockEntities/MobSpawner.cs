using fNbt;
using McFileIo.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    [ApplyTo("mob_spawner")]
    public class MobSpawner : BlockEntity
    {
        // TODO: SpawnPotentials

        [NbtEntry(Optional: true)]
        public NbtCompound SpawnData;

        [NbtEntry(Optional: true)]
        public short? SpawnCount;

        [NbtEntry(Optional: true)]
        public short? SpawnRange;

        [NbtEntry(Optional: true)]
        public short? Delay;

        [NbtEntry(Optional: true)]
        public short? MinSpawnDelay;

        [NbtEntry(Optional: true)]
        public short? MaxSpawnDelay;

        [NbtEntry(Optional: true)]
        public short? MaxNearbyEntities;

        [NbtEntry(Optional: true)]
        public short? RequiredPlayerRange;
    }
}
