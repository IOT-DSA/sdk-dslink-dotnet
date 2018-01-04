using System.Collections;
using System.Collections.Generic;

namespace DSLink.Util
{
    public class ReadOnlyList<T> : IList<T>
    {
        private readonly IList<T> _backedList;
        public int Count => _backedList.Count;
        public bool IsReadOnly => true;

        public ReadOnlyList(IList<T> backedList)
        {
            _backedList = backedList;
        }

        public T this[int index]
        {
            get
            {
                return _backedList[index];
            }
            set
            {
                throw new ReadOnlyException();
            }
        }

        public void Add(T item)
        {
            throw new ReadOnlyException();
        }

        public void Clear()
        {
            throw new ReadOnlyException();
        }

        public bool Contains(T item)
        {
            return _backedList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new ReadOnlyException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _backedList.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _backedList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            throw new ReadOnlyException();
        }

        public bool Remove(T item)
        {
            throw new ReadOnlyException();
        }

        public void RemoveAt(int index)
        {
            throw new ReadOnlyException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
