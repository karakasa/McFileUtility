using McFileIo.Attributes;
using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    public class Coordinate : INbtIoCapable
    {
        [NbtEntry]
        public int X;

        [NbtEntry]
        public int Y;

        [NbtEntry]
        public int Z;
    }
}
