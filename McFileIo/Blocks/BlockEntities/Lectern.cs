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
    [ApplyTo("lectern")]
    public class Lectern : BlockEntity, IContainerCapable
    {
        [NbtEntry]
        public InContainerItem Book;

        private IList<InContainerItem> _virtualItems = null;

        public IList<InContainerItem> InContainerItems
        {
            get
            {
                if (_virtualItems == null)
                    _virtualItems = new SingularListWrapper<InContainerItem>(Book);

                return _virtualItems;
            }
        }

        [NbtEntry(Optional: true)]
        public int? Page;
    }
}
