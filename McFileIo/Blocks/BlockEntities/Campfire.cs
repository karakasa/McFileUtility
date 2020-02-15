using McFileIo.Attributes;
using McFileIo.Interfaces.McObject;
using McFileIo.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    [ApplyTo("campfire")]
    public class Campfire : BlockEntity, IContainerCapable
    {
        [NbtEntry]
        public int[] CookingTimes;

        [NbtEntry]
        public int[] CookingTotalTimes;

        [NbtEntry]
        private List<InContainerItem> Items { get; set; }

        public IList<InContainerItem> InContainerItems => Items;
    }
}
