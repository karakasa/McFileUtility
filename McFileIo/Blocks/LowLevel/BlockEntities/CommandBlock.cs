using McFileIo.Attributes;
using McFileIo.Interfaces.McObject;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.LowLevel.BlockEntities
{
    [ApplyTo("command_block")]
    public class CommandBlock : BlockEntity, ICustomNameCapable
    {
        [NbtEntry(Optional: true)]
        public string CustomName { get; set; }

        [NbtEntry]
        public string Command;

        [NbtEntry]
        public int SuccessCount;

        [NbtEntry]
        public string LastOutput;

        [NbtEntry]
        public bool TrackOutput;

        [NbtEntry("powered")]
        public bool Powered;

        [NbtEntry("auto")]
        public bool Automatic;

        [NbtEntry("conditionMet")]
        public bool ConditionMet;

        [NbtEntry]
        public bool UpdateLastExecution;

        [NbtEntry]
        public long LastExecution;
    }
}
