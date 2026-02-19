using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

#pragma warning disable IDE0028 // simplify collection definition, i dont like how it looks
#pragma warning disable IDE0090 // using new() looks worse than new List() and that is more clear
#pragma warning disable IDE0025 // property without body, easier to understand like that
#pragma warning disable IDE0022 // method without body, usually bad but here is just used to forward to another method
#pragma warning disable IDE0003 // remove the this from this.Function, in some cases this.Function looks better
#pragma warning disable CA1710 // Should have stack or collection after, its a container and i think that is a better more clear name
#pragma warning disable CA1515 // The class should be internal, this is to be published so it must be public
#pragma warning disable CA1805 // Variables initialised to default value. This makes it clear to read.

#if DEBUG
#pragma warning disable CA1303 // Use a string dictionary rather than hard coding messages, not relevant in debugging
#endif

namespace ShortTools.MagicContainer
{
    /// <summary>
    /// A class inspired by "Pezzza's Work" on youtube. This container has an O(1) access, delete, and add functionalies. The cost is that this is more 
    /// memory intensive than other containers, with an extra 2 integers per element.
    /// </summary>
    /// <typeparam name="T">The type contained in the container.</typeparam>
    [DebuggerDisplay("Length : {_length}, _maxLength : {_maxLength}, Data : {string.Join(',', _data)}")] //string.Join(",", Client)
    public sealed class SMContainer<T> : ICollection<T>, ICollection, IEnumerable<T>, IEnumerable, ICloneable, IList<T>, IList
    {
        private List<int> _dataIndex = new List<int>();
        private List<int> _ID = new List<int>();
        private List<T> _data = new List<T>();

        /// <summary>
        /// The length of the container
        /// </summary>
        public int Length { get => _length; }
        private int _length = 0;

        // length of the dataIndex and ID lists as they never go down.
        private int _maxLength = 0;


        public T this[int index]
        {
            get => _data[_dataIndex[index]];
            set => _data[_dataIndex[index]] = value;
        }



        #region ICollection

        /// <summary>
        /// Adds an element to the <see cref="SMContainer{T}"/> and returns the index it was added at.
        /// </summary>
        /// <param name="item">The item to be added</param>
        /// <returns>The index of the element in the collection.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public int Add(T item)
        {
            _data.Add(item);

            // length will now point to the next item
            if (_length >= _maxLength)
            {
                _ID.Add(_length);
                _dataIndex.Add(_length);
                // add the stuff to dataIndex and ID
                _maxLength++;
            }

            _length++;
            return _ID[_length - 1];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICollection<T>.Add(T item) => this.Add(item);
        int IList.Add(object? value)
        {
            return value is T item ? this.Add(item) : -1;
        }


        /// <summary>
        /// Removes the element at the given index.
        /// </summary>
        /// <param name="index">The index of the item to be removed.</param>
        /// <returns>True if successful, false if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public bool RemoveAt(int index)
        {
            if (index >= _length) { return false; }

            int dIndex = _dataIndex[index]; // index of the data
            if (dIndex >= _length) { return false; } // already deleted

            _length--;

            // now to swap and pop, but first swapping

            _data[dIndex] = _data[_length];
            (_ID[_length], _ID[dIndex]) = (_ID[dIndex], _ID[_length]); // swap IDs

            // pop
            // now we need to update the data index at ID[length] to be correct, and the one at ID[dIndex] by swapping
            (_dataIndex[_ID[_length]], _dataIndex[_ID[dIndex]]) = (_dataIndex[_ID[dIndex]], _dataIndex[_ID[_length]]); // swap data indexes
            _data.RemoveAt(_length);

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IList.RemoveAt(int index)
        {
            _ = RemoveAt(index);
        }



        /// <summary>
        /// Removes all elements from <see cref="SMContainer{T}"/>
        /// </summary>
        public void Clear()
        {
#if DEBUG
            Console.WriteLine("Clearing magic container.");
#endif
            _dataIndex.Clear();
            _ID.Clear();
            _data.Clear();
            _length = 0;
            _maxLength = 0;
        }



        /// <summary>
        /// Checks for if the given item is in the <see cref="SMContainer{T}"/>.
        /// </summary>
        /// <param name="value">The item to see if it is in the container.</param>
        /// <returns>True if it is in the container, and false if it is not.</returns>
        public bool Contains(T value)
        {
            return this.IndexOf(value) != -1;
        }
        bool IList.Contains(object? value)
        {
            return value is T item && this.Contains(item);
        }



        /// <summary>
        /// Copies the collections data to the target array.
        /// </summary>
        /// <param name="array">Array to be copied to.</param>
        /// <param name="arrayIndex">Starting index.</param>
        /// <exception cref="ArgumentException">Thrown if the given array was too small.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array);
            ArgumentOutOfRangeException.ThrowIfLessThan(arrayIndex, 0);
            if (array.Length < arrayIndex + _length) { throw new ArgumentException($"Given array was of an insufficient size. Array Length : {array.Length}, Collection Length : {_length}"); }

            for (int i = arrayIndex; i < _length; i++)
            {
                array[i] = _data[i - arrayIndex];
            }
        }
        public void CopyTo(Array array, int index)
        {
            ArgumentNullException.ThrowIfNull(array);
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
            if (array.Length < index + _length) { throw new ArgumentException($"Given array was of an insufficient size. Array Length : {array.Length}, Collection Length : {_length}"); }

            for (int i = index; i < _length; i++)
            {
                array.SetValue(_data[i - index], i);
            }
        }




        /// <summary>
        /// Removes the first item that is .Equals to the given item
        /// </summary>
        /// <param name="item">The item to be removed</param>
        /// <returns>True if successfully removed, false if not.</returns>
        public bool Remove(T item)
        {
            for (int i = 0; i < _length; i++)
            {
                if (item?.Equals(_data[i]) == true)
                {
                    _ = RemoveAt(_ID[i]);
                    return true;
                }
            }
            return false;
        }
        void IList.Remove(object? value)
        {
            if (value is T item) { _ = this.Remove(item); }
        }




        [Obsolete("Do not use this function, use .Length instead.", error: false)]
        int ICollection<T>.Count => _length;
        [Obsolete("Do not use this function, use .Length instead.", error: false)]
        int ICollection.Count => _length;

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot => this;


        public bool IsReadOnly => false;




        #endregion ICollection


        #region IClonable

        public SMContainer<T> Clone()
        {
            return new SMContainer<T>()
            {
                _dataIndex = this._dataIndex,
                _ID = this._ID,
                _data = this._data
            };
        }
        object ICloneable.Clone() { return this.Clone(); }

        #endregion IClonable


        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return new SMContainerEnumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            // call the generic version of the method
            return this.GetEnumerator();
        }



