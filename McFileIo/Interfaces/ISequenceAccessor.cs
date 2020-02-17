using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces
{
    public interface ISequenceAccessor<T>
    {
        T this[int index]
        {
            get;
            set;
        }
    }
}
