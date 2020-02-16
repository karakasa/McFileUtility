using fNbt;
using McFileIo.Attributes;
using McFileIo.Interfaces;
using McFileIo.Interfaces.McObject;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Items
{
    public class InContainerItem : INbtIoCapable, IMultiIdCapable
    {
        [NbtEntry]
        public byte Count;

        [NbtEntry(Optional: true)]
        public byte Slot;

        [NbtEntry("id")]
        public string Id;

        [NbtEntry("tag", Optional: true)]
        public NbtCompound Tag = null;

        [NbtEntry(Optional: true)]
        public short? Damage;

        public bool IsNamespacedNameUsed => Damage == null;
    }
}
