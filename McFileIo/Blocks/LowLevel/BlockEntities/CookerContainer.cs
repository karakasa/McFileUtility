using fNbt;
using McFileIo.Attributes;
using McFileIo.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.LowLevel.BlockEntities
{
    [ApplyTo("furnace")]
    [ApplyTo("smoker")]
    [ApplyTo("blast_furnace")]
    public class CookerContainer : Container
    {
        [NbtEntry]
        public short BurnTime;

        [NbtEntry]
        public short CookTime;

        [NbtEntry(Optional: true)]
        public short CookTimeTotal;

        [NbtEntry(Optional: true)]
        public short RecipeUsedSize;

        public bool IsEmpty => Items == null || Items.Count < 3 
            || (Items[0] == null && Items[1] == null && Items[2] == null);

        public InContainerItem ItemBeingSmelted { get => Items[0]; set => Items[0] = value; }
        public InContainerItem ItemFuel { get => Items[1]; set => Items[1] = value; }
        public InContainerItem ItemResult { get => Items[2]; set => Items[2] = value; }
    }
}
