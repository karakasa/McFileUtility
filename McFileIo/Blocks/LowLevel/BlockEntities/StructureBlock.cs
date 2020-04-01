using McFileIo.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.LowLevel.BlockEntities
{
    [ApplyTo("structure_block")]
    public class StructureBlock : BlockEntity
    {
        [NbtEntry("name")]
        public string Name;

        [NbtEntry("author")]
        public string Author;

        [NbtEntry("metadata")]
        public string Metadata;

        [NbtEntry("posX")]
        public int PosX;

        [NbtEntry("posY")]
        public int PosY;

        [NbtEntry("posZ")]
        public int PosZ;

        [NbtEntry("sizeX")]
        public int SizeX;

        [NbtEntry("sizeY")]
        public int SizeY;

        [NbtEntry("sizeZ")]
        public int SizeZ;

        [NbtEntry("rotation")]
        public string Rotation;

        [NbtEntry("mirror")]
        public string Mirror;

        [NbtEntry("mode")]
        public string Mode;

        [NbtEntry("integrity")]
        public float? Integrity;

        [NbtEntry("seed")]
        public long? Seed;

        [NbtEntry("ignoreEntities")]
        public bool? IgnoreEntities;

        [NbtEntry("showboundingbox")]
        public bool? ShowBoundingBox;

        [NbtEntry("powered")]
        public bool? Powered;
    }
}
