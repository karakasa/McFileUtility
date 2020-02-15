using fNbt;
using McFileIo.Attributes;
using McFileIo.Interfaces.McObject;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    [ApplyTo("chest")]
    [ApplyTo("trapped_chest")]
    [ApplyTo("dispenser")]
    [ApplyTo("dropper")]
    [ApplyTo("barrel")]
    [ApplyTo("shulker_box")]
    public class LootableContainer : Container, ILootTableCapable
    {
        [NbtEntry(Optional: true)]
        public string LootTable { get; set; } = null;

        [NbtEntry(Optional: true)]
        [NbtSkipWriteIfEqual(0)]
        public long LootTableSeed { get; set; } = 0;

        protected override void PostInitialization(NbtCompound compound)
        {
            base.PostInitialization(compound);
        }

        public bool IsLootTableActive => Items == null;
    }
}
