using System;
using System.Collections;
using System.Collections.Generic;

namespace DspSharp.Collections
{
    /// <summary>
    /// Implements a List of T like class that under the hood uses a circular buffer, so that the
    /// list has high performance with many adds and removes. The capacity of the list grows
    /// automatically as elements are added to the lsit.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CircularList<T> : IList<T>
    {
        private T[] buffer;

        /// <summary>
        /// Points to the first filled bucket.
        /// </summary>
        private int readPos;

        /// <summary>
        /// Points to the first empty bucket.
        /// </summary>
        private int writePos;

        /// <summary>
        /// The size of the buffer
        /// </summary>
        private int size;

        /// <summary>
        /// Initializes a new instance of the CircularList class that is empty with a default initial capacity.
        /// </summary>
        public CircularList() : this( 2 ) { }

        /// <summary>
        /// Initializes a new instance of the CircularList class that is empty, with the given
        /// initial capacity.
        /// </summary>
        /// <param name="initialCapacity">
        /// The initial number of elements the list can store without reallocting memory.
        /// </param>
        public CircularList( int initialCapacity )
        {
            // Waste one byte so that when we're full, the head and tail pointers aren't on top of each other.
            this.size = initialCapacity + 1;

            this.buffer = new T[this.size];

            this.readPos = 0;
            this.writePos = 0;
            this.Count = 0;
        }

        /// <summary>
        /// Returns the number of elements stored in the list.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Returns the total number of elements that can be stored in the list without resizing the
        /// list.
        /// </summary>
        public int Capacity { get { return this.size; } }

        /// <summary>
        /// Gets or sets the element at the given index.
        /// </summary>
        public T this[int index]
        {
            get
            {
                if( index > this.Count - 1 )
                {
                    throw new IndexOutOfRangeException();
                }

                return this.buffer[( readPos + index ) % this.size];
            }
            set
            {
                if( index > this.size - 1 )
                {
                    throw new IndexOutOfRangeException();
                }

                this.buffer[( readPos + index ) % this.size] = value;
            }
        }

        /// <summary>
        /// Gets or sets the first element stored in the list.
        /// </summary>
        public T First
        {
            get => this.buffer[readPos];
            set => this.buffer[readPos] = value;
        }

        /// <summary>
        /// Get or sets the last element stored in the list.
        /// </summary>
        public T Last
        {
            get => this.buffer[( this.writePos - 1 + this.size ) % this.size];
            set => this.buffer[( this.writePos - 1 + this.size ) % this.size] = value;
        }

        /// <summary>
        /// Adds a new element to the end of the list.
        /// </summary>
        /// <param name="item"></param>
        public void Add( T item )
        {
            CheckCapacity();

            this.buffer[this.writePos] = item;

            this.writePos = ( this.writePos + 1 ) % this.size;
            this.Count++;
        }

        /// <summary>
        /// Removes all elements from the list.
        /// </summary>
        public void Clear()
        {
            this.readPos = 0;
            this.writePos = 0;
            this.Count = 0;
            Array.Clear( this.buffer, 0, this.buffer.Length );
        }

        /// <summary>
        /// Returns the index of the first item that is equivilent to the given item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf( T item )
        {
            int externalIndex = 0;
            for( int i = readPos; i < this.Count; i++ )
            {
                if( this.buffer[i % this.size].Equals( item ) )
                {
                    return externalIndex;
                }

                externalIndex++;
            }

            return -1;
        }

        /// <summary>
        /// Inserts an element into the list at the given index. To make room for the element,
        /// the element at or above the given index are moved to one position higher in the list.
        /// If the index points to the first empty index at the end of the list, the new element is
        /// inserted at the end of the list.
        /// </summary>
        /// <remarks>
        /// If the capacity of the list is insufficient to fit the new element, the list is automatically
        /// grown to accomodate the new element.
        ///
        /// Since the underlying data type is a circular array, the insert may be handled internally by
        /// moving items toward the tail, if the insert is nearer to the tail, or toward the head, if the
        /// insert is nearer the head, reducing the number of copies that need to be performed.
        /// </remarks>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert( int index, T item )
        {
            if( index > this.Count )
            {
                // Caller is inserting past the end of the list, which would leave a hole; not permitted.
                throw new IndexOutOfRangeException();
            }
            else if( index == this.Count )
            {
                // Caller is inserting at the exact tail of list. Allow it, and treat it as a regular add.
                Add( item );
            }
            else if( index == 0 )
            {
                // Caller is inserting at the exact beginning. Just move the head pointer back one and insert at
                // the new head.

                CheckCapacity();
                this.readPos = ( this.readPos - 1 + this.size ) % this.size;
                this.buffer[this.readPos] = item;
                this.Count++;
            }
            else
            {
                // Normal insert works like this:
                // Step 1:                 Step 2:                 Step 3:
                //        V                      V                      V
                //  0 1 2 3 4 5 6 7 8 9    0 1 2 3 4 5 6 7 8 9    0 1 2 3 4 5 6 7 8 9
                // ---------------------  ---------------------  ---------------------
                // |a|b|c|d|e|f| | | | |  |a|b|c| |d|e|f| | | |  |a|b|c|x|d|e|f| | | |
                // ---------------------  ---------------------  ---------------------
                //
                //
                // It's very important to note that, no matter what happens, the element thats
                // in the index that we want to insert at ('d', in above) has to end up ahead of
                // the inserted element. |c|d|...|  became  |c|x|d|...|.
                // If we're doing tail moves, then we take d, e, f and push them all up one, and then
                // insert where d was.
                // If we're doing head moves, then we *leave d where it is*, take c, b, a and push them
                // down one, and *insert where c was*.

                // -------------- Tail moves --------------
                // Step 1:                Step 2:                Step 3:
                //                  V                      V                      V
                //  0 1 2 3 4 5 6 7 8 9    0 1 2 3 4 5 6 7 8 9    0 1 2 3 4 5 6 7 8 9
                // ---------------------  ---------------------  ---------------------
                // |d|e|f| | | | |a|b|c|  |c|d|e|f| | | |a| |b|  |c|d|e|f| | | |a|x|b|
                // ---------------------  ---------------------  ---------------------
                //
                // -------------- Head moves --------------
                // Step 1:                Step 2:                Step 3:
                //                  V                    V                      V
                //  0 1 2 3 4 5 6 7 8 9    0 1 2 3 4 5 6 7 8 9    0 1 2 3 4 5 6 7 8 9
                // ---------------------  ---------------------  ---------------------
                // |d|e|f| | | | |a|b|c|  |d|e|f| | | |a| |b|c|  |d|e|f| | | |a|x|b|c|
                // ---------------------  ---------------------  ---------------------
                // The insert index went from 8 to 7 and we moved one element.

                int headMoves;
                int tailMoves;

                // First we have to make sure there's room. Do this first, since if we reallocate the
                // array, all of our pointers change.
                CheckCapacity();

                // Then, second, we have to figure out what kind of move we're going to do.
                // Compute how many moves each mode would take, and then perform the one
                // with fewer moves, prefering tail moves if they're the same.

                // If we're inserting at the head, then no moves are needed (just push the head
                // pointer back one and insert there). head == bufIndex --> headMoves = 0
                // Since index 0 == head, the number of move needed is just the index.
                headMoves = index;

                // If we're inserting at the tail, then no moves are needed. Insert at the tail,
                // move the tail pointer forward.
                // count = 6, index = 3, 3 moves. thus. tailMoves = Count - index.
                tailMoves = Count - index;

                if( headMoves < tailMoves )
                {
                    // Do the head move algorithm.
                    int curr;
                    int next;

                    // Move the head back so that it points to an empty space.
                    // Set curr = head (so that curr points to the empty cell).
                    // Copy the element in front of curr to curr, then increment curr.
                    // Finally, curr will be the empty cell to write the new value to.

                    this.readPos = ( this.readPos - 1 + this.size ) % this.size;

                    curr = readPos;

                    for( int i = 0; i < headMoves; i++ )
                    {
                        next = ( curr + 1 ) % this.size;
                        this.buffer[curr] = this.buffer[next];
                        curr = next;
                    }

                    this.buffer[curr] = item;

                    this.Count++;
                }
                else
                {
                    // Do the tail move algorithm.
                    int curr = writePos;
                    int prev;

                    // Move the elements forward one at a time, working from the tail backward.
                    for( int i = 0; i < tailMoves; i++ )
                    {
                        prev = ( curr - 1 + this.size ) % this.size;
                        this.buffer[curr] = this.buffer[prev];
                        curr = prev;
                    }

                    this.writePos = ( this.writePos + 1 ) % this.size;

                    this.buffer[curr] = item;

                    this.Count++;
                }
            }
        }

        /// <summary>
        /// Removes the element at the given index, and moves elements from higher indexes down one
        /// to fill the hole. The capacity of the list is not reduced as elements are removed.
        /// </summary>
        /// <remarks>
        /// Since the underlying storage is a circular list, elements will be moved from the head inward
        /// to fill the hole if the hole is nearer the head, or will be moved from the tail inward
        /// if the hole is nearer the tail, reducing the number of copies that need to be performed.
        /// </remarks>
        /// <param name="index"></param>
        public void RemoveAt( int index )
        {
            if( index >= this.Count )
            {
                throw new IndexOutOfRangeException();
            }
            else if( index == 0 )
            {
                this.buffer[readPos] = default( T );
                this.readPos = ( this.readPos + 1 ) % this.size;
                this.Count--;
            }
            else
            {
                // Close the hole.

                //  0 1 2 3 4 5 6 7 8
                // -------------------
                // |d|e|f| | | |a|b|c|
                // -------------------
                //        T     H I
                //
                // size = 9
                // tail = 3
                // head = 6
                // internalIndex = 7
                // moves should be 5.

                // Example: Delete 'g' at array index 6 by moving elements down from the tail
                //
                //  H           X   T      H           X   T      H           X T
                //  0 1 2 3 4 5 6 7 8 9    0 1 2 3 4 5 6 7 8 9    0 1 2 3 4 5 6 7 8 9
                // ---------------------  ---------------------  ---------------------
                // |a|b|c|d|e|f|g|h| | |  |a|b|c|d|e|f| |h| | |  |a|b|c|d|e|f|h| | | |
                // ---------------------  ---------------------  ---------------------
                // Elements move down starting from the hole to the tail, and then the tail
                // pointer is decremented once.

                // Example: Delete 'c' at array index 2 by moving elements up from the head
                //
                //  H   X           T      H   x           T        H X           T
                //  0 1 2 3 4 5 6 7 8 9    0 1 2 3 4 5 6 7 8 9    0 1 2 3 4 5 6 7 8 9
                // ---------------------  ---------------------  ---------------------
                // |a|b|c|d|e|f|g|h| | |  |a|b| |d|e|f|g|h| | |  | |a|b|d|e|f|g|h| | |
                // ---------------------  ---------------------  ---------------------
                // Elements move up starting from the hole to the head, and then the head
                // pointer is incremented once.

                int internalIndex = ( index + readPos ) % this.size;

                // ---- Tail remove ----
                int curr = internalIndex;
                int next;
                while( curr != this.writePos )
                {
                    next = ( curr + 1 ) % size;
                    this.buffer[curr] = this.buffer[next];
                    curr = next;
                }

                this.Count = ( this.Count - 1 + size ) % size;
                this.writePos = ( this.writePos - 1 + size ) % size;
            }
        }

        /// <summary>
        /// Removes the first element in the list.
        /// </summary>
        public void RemoveFirst()
        {
            if( this.Count == 0 )
            {
                throw new InvalidOperationException( "The list is empty" );
            }

            this.buffer[this.readPos] = default( T );
            this.readPos = ( this.readPos + 1 ) % this.size;
            this.Count--;
        }

        /// <summary>
        /// Removes the last element in the list.
        /// </summary>
        public void RemoveLast()
        {
            if( this.Count == 0 )
            {
                throw new InvalidOperationException( "The list is empty" );
            }

            int newTail = ( this.writePos - 1 + this.size ) % this.size;

            this.buffer[newTail] = default( T );
            this.writePos = newTail;
            this.Count--;
        }

        /// <summary>
        /// Removes the first element that is equvilent to the provided element.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove( T item )
        {
            int index = IndexOf( item );

            if( index < 0 )
            {
                return false;
            }
            else
            {
                RemoveAt( index );
                return true;
            }
        }


        /// <summary>
        /// Copies the entire list to the given array.
        /// </summary>
        /// <param name="array">The array to copy values to.</param>
        /// <param name="arrayIndex">The first index to write to in the destination array.</param>
        /// <param name="length">The number of elements to copy to the destination array.</param>
        public void CopyTo( T[] array, int arrayIndex )
        {
            CopyTo( array, arrayIndex, this.Count );
        }

        /// <summary>
        /// Copies the entire list to the given array.
        /// </summary>
        /// <param name="array">The array to copy values to.</param>
        /// <param name="arrayIndex">The first index to write to in the destination array.</param>
        /// <param name="length">The number of elements to copy to the destination array.</param>
        public void CopyTo( T[] array, int arrayIndex, int length )
        {
            if( readPos > writePos )
            {
                // Our array looks like:
                //        T       H
                //  0 1 2 3 4 5 6 7 8 9
                // ---------------------
                // |d|e|f| | | | |a|b|c|
                // ---------------------
                //    L-----|       |
                //          |       |
                //    T-----+-------v
                //    |     |
                //  |-v-| |-v-|
                //  0 1 2 3 4 5 6 7 8 9
                // ---------------------
                // |a|b|c|d|e|f| | | | |
                // ---------------------
                int chunkLen;

                // Copy the chunk of elements at the end of the buffer.
                // - 'a b c' == 3 elements ---> 10 - 7 = 3 ---> chunkLen = size - head.
                chunkLen = Math.Min( this.size - this.readPos, length );
                Array.Copy( this.buffer, this.readPos, array, arrayIndex, chunkLen );

                length -= chunkLen;
                arrayIndex += chunkLen;

                if( length > 0 )
                {
                    // Copy the chunk of elements at the beginning of the buffer.
                    chunkLen = Math.Min( writePos, length );
                    Array.Copy( this.buffer, 0, array, arrayIndex, chunkLen );
                }
            }
            else
            {
                // Our array looks like:
                //    H           T
                // ---------------------
                // | |a|b|c|d|e|f| | | |
                // ---------------------

                Array.Copy( this.buffer, this.readPos, array, arrayIndex, Math.Min( this.Count, length ) );
            }
        }

        /// <summary>
        /// Returns whether or not the list contains an element that is equivilent to the
        /// provided element.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains( T item )
        {
            return IndexOf( item ) >= 0;
        }

        /// <summary>
        /// Returns whether or not the array is read-only. Always returns false.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Enumerates the values stored in the list.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator<T>( this );
        }

