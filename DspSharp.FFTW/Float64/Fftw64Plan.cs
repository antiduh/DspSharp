
namespace DspSharp.FFTW.Float64
{
    /// <summary>
    /// Stores an FFTW execution plan.
    /// </summary>
    public sealed unsafe class Fftw64Plan : IDisposable
    {
        private nint handle;

        /// <summary>
        /// Initializes a new instance of the <see cref="Fftw64Plan"/> class.
        /// </summary>
        /// <param name="handle">The handle returned from FFTW.</param>
        internal Fftw64Plan( nint handle )
        {
            this.handle = handle;
        }

        /// <summary>
        /// Performs the FFT on the arguments provided when the plan was constructed.
        /// </summary>
        public void Execute()
        {
            CheckDisposed();
            NativeMethods64.Execute( this.handle );
        }

        /// <summary>
        /// Releases the resources held by the <see cref="Fftw64Plan"/>.
        /// </summary>
        public void Dispose()
        {
            if( this.handle == 0 )
            {
                return;
            }

            lock( Fftw64ApiLock.Lock )
            {
                NativeMethods64.DestroyPlan( this.handle );
            }

            this.handle = 0;
        }

        private void CheckDisposed()
        {
            ObjectDisposedException.ThrowIf( this.handle == 0, this );
        }
    }
}