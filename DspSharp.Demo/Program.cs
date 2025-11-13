using System.Numerics;
using DspSharp.Buffers;
using DspSharp.FFTW;
using DspSharp.FFTW.Float64;
using DspSharp.FreqGen;

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

            Complex64Array freq1Data = new Complex64Array( length, 128 );
            Span<Complex> freq1DataSpan = freq1Data;
            NaiveFreqGen64 freq1Gen = new NaiveFreqGen64( 48000, 1000 );

            freq1Gen.Process( freq1Data );


            Complex64Array freq2Data = new Complex64Array( length, 128 );
            Span<Complex> freq2DataSpan = freq2Data;
            NaiveFreqGen64 freq2Gen = new NaiveFreqGen64( 48000, 2000 );

            freq2Gen.Process( freq2Data );

            Span<Complex> inputSpan = input.AsSpan();

            for( int i = 0; i < length; i++ )
            {
                inputSpan[i] = freq1DataSpan[i] + freq2DataSpan[i];
            }

            plan.Execute();

            var outputSpan = output.AsSpan();
            for( int i = 0; i < length; i++ )
            {
                Console.WriteLine( outputSpan[i].Magnitude );
            }
        }
    }
}
