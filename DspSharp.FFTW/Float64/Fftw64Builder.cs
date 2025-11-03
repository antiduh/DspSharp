using System;
using DspSharp.Buffers;

namespace DspSharp.FFTW.Float64
{
    public static unsafe class Fftw64Builder
    {
        // Input and output might be filled with garbage
        public static Fftw64Plan Create1( int n, Complex64Array input, Complex64Array output, Direction dir, Options opts )
        {
            lock( Fftw64ApiLock.Lock )
            {
                nint handle = NativeMethods64.plan_dft_1d( n, input.Handle, output.Handle, dir, opts );
                return new Fftw64Plan( handle );
            }
        }
    }
}