using fNbt;
using McFileIo.World;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Utility
{
    public static class EnumTranslate
    {
        public static NbtCompression ToNbtCompression(this ChunkCompressionType type)
        {
            if (type == ChunkCompressionType.GZip) return NbtCompression.GZip;
            if (type == ChunkCompressionType.ZLib) return NbtCompression.ZLib;
            return NbtCompression.AutoDetect;
        }
    }
}
