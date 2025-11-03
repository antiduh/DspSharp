using System;
using DspSharp.Buffers;

namespace DspSharp.FFTW.Float32
{
    public static unsafe class Fftw32Builder
    {
        public static Fftw32Plan Create1( int n, Complex32Array input, Complex32Array output, Direction dir, Options opts )
        {
            lock( Fftw32ApiLock.Lock )
            {
                nint handle = NativeMethods32.plan_dft_1d( n, input.Handle, output.Handle, dir, opts );
                return new Fftw32Plan( handle );
            }
        }
    }
}