using System.Numerics;
using System.Runtime.Intrinsics.X86;
using DspSharp.FreqGen;
using DspSharp.Simd;

namespace DspSharp.Tests
{
    [TestClass]
    public sealed class Complex64SimdTests
    {
        // A buffer size that is not an even multiple of 128 or 256. Checks that SIMD
        // implementations handle incomplete blocks.
        private const int bufferSize = 1024 + 100;

        private const double epsilon = 0.000_000_000_1;

        [TestMethod]
        public void ComplexMultiply128_Identity()
        {
            RequireAvx128();

            Span<Complex> signal = GetSine( 1000 );
            Span<Complex> identity = GetIdentity();
            Span<Complex> result = new Complex[bufferSize];

            Complex64Simd.ComplexMultiply_128( result, signal, identity );

            for( int i = 0; i < signal.Length; i++ )
            {
                Complex diff = signal[i] - result[i];
                Assert.IsLessThan( epsilon, diff.Magnitude );
            }
        }

        [TestMethod]
        public void ComplexMultiply128_Shift()
        {
            RequireAvx128();

            Span<Complex> signal = GetSine( 1000 );
            Span<Complex> shift = GetSine( 1000 );
            Span<Complex> expected = GetSine( 2000 );
            Span<Complex> result = new Complex[bufferSize];

            Complex64Simd.ComplexMultiply_128( result, signal, shift );

            for( int i = 0; i < signal.Length; i++ )
            {
                Complex diff = result[i] - expected[i];
                Assert.IsLessThan( epsilon, diff.Magnitude );
            }
        }

        [TestMethod]
        public void ComplexAdd128()
        {
            Span<Complex> signal = GetSine( 1000 );
            Span<Complex> signal2 = GetSine( 1000 );
            Span<Complex> expected = new Complex[bufferSize];
            Span<Complex> result = new Complex[bufferSize];

            for( int i = 0; i < signal.Length; i++ )
            {
                expected[i] = signal[i] + signal2[i];
            }

            Complex64Simd.Add_128( result, signal, signal2 );

            for( int i = 0; i < signal.Length; i++ )
            {
                Complex diff = result[i] - expected[i];
                Assert.IsLessThan( epsilon, diff.Magnitude );
            }
        }

        [TestMethod]
        public void ComplexMultiply256_Identity()
        {
            RequireAvx256();

            Span<Complex> signal = GetSine( 1000 );
            Span<Complex> identity = GetIdentity();
            Span<Complex> result = new Complex[bufferSize];

            Complex64Simd.ComplexMultiply_256( result, signal, identity );

            for( int i = 0; i < signal.Length; i++ )
            {
                Complex diff = signal[i] - result[i];
                Assert.IsLessThan( epsilon, diff.Magnitude );
            }
        }

        [TestMethod]
        public void ComplexMultiply256_Shift()
        {
            RequireAvx256();

            Span<Complex> signal = GetSine( 1000 );
            Span<Complex> shift = GetSine( 1000 );
            Span<Complex> expected = GetSine( 2000 );
            Span<Complex> result = new Complex[bufferSize];

            Complex64Simd.ComplexMultiply_256( result, signal, shift );

            for( int i = 0; i < signal.Length; i++ )
            {
                Complex diff = result[i] - expected[i];
                Assert.IsLessThan( epsilon, diff.Magnitude );
            }
        }

        [TestMethod]
        public void ComplexAdd256()
        {
            Span<Complex> signal = GetSine( 1000 );
            Span<Complex> signal2 = GetSine( 1000 );
            Span<Complex> expected = new Complex[bufferSize];
            Span<Complex> result = new Complex[bufferSize];

            for( int i = 0; i < signal.Length; i++ )
            {
                expected[i] = signal[i] + signal2[i];
            }

            Complex64Simd.Add_256( result, signal, signal2 );

            for( int i = 0; i < signal.Length; i++ )
            {
                Complex diff = result[i] - expected[i];
                Assert.IsLessThan( epsilon, diff.Magnitude );
            }
        }


        private static void RequireAvx128()
        {
            if( Avx.IsSupported == false )
            {
                Assert.Inconclusive( "AVX 128 not supported on this CPU." );
            }
        }

        private static void RequireAvx256()
        {
            if( Avx2.IsSupported == false )
            {
                Assert.Inconclusive( "AVX 128 not supported on this CPU." );
            }
        }

        private static Complex[] GetSine( int freq )
        {
            Complex[] result = new Complex[bufferSize];

            NaiveFreqGen64 gen = new( 50000, freq );
            gen.Process( result );

            return result;
        }

        private static Complex[] GetIdentity()
        {
            Complex[] result = new Complex[bufferSize];

            for( int i = 0; i < result.Length; i++ )
            {
                result[i] = new Complex( 1.0, 0.0 );
            }

            return result;
        }
    }
}
