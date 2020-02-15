using McFileIo.Attributes;
using McFileIo.Interfaces.McObject;
using McFileIo.Items;
using McFileIo.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace McFileIo.Blocks.BlockEntities
{
    [ApplyTo("jukebox")]
    public class Jukebox : BlockEntity, IContainerCapable
    {
        [NbtEntry]
        public InContainerItem RecordItem;

        private IList<InContainerItem> _virtualItems = null;

        public IList<InContainerItem> InContainerItems
        {
            get
            {
                if (_virtualItems == null)
                    _virtualItems = new SingularListWrapper<InContainerItem>(RecordItem);

                return _virtualItems;
            }
        }
    }
}
