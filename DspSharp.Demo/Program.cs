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
            int length = 8192;
            Complex64Array input = new Complex64Array( length, 128 );
            Complex64Array output = new Complex64Array( length, 128 );

            var plan = Fftw64Builder.Create1( length, input, output, Direction.Forward, Options.Patient );

            input.Clear();

            FreqGenerator64 freq = new FreqGenerator64( 48000, 16000 );

            freq.Process( input.AsSpan() );

            plan.Execute();

            var outputSpan = output.AsSpan();
            for( int i = 0; i < length; i++ )
            {
                Console.WriteLine( outputSpan[i].Magnitude );
            }
        }
    }
}
