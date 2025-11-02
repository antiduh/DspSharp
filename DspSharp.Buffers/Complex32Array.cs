using System;
using System.Runtime.InteropServices;

namespace DspSharp.Buffers
{
    /// <summary>
    /// Stores an array of <see cref="Complex32"/> values allocated in native memory, aligned to the
    /// requested memory alignment value.
    /// </summary>
    /// <remarks>
    /// The array is allocated in native memory instead of managed memory because it reduces GC
    /// execution time.
    /// </remarks>
    public sealed unsafe class Complex32Array : IDisposable
    {
        private bool isDisposed;
        private void* handle;

        public Complex32Array( int numElements, int alignment )
        {
            nuint bytes = (nuint)(numElements * sizeof( Complex32 ));
            
            this.Count = numElements;
            this.handle = NativeMemory.AlignedAlloc( bytes, (nuint)alignment );
        }
       
        public int Count { get; }

        public void* Handle
        {
            get 
            {
                CheckDisposed();
                return this.handle;
            }

        }
        public Span<Complex32> AsSpan()
        {
            return new Span<Complex32>( this.handle, this.Count );
        }

        public void Clear()
        {
            AsSpan().Clear();
        }

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
