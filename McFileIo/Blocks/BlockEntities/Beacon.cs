using McFileIo.Attributes;
using McFileIo.Interfaces.McObject;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    [ApplyTo("beacon")]
    public class Beacon : BlockEntity, ILockable
    {
        [NbtEntry]
        public int Levels;

        [NbtEntry]
        public int Primary;

        [NbtEntry]
        public int Secondary;

        [NbtEntry]
        public string Lock { get; set; }
    }
}
