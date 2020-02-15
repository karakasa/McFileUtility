using McFileIo.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces.McObject
{
    public interface IContainerCapable
    {
        IList<InContainerItem> InContainerItems { get; }
    }
}
