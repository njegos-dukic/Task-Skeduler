using System;
using System.Collections.Generic;

namespace OPOS
{
    /// <summary>
    /// Thread-safe sorted list that allows duplicate keys and sorts them in FIFO order.
    /// </summary>
    /// <typeparam name="T">Type of items to be stored in the list.</typeparam>
    public class ConcurrentSortedList<T> where T : class
    {
        // Using redefined comparator because classic SortedList doesn't allow duplicate keys.
        private readonly SortedList<int, T> _list = new SortedList<int, T>(new DualKeyComparator<int>());
        // Lock object used for enabling atomic operations.
        private readonly object _lock = new object();

        /// <summary>
        /// Adds (Key, Value) pair to the list.
        /// </summary>
        /// <param name="key">Integer key for sorting.</param>
        /// <param name="value">Value associated with the key.</param>
        public void Add(int key, T value)
        {
            lock (_lock)
                _list.Add(key, value);
        }

        /// <summary>
        /// Removes most prioritized (Key, Value) pair.
        /// </summary>
        /// <returns>Returns removed KVP or null if the list is empty.</returns>
        public T GetFirst()
        {
            if (_list.Count == 0)
                return null;

            lock (_lock)
            {
                T topTask = _list.Values[0];
                _list.RemoveAt(0);
                return topTask;
            }
        }

        /// <summary>
        /// Peeks the most prioritized (Key, Value) pair.
        /// </summary>
        /// <returns>KVP from the top of the list without removing it.</returns>
        public T GetFirstWithoutRemoving()
        {
            if (_list.Count == 0)
                return null;
            
            lock (_lock)
                return _list.Values[0];
        }
    }

    /// <summary>
    /// Helper class that extends IComparer and redefines Compare to allow duplicate keys and FIFO sorting.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    class DualKeyComparator<TKey> : IComparer<TKey> where TKey : IComparable
    {
        /// <summary>
        /// Method for comparing two keys.
        /// </summary>
        /// <param name="x">First key.</param>
        /// <param name="y">Second key.</param>
        /// <returns>-1 if keys are equal, treating equality as being smaller, otherwise result of x.CompareTo(y);</returns>
        public int Compare(TKey x, TKey y) { return x.CompareTo(y) == 0 ? -1 : x.CompareTo(y); }
    }
}