        #endregion IEnumerable


        #region IList

        /// <summary>
        /// Gets the index of the given target, or -1 if it is not there
        /// </summary>
        /// <param name="target">Item to be searched for</param>
        /// <returns>Index of the item, or -1 if it is not in the collection.</returns>
        public int IndexOf(T target)
        {
            for (int i = 0; i < _length; i++)
            {
                if (target?.Equals(_data[i]) == true)
                {
                    return _ID[i];
                }
            }
            return -1;
        }
        int IList.IndexOf(object? value)
        {
            return value is T item ? this.IndexOf(item) : -1;
        }

        /// <summary>
        /// Inserts the given item, at the index. If the index is above the length it will not add it.
        /// </summary>
        /// <param name="index">The index of the item to be added.</param>
        /// <param name="item">The item to be added.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when index > length</exception>
        public void Insert(int index, T item)
        {
            if (index < _length)
            {
                _data[_dataIndex[index]] = item;
            }
            else if (index == _length)
            {
                _ = Add(item);
            }
            throw new ArgumentOutOfRangeException($"Parameter index ({index}) was out of range.");
            // too far out, ignore it
        }
        void IList.Insert(int index, object? value)
        {
            if (value is T item) { this.Insert(index, item); }
        }

        /// <summary>
        /// Removes the item at the given index.
        /// </summary>
        /// <param name="index">The index of the item to be removed.</param>
        void IList<T>.RemoveAt(int index)
        {
            _ = this.RemoveAt(index);
        }


        public bool IsFixedSize => false;

        object? IList.this[int index] 
        { 
            get => this[index];
            set
            {
                if (value is T item)
                {
                    this[index] = item;
                    return;
                }
                throw new ArgumentException($"Argument of type {value?.GetType()?.ToString() ?? "Null"} was the incorrect type");
            }
        }



        #endregion IList


        #region Constructors

        public SMContainer() { }

        public SMContainer(IEnumerable<T> collection)
        {
            if (collection is null) { return; }

            foreach (T item in collection)
            {
                _ = this.Add(item);
            }
        }


        #endregion Constructors




        #region AdditionalFunctions
        
        public T[] ToArray()
        {
            T[] outputArray = new T[_length];
            for (int i = 0; i < _length; i++)
            {
                outputArray[i] = _data[i];
            }
            return outputArray;
        }
        
        public bool RemoveRange(int start, int end)
        {
            if (end < start || end >= _length) { return false; }
        
            for (int i = start; i < end; i++)
            {
                _ = this.RemoveAt(i);
            }
            return true;
        }
        
        public void RemoveAll(Predicate<T> specifier) // return count
        {
            ArgumentNullException.ThrowIfNull(specifier);

            for (int i = 0; i < _length; i++)
            {
                if (specifier(this[i]))
                {
                    _ = this.RemoveAt(i);
                }
            }
        }
        
        public T Last(out int index)
        {
            index = _length - 1;
            return this[index];
        }
        
        public SMContainer<T> GetRange(int start, int end)
        {
            return new SMContainer<T>(this.GetRangeEnumerable(start, end));
        }
        
        public IEnumerable<T> GetRangeEnumerable(int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                yield return this[i];   
            }
        }
        
