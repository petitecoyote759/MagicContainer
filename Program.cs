using System.Text;
using System.Collections;

#pragma warning disable IDE0028 // simplify collection definition, i dont like how it looks
#pragma warning disable IDE0090 // using new() looks worse than new List() and that is more clear
#pragma warning disable IDE0025 // property without body, easier to understand like that
#pragma warning disable IDE0022 // method without body, usually bad but here is just used to forward to another method
#pragma warning disable IDE0003 // remove the this from this.Function, in some cases this.Function looks better
#pragma warning disable CA1710 // Should have stack or collection after, its a container and i think that is a better more clear name
#pragma warning disable CA1515 // The class should be internal, this is to be published so it must be public
#pragma warning disable CA1805 // Variables initialised to default value. This makes it clear to read.

namespace ShortTools.MagicContainer
{
    /// <summary>
    /// A class inspired by "Pezzza's Work" on youtube. This container has an O(1) access, delete, and add functionality. The cost is that this is more 
    /// memory intensive than other containers, with an extra 2 integers per element.
    /// </summary>
    /// <typeparam name="T">The type contained in the container.</typeparam>
    public sealed class SMContainer<T> : ICollection<T>, ICloneable, IEnumerator<T>, IList<T>
    {
        private List<int> dataIndex = new List<int>();
        private List<int> ID = new List<int>();
        private List<T> data = new List<T>();

        /// <summary>
        /// The length of the container
        /// </summary>
        public int Length { get => length; }
        private int length = 0;

        // length of the dataIndex and ID lists as they never go down.
        private int maxLength = 0;


        public T this[int index]
        {
            get => data[dataIndex[index]];
            set => data[dataIndex[index]] = value;
        }



        #region ICollection

        /// <summary>
        /// Adds an element to the <see cref="SMContainer{T}"/> and returns the index it was added at.
        /// </summary>
        /// <param name="item">The item to be added</param>
        /// <returns>The index of the element in the collection.</returns>
        public int Add(T item)
        {
            data.Add(item);

            // length will now point to the next item
            if (length >= maxLength)
            {
                ID.Add(length);
                dataIndex.Add(length);
                // add the stuff to dataIndex and ID
                maxLength++;
            }

            length++;
            return ID[length - 1];
        }
        void ICollection<T>.Add(T item) => this.Add(item);


        /// <summary>
        /// Removes the element at the given index.
        /// </summary>
        /// <param name="index">The index of the item to be removed.</param>
        /// <returns>True if successful, false if not.</returns>
        public bool RemoveAt(int index)
        {
            if (index >= length) { return false; }

            int dIndex = dataIndex[index]; // index of the data

            length--;

            // now to swap and pop, but first swapping

            data[dIndex] = data[length];
            (ID[length], ID[dIndex]) = (ID[dIndex], ID[length]); // swap IDs

            // pop
            // now we need to update the data index at ID[length] to be correct, and the one at ID[dIndex] by swapping
            (dataIndex[ID[length]], dataIndex[ID[dIndex]]) = (dataIndex[ID[dIndex]], dataIndex[ID[length]]); // swap data indexes
            data.RemoveAt(length);

            return true;
        }



        /// <summary>
        /// Removes all elements from <see cref="SMContainer{T}"/>
        /// </summary>
        public void Clear()
        {
            dataIndex.Clear();
            ID.Clear();
            data.Clear();
            length = 0;
            maxLength = 0;
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
            if (array.Length < arrayIndex + length) { throw new ArgumentException($"Given array was of an insufficient size. Array Length : {array.Length}, Collection Length : {length}"); }

            for (int i = arrayIndex; i < length; i++)
            {
                array[i] = data[i - arrayIndex];
            }
        }




        /// <summary>
        /// Removes the first item that is .Equals to the given item
        /// </summary>
        /// <param name="item">The item to be removed</param>
        /// <returns>True if successfully removed, false if not.</returns>
        public bool Remove(T item)
        {
            for (int i = 0; i < length; i++)
            {
                if (item?.Equals(data[i]) == true)
                {
                    _ = RemoveAt(ID[i]);
                    return true;
                }
            }
            return false;
        }