        /// <summary>
        /// Enumerates the values stored in the list.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator<T>( this );
        }

        private void CheckCapacity()
        {
            if( this.Count == this.buffer.Length - 1 )
            {
                T[] newBuffer = new T[this.size * 2];

                CopyTo( newBuffer, 0, this.Count );
                this.readPos = 0;
                this.writePos = this.Count;

                Array.Clear( this.buffer, 0, this.buffer.Length );
                this.buffer = newBuffer;
                this.size = newBuffer.Length;
            }
        }

        private class Enumerator<TEnum> : IEnumerator<TEnum>, IDisposable, IEnumerator
        {
            private CircularList<TEnum> list;
            private int index;
            private TEnum current;

            public Enumerator( CircularList<TEnum> list )
            {
                this.list = list;
                this.index = 0;
                this.current = default( TEnum );
            }

            public TEnum Current { get { return current; } }

            public void Dispose()
            {
                this.list = null;
                this.current = default( TEnum );
            }

            public bool MoveNext()
            {
                CircularList<TEnum> list = this.list;

                if( this.index < list.Count )
                {
                    this.current = list[index];
                    this.index++;
                    return true;
                }
                else
                {
                    this.current = default( TEnum );
                    return false;
                }
            }

            void IEnumerator.Reset()
            {
                this.current = default( TEnum );
                this.index = 0;
            }

            object IEnumerator.Current { get { return this.current; } }
        }

    }
}
