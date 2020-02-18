using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Utility
{
    public class ArraySeqAccessor<T> : ISequenceAccessor<T>
    {
        private readonly T[] _array;

        public ArraySeqAccessor(T[] array)
        {
            _array = array;
        }

        public T this[int index] {
            get => _array[index];
            set => _array[index] = value;
        }

        public int Length => _array.Length;
    }
}
