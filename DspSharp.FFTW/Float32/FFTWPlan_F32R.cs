using DspSharp.Buffers;

namespace DspSharp.FFTW.Float32
{
    public class FFTWPlan_F32R : IDisposable
    {
        private nint handle;

        public static FFTWPlan_F32R Create_1D( int n, Span<Complex32> input, Span<Complex32> output, Direction dir, Options opts )
        {

        }

        public void Execute()
        {
        }

        public void Dispose()
        {

        }
    }
}
