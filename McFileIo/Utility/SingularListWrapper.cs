using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Utility
{
    /// <summary>
    /// An IList<typeparamref name="T"/> wrapper for one single element
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingularListWrapper<T> : IList<T>
    {
        private T _data;

        public bool IsEmpty { get; private set; }

        public SingularListWrapper()
        {
            IsEmpty = true;
        }

        public SingularListWrapper(T obj)
        {
            _data = obj;
            IsEmpty = false;
        }

        public T this[int index]
        {
            get
            {
                if (IsEmpty || index != 0) throw new ArgumentOutOfRangeException();
                return _data;
            }
            set
            {
                if (IsEmpty || index != 0) throw new ArgumentOutOfRangeException();
                _data = value;
            }
        }

        public int Count => IsEmpty ? 0 : 1;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            if (!IsEmpty) throw new NotSupportedException();

            _data = item;
            IsEmpty = false;
        }

        public void Clear()
        {
            _data = default;
            IsEmpty = true;
        }

        public bool Contains(T item)
        {
            return !IsEmpty && item.Equals(_data);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (!IsEmpty)
            {
                if (array.Length - arrayIndex < 1) throw new ArgumentException(nameof(array));
                array[arrayIndex] = _data;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new WrapperEnumerator(this);
        }

        public int IndexOf(T item)
        {
            if (IsEmpty) return -1;
            return item.Equals(_data) ? 0 : -1;
        }

        public void Insert(int index, T item)
        {
            if (index != 0) throw new NotSupportedException();
            Add(item);
        }

        public bool Remove(T item)
        {
            if (Contains(item)) {
                Clear();
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            if (index != 0) throw new NotSupportedException();
            if (IsEmpty) throw new ArgumentOutOfRangeException();

            Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new WrapperEnumerator(this);
        }

        private class WrapperEnumerator : IEnumerator<T>
        {
            private readonly SingularListWrapper<T> _wrapper;
            private int index = 0;

            public WrapperEnumerator(SingularListWrapper<T> wrapper)
            {
                _wrapper = wrapper;
            }

            public T Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_wrapper.IsEmpty) return false;
                if (index == 0) {
                    index = 1;
                    Current = _wrapper._data;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                index = 0;
                Current = default;
            }
        }
    }
}
