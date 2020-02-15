using McFileIo.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    [ApplyTo("jigsaw")]
    public class Jigsaw : BlockEntity
    {
        [NbtEntry("target_pool")]
        public string TargetPool;

        [NbtEntry("final_state")]
        public string FinalState;

        [NbtEntry("attachment_type")]
        public string AttachmentType;
    }
}
