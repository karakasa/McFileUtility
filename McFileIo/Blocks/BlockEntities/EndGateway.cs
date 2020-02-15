using McFileIo.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    [ApplyTo(IdEndGateway)]
    public class EndGateway : BlockEntity
    {
        public const string IdEndGateway = "end_gateway";

        [NbtEntry("ExactTeleport")]
        private byte _exactTeleport = 0;

        public bool ExactTeleport { get => _exactTeleport == 1; set => _exactTeleport = (byte)(value ? 1 : 0); }

        [NbtEntry]
        public Coordinate ExitPortal;

        [NbtEntry]
        public long Age;
    }
}
