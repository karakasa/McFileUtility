using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockProperties
{
    public abstract class BlockProperty : INbtIoCapable, ICloneable, IEquatable<BlockProperty>
    {
        public abstract object Clone();

        public abstract bool Equals(BlockProperty other);
    }
}
