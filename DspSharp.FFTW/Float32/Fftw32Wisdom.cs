namespace DspSharp.FFTW.Float32
{
    /// <summary>
    /// Saves and loads FFTW's Wisdom.
    /// </summary>
    /// <remarks>
    /// FFTW's Wisdom is a cache of FFT execution plans for a given set of parameters. The first
    /// time a plan is constructed, FFTW will spend a considerable amount of time determining the
    /// fastest execution plan to use. Save and load Wisdom between program executions to save
    /// execution time on subsequent executions.
    /// </remarks>
    public sealed class Fftw32Wisdom : IDisposable
    {
        private string? filename;

        /// <summary>
        /// Loads Wisdom from the given filename.
        /// </summary>
        /// <param name="filename">The file to load from.</param>
        public Fftw32Wisdom( string filename )
        {
            this.filename = filename;
            Load( filename );
        }

        /// <summary>
        /// Saves Wisdom.
        /// </summary>
        public void Dispose()
        {
            if( this.filename == null )
            {
                return;
            }

            Save( filename );
            this.filename = null;
        }

        /// <summary>
        /// Saves FFTW Wisdom to the given file.
        /// </summary>
        /// <param name="filename"></param>
        public static void Save( string filename )
        {
            lock( Fftw32ApiLock.Lock )
            {
                NativeMethods32.ExportWisdom( filename );
            }
        }

        /// <summary>
        /// Loads FFTW wisdom from the given file.
        /// </summary>
        /// <param name="filename"></param>
        public static void Load( string filename )
        {
            lock( Fftw32ApiLock.Lock )
            {
                NativeMethods32.ImportWisdom( filename );
            }
        }
    }
}