        [Obsolete("Do not use this function, use .Length instead.", error: false)]
        int ICollection<T>.Count => length;


        public bool IsReadOnly => false;

        






        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            // call the generic version of the method
            return this.GetEnumerator();
        }

        #endregion ICollection


        #region IClonable

        public SMContainer<T> Clone()
        {
            return new SMContainer<T>()
            {
                dataIndex = this.dataIndex,
                ID = this.ID,
                data = this.data
            };
        }
        object ICloneable.Clone() { return this.Clone(); }

        #endregion IClonable


        #region IEnumerator

        private int eIndex = -1; // enumerator index;

        T IEnumerator<T>.Current => this[eIndex];
        object? IEnumerator.Current => this[eIndex];
        bool IEnumerator.MoveNext() 
        { 
            eIndex++;
            return eIndex < length;
        }
        void IEnumerator.Reset() { eIndex = -1; }


        public void Dispose() 
        {
            this.Clear();
        }

        #endregion IEnumerator


        #region IList

        /// <summary>
        /// Gets the index of the given target, or -1 if it is not there
        /// </summary>
        /// <param name="target">Item to be searched for</param>
        /// <returns>Index of the item, or -1 if it is not in the collection.</returns>
        public int IndexOf(T target)
        {
            for (int i = 0; i < length; i++)
            {
                if (target?.Equals(data[i]) == true)
                {
                    return ID[i];
                }
            }
            return -1;
        }

        /// <summary>
        /// Inserts the given item, at the index. If the index is above the length it will not add it.
        /// </summary>
        /// <param name="index">The index of the item to be added.</param>
        /// <param name="item">The item to be added.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when index > length</exception>
        public void Insert(int index, T item)
        {
            if (index < length)
            {
                data[dataIndex[index]] = item;
            }
            else if (index == length)
            {
                _ = Add(item);
            }
            throw new ArgumentOutOfRangeException($"Parameter index ({index}) was out of range.");
            // too far out, ignore it
        }

        /// <summary>
        /// Removes the item at the given index.
        /// </summary>
        /// <param name="index">The index of the item to be removed.</param>
        void IList<T>.RemoveAt(int index)
        {
            _ = this.RemoveAt(index);
        }

        #endregion IList


        #region Constructors

        public SMContainer() { }

        public SMContainer(ICollection<T> collection) 
        { 
            if (collection is null) { return; }

            int length = collection.Count;
            for (int i = 0; i < length; i++)
            {
                _ = this.Add(collection.ElementAt(i));
            }
        }


        #endregion Constructors






        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            _ = builder.Append('[');
            for (int i = 0; i < length - 1; i++)
            {
                _ = builder.Append(data[i]);
                _ = builder.Append(", ");
            }
            if (length > 0) { _ = builder.Append(data[length - 1]); }
            _ = builder.Append(']');

            return builder.ToString();
        }
        public string ToString(bool displayExtra)
        {
            if (!displayExtra) { return this.ToString(); }


            StringBuilder builder = new StringBuilder();

            _ = builder.Append('[');
            for (int i = 0; i < maxLength - 1; i++)
            {
                _ = builder.Append(dataIndex[i]);
                _ = builder.Append(", ");
            }
            if (maxLength > 0) { _ = builder.Append(dataIndex[maxLength - 1]); }
            _ = builder.AppendLine("]");
            

            _ = builder.Append('[');
            for (int i = 0; i < maxLength - 1; i++)
            {
                _ = builder.Append(ID[i]);
                _ = builder.Append(", ");
            }
            if (maxLength > 0) { _ = builder.Append(ID[maxLength - 1]); }
            _ = builder.AppendLine("]");


            _ = builder.Append(this.ToString());

            return builder.ToString();
        }

    }

























    internal static class Tester
    {
        private static void Main()
        {
            using SMContainer<int> container = new SMContainer<int>([0, 4, 7, 10, 12, 13, 16, 200, 19, 34]);
            
            Console.WriteLine($"{container.ToString(true)}\n");

            _ = container.RemoveAt(3);

            Console.WriteLine($"{container.ToString(true)}\n");
        }
    }
}