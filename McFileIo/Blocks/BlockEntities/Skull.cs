using McFileIo.Attributes;
using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    [ApplyTo("skull")]
    public class Skull : BlockEntity
    {
        // TODO

        [NbtEntry]
        public SkullOwner Owner;

        public class SkullOwner : INbtIoCapable
        {
            [NbtEntry]
            public string Id;

            [NbtEntry]
            public string Name;
        }
    }
}
