using McFileIo.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    [ApplyTo("piston")]
    public class Piston : BlockEntity
    {
        // TODO: blockState

        public enum FacingDirection
        {
            Down = 0,
            Up = 1,
            North = 2,
            South = 3,
            West = 4,
            East = 5
        }

        [NbtEntry]
        private int facing;

        [NbtEntry("progress")]
        public float Progress;

        [NbtEntry("extending")]
        public bool Extending;

        [NbtEntry("source")]
        public bool Source;

        public FacingDirection Facing
        {
            get => (FacingDirection)facing;
            set => facing = (int)value;
        }
    }
}
