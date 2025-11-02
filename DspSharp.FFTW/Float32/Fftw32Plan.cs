using DspSharp.Buffers;

namespace DspSharp.FFTW.Float32
{
    /// <summary>
    /// Stores an FFTW execution plan.
    /// </summary>
    public sealed unsafe class Fftw32Plan : IDisposable
    {
        private nint handle;

        /// <summary>
        /// Initializes a new instance of the <see cref="Fftw32Plan"/> class.
        /// </summary>
        /// <param name="handle">The handle returned from FFTW.</param>
        internal Fftw32Plan( nint handle )
        {
            this.handle = handle;
        }

        /// <summary>
        /// Performs the FFT on the arguments provided when the plan was constructed.
        /// </summary>
        public void Execute()
        {
            CheckDisposed();
            NativeMethods32.Execute( this.handle );
        }

        /// <summary>
        /// Releases the resources held by the <see cref="Fftw32Plan"/>.
        /// </summary>
        public void Dispose()
        {
            if( this.handle == 0 )
            {
                return;
            }

            lock( Fftw32ApiLock.Lock )
            {
                NativeMethods32.DestroyPlan( this.handle );
            }

            this.handle = 0;
        }

        private void CheckDisposed()
        {
            ObjectDisposedException.ThrowIf( this.handle == 0, this );
        }
    }
}