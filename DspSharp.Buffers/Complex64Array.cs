using System.Numerics;
using System.Runtime.InteropServices;

namespace DspSharp.Buffers
{
    /// <summary>
    /// Stores an array of <see cref="Complex"/> values allocated in native memory, aligned to the
    /// requested memory alignment value.
    /// </summary>
    /// <remarks>
    /// The array is allocated in native memory instead of managed memory because it reduces GC
    /// execution time.
    /// </remarks>
    public sealed unsafe class Complex64Array : IDisposable
    {
        private void* handle;

        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of <see cref="Complex64Array"/>.
        /// </summary>
        /// <param name="numElements">The number of <see cref="Complex"/> values to store</param>
        /// <param name="alignment">The memory alignment, in bytes.</param>
        public Complex64Array( int numElements, int alignment )
        {
            nuint bytes = (nuint)(numElements * sizeof( Complex ));

            this.Count = numElements;
            this.handle = NativeMemory.AlignedAlloc( bytes, (nuint)alignment );
        }

        /// <summary>
        /// Gets the number of elements in the array.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets a handle to the array's memory.
        /// </summary>
        public void* Handle
        {
            get
            {
                CheckDisposed();
                return this.handle;
            }
        }

        /// <summary>
        /// Returns a view of the array as a <see cref="Span{T}"/>.
        /// </summary>
        /// <returns></returns>
        public Span<Complex> AsSpan()
        {
            CheckDisposed();
            return new Span<Complex>( this.handle, this.Count );
        }

        /// <summary>
        /// Resets every value in the array to 0.
        /// </summary>
        public void Clear()
        {
            CheckDisposed();
            AsSpan().Clear();
        }

        /// <summary>
        /// Releases the resources held by the array.
        /// </summary>
        public void Dispose()
        {
            if ( this.isDisposed )
            {
                return;
            }

            NativeMemory.Free( this.handle );
            this.handle = null;
            this.isDisposed = true;
        }

        private void CheckDisposed()
        {
            ObjectDisposedException.ThrowIf( this.isDisposed, this );
        }
    }
}