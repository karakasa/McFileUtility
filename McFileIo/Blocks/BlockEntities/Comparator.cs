using McFileIo.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    [ApplyTo("comparator")]
    public class Comparator : BlockEntity
    {
        [NbtEntry]
        public int OutputSignal;
    }
}