        public SMContainer<T> FindAll(Predicate<T> specifier)
        {
            ArgumentNullException.ThrowIfNull(specifier);

            SMContainer<T> output = new SMContainer<T>();
            
            for (int i = 0; i < _length; i++)
            {
                T item = this[i];
                if (specifier(item))
                {
                    _ = output.Add(item);
                }
            }
            return output;
        }
        
        public bool Exists(Predicate<T> specifier)
        {
            ArgumentNullException.ThrowIfNull(specifier);

            foreach (T item in _data)
            {
                if (specifier(item))
                {
                    return true;
                }
            }
            return false;
        }
        
        public void AddRange(IEnumerable<T> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            foreach (T item in items)
            {
                _ = this.Add(item);
            }
        }
        
        public SMContainer<T2> ConvertAll<T2>(Converter<T, T2> converter)
        {
            return new SMContainer<T2>() { _dataIndex = this._dataIndex, _ID = this._ID, _data = this._data.ConvertAll<T2>(converter) };
        }







        #endregion AdditionalFunctions










        #region IEnumerator

        public sealed class SMContainerEnumerator : IEnumerator, IEnumerator<T>
        {
            private int eIndex = -1; // enumerator index;
            SMContainer<T>? instance;

            public SMContainerEnumerator(SMContainer<T> instance)
            {
                this.instance = instance;
            }

#pragma warning disable CS8602 // instance will never be null unless calling post disposal
            T IEnumerator<T>.Current => instance[eIndex];
            object? IEnumerator.Current => instance[eIndex];
            bool IEnumerator.MoveNext()
            {
                while (true)
                {
                    eIndex++;
                    if (eIndex >= instance._length) { return false; }
                    if (instance._dataIndex[eIndex] < instance._length) { return true; }
                }
            }
            void IEnumerator.Reset() { eIndex = -1; }
#pragma warning restore CS8602 




            private bool disposed = false;
            public void Dispose()
            {
#if DEBUG
                Console.WriteLine("Non-explicit disposing.");
#endif
                Dispose(disposing: false);
                GC.SuppressFinalize(this);
            }
            public void Dispose(bool disposing)
            {
                if (!this.disposed)
                {
                    if (disposing)
                    {
#if DEBUG
                        Console.WriteLine("Explicit disposing.");
#endif
                    }
                    instance = null;
                    disposed = true;
                }
            }
        }

        #endregion IEnumerator













        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            _ = builder.Append('[');
            for (int i = 0; i < _length - 1; i++)
            {
                _ = builder.Append(_data[i]);
                _ = builder.Append(", ");
            }
            if (_length > 0) { _ = builder.Append(_data[_length - 1]); }
            _ = builder.Append(']');

            return builder.ToString();
        }
        public string ToString(bool displayExtra)
        {
            if (!displayExtra) { return this.ToString(); }


            StringBuilder builder = new StringBuilder();

            _ = builder.Append('[');
            for (int i = 0; i < _maxLength - 1; i++)
            {
                _ = builder.Append(_dataIndex[i]);
                _ = builder.Append(", ");
            }
            if (_maxLength > 0) { _ = builder.Append(_dataIndex[_maxLength - 1]); }
            _ = builder.AppendLine("]");


            _ = builder.Append('[');
            for (int i = 0; i < _maxLength - 1; i++)
            {
                _ = builder.Append(_ID[i]);
                _ = builder.Append(", ");
            }
            if (_maxLength > 0) { _ = builder.Append(_ID[_maxLength - 1]); }
            _ = builder.AppendLine("]");


            _ = builder.Append(this.ToString());

            return builder.ToString();
        }

    }
























#pragma warning disable CA1303
    internal static class Tester
    {
        private static bool running = true;
        private static readonly ManualResetEvent modifierCompleted = new ManualResetEvent(false);

        private static void Main()
        {
            SMContainer<int> container = new SMContainer<int>([0, 4, 7, 10, 12, 13, 16, 200, 19, 34]);

            Console.WriteLine($"{container.ToString(true)}\n");

            Thread modifierThread = new Thread(new ThreadStart(() => ModifyContainer(container)));
            modifierThread.Start();

            Thread.Sleep(100);

            int count = 0;
            foreach (int value in container)
            {
                count++;
                Console.WriteLine($"Value: {value}");
            }
            Console.WriteLine($"Count: {count}");

            foreach (int value in container)
            {
                Console.WriteLine($"Value: {value}");
            }

            Thread.Sleep(100);

            running = false;
            modifierThread.Join();
            _ = modifierCompleted.WaitOne();

            Console.WriteLine($"{container.ToString(true)}\n");

            Console.WriteLine("Ending");
        }

#pragma warning disable CA5394 // use cryptographically secure random, not required here
        private static void ModifyContainer(SMContainer<int> container)
        {
            Random random = new Random();

            bool adding = false;

            while (running)
            {
                if (adding)
                {
                    _ = container.Add(random.Next(100));
                }
                else
                {
                    int index = random.Next(container.Length);
                    _ = container.RemoveAt(index);
                }
                adding = !adding;
            }
            _ = modifierCompleted.Set();
            Console.WriteLine("Completed");
        }
#pragma warning restore CA5394
    }
}