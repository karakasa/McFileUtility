using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Utility
{
    public class ListSeqAccessor<T> : ISequenceAccessor<T>
    {
        private IList<T> _ilist;

        public ListSeqAccessor(IList<T> baseObject)
        {
            _ilist = baseObject;
        }

        public T this[int index] {
            get => _ilist[index];
            set => _ilist[index] = value;
        }

        public int Length => _ilist.Count;
    }
}
