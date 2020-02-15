using McFileIo.Attributes;
using McFileIo.Misc;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    [ApplyTo("conduit")]
    public class Conduit : BlockEntity
    {
        [NbtEntry("target_uuid", Optional: true)]
        public NbtUuid TargetUuid;
    }
}
