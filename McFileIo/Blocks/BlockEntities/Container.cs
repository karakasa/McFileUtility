using McFileIo.Attributes;
using McFileIo.Interfaces.McObject;
using McFileIo.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    public abstract class Container : BlockEntity, ICustomNameCapable, IContainerCapable, ILockable
    {
        [NbtEntry(Optional: true)]
        public string CustomName { get; set; } = null;

        [NbtEntry(Optional: true)]
        public string Lock { get; set; }

        [NbtEntry(Optional: true)]
        protected List<InContainerItem> Items { get; set; } = null;

        public IList<InContainerItem> InContainerItems => Items;
    }
}
