using fNbt;
using McFileIo.Attributes;
using McFileIo.Interfaces;
using McFileIo.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    /// <summary>
    /// BlockEntity of Sign
    /// </summary>
    [ApplyTo("sign")]
    public class Sign : BlockEntity, INbtIoCapable
    {
        /// <summary>
        /// First line
        /// </summary>
        [NbtEntry]
        public string Text1;

        /// <summary>
        /// Second line
        /// </summary>
        [NbtEntry]
        public string Text2;

        /// <summary>
        /// Third line
        /// </summary>
        [NbtEntry]
        public string Text3;

        /// <summary>
        /// Fourth line
        /// </summary>
        [NbtEntry]
        public string Text4;

        /// <summary>
        /// Color
        /// </summary>
        [NbtEntry(Optional: true)]
        public string Color;
    }
}
