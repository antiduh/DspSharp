using System.Numerics;
using DspSharp.Buffers;
using DspSharp.FFTW;
using DspSharp.FFTW.Float64;

namespace DspSharp.Demo
{
    internal class Program
    {
        static void Main( string[] args )
        {
            int length = 512;
            Complex64Array input = new Complex64Array( length, 128 );
            Complex64Array output = new Complex64Array( length, 128 );

            var plan = Fftw64Builder.Create1( length, input, output, Direction.Forward, Options.Patient );

            input.Clear();

        }
    }
}
