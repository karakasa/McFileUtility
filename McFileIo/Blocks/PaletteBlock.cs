using fNbt;
using McFileIo.Attributes;
using McFileIo.Interfaces;
using McFileIo.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks
{
    public class PaletteBlock : INbtIoCapable, INbtCustomReader
    {
        public void Read(INbtIoContext context, NbtCompound activeNode)
        {
            if (!activeNode.TryGet<NbtString>(nameof(Name), out var str)) return;
            Name = str.Value;

            if (activeNode.TryGet<NbtCompound>(nameof(Properties), out var properties))
                Properties = properties;
        }

        [NbtEntry]
        public string Name;

        [NbtEntry(Optional: true)]
        public NbtCompound Properties;

        public static readonly PaletteBlock AirBlock = new PaletteBlock()
        {
            Name = "minecraft:air"
        };
    }
}
