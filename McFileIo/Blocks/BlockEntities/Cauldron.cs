using McFileIo.Attributes;
using McFileIo.Interfaces.McObject;
using McFileIo.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    [ApplyTo("cauldron")]
    public class Cauldron : BlockEntity, IContainerCapable
    {
        [NbtEntry]
        public int CustomColor;

        [NbtEntry]
        private List<InContainerItem> Items { get; set; } = null;

        public short PotionId;

        [NbtEntry("SplashPotion")]
        private byte _splashPotion = 0;

        public bool SplashPotion { get => _splashPotion == 1; set => _splashPotion = (byte)(value ? 1 : 0); }

        [NbtEntry("isMovable")]
        private byte _isMovable = 1;

        public bool IsMovable { get => _isMovable == 1; set => _isMovable = (byte)(value ? 1 : 0); }

        public IList<InContainerItem> InContainerItems => Items;
    }
}
