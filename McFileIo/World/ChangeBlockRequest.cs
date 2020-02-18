using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.World
{
    public struct ChangeBlockRequest
    {
        public ChangeBlockRequest(int x, int y, int z, int inListIndex)
        {
            X = x;
            Y = y;
            Z = z;
            InListIndex = inListIndex;
        }

        public int X;
        public int Y;
        public int Z;
        public int InListIndex;
    }
}
