// Derived from https://stackoverflow.com/questions/53677757/simd-vectors-complex-numbers

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace DspSharp.Simd
{
    public static class Complex64Simd
    {
        public static void ComplexMultiply( Span<Complex> result, ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right )
        {
            if( Avx2.IsSupported )
            {
                ComplexMultiply_256( result, left, right );
            }
            
            else if( Sse.IsSupported )
            {
                ComplexMultiply_128( result, left, right );
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static void ComplexMultiply_128( Span<Complex> result, ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right )
        {
            Span<Vector128<double>> vectorRes = MemoryMarshal.Cast<Complex, Vector128<double>>( result );
            ReadOnlySpan<Vector128<double>> vectorLeft = MemoryMarshal.Cast<Complex, Vector128<double>>( left );
            ReadOnlySpan<Vector128<double>> vectorRight = MemoryMarshal.Cast<Complex, Vector128<double>>( right );

            for( int i = 0; i < vectorRes.Length; i++ )
            {
                Vector128<double> l = vectorLeft[i];
                Vector128<double> r = vectorRight[i];

                vectorRes[i] = Sse3.HorizontalAdd(
                    Sse2.Multiply(
                        Sse2.Multiply( l, r ),
                        Vector128.Create( 1.0, -1.0 )
                    ),

                    Sse2.Multiply(
                        l,
                        Avx.Permute( r, 0b01 )
                    )
                );
            }
        }

        public static void ComplexMultiply_256( Span<Complex> result, ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right )
        {
            Span<Vector256<double>> vectorRes = MemoryMarshal.Cast<Complex, Vector256<double>>( result );
            ReadOnlySpan<Vector256<double>> vectorLeft = MemoryMarshal.Cast<Complex, Vector256<double>>( left );
            ReadOnlySpan<Vector256<double>> vectorRight = MemoryMarshal.Cast<Complex, Vector256<double>>( right );
            
            for( int i = 0; i < vectorRes.Length; i++ )
            {
                Vector256<double> l = vectorLeft[i];
                Vector256<double> r = vectorRight[i];

                vectorRes[i] = Avx.HorizontalAdd(
                    Avx.Multiply(
                        Avx.Multiply( l, r ),
                        Vector256.Create( 1.0, -1.0, 1.0, -1.0 ) 
                    ),

                    Avx.Multiply(
                        l,
                        Avx.Permute( r, 0b0101 )
                    ) 
                );
            }

            for( int i = 2 * vectorRes.Length; i < result.Length; i++ )
            {
                result[i] = left[i] * right[i];
            }
        }

        public static void ComplexMultiply( Span<Complex> result, ReadOnlySpan<Complex> left, Complex single )
        {
            Span<Vector256<double>> vectorRes = MemoryMarshal.Cast<Complex, Vector256<double>>( result );
            ReadOnlySpan<Vector256<double>> vectorLeft = MemoryMarshal.Cast<Complex, Vector256<double>>( left );

            Vector256<double> scalerVect = Vector256.Create( single.Real, single.Imaginary, single.Real, single.Imaginary );

            for( int i = 0; i < vectorRes.Length; i++ )
            {
                Vector256<double> l = vectorLeft[i];


                vectorRes[i] = Avx.HorizontalAdd(
                    Avx.Multiply(
                        Avx.Multiply( l, scalerVect ),
                        Vector256.Create( 1.0, -1.0, 1.0, -1.0 )
                    ),

                    Avx.Multiply(
                        l,
                        Avx.Permute( scalerVect, 0b0101 )
                    )
                );
            }

            for( int i = 2 * vectorRes.Length; i < result.Length; i++ )
            {
                result[i] = left[i] * single;
            }
        }

        public static void Add( Span<Complex> result, ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right )
        {
            if( Avx.IsSupported )
            {
                Add_256( result, left, right );
            }
            else if( Sse2.IsSupported )
            {
                Add_128( result, left, right );
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static void Add_128( Span<Complex> result, ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right )
        {
            Span<Vector128<double>> vectorRes = MemoryMarshal.Cast<Complex, Vector128<double>>( result );
            ReadOnlySpan<Vector128<double>> vectorLeft = MemoryMarshal.Cast<Complex, Vector128<double>>( left );
            ReadOnlySpan<Vector128<double>> vectorRight = MemoryMarshal.Cast<Complex, Vector128<double>>( right );

            for( int i = 0; i < vectorRes.Length; i++ )
            {
                vectorRes[i] = Sse2.Add( vectorLeft[i], vectorRight[i] );
            }

            for( int i = 2 * vectorRes.Length; i < result.Length; i++ )
            {
                result[i] = left[i] + right[i];
            }
        }

        public static void Add_256( Span<Complex> result, ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right )
        {
            Span<Vector256<double>> vectorRes = MemoryMarshal.Cast<Complex, Vector256<double>>( result );
            ReadOnlySpan<Vector256<double>> vectorLeft = MemoryMarshal.Cast<Complex, Vector256<double>>( left );
            ReadOnlySpan<Vector256<double>> vectorRight = MemoryMarshal.Cast<Complex, Vector256<double>>( right );

            for( int i = 0; i < vectorRes.Length; i++ )
            {
                vectorRes[i] = Avx.Add( vectorLeft[i], vectorRight[i] );
            }

            for( int i = 2 * vectorRes.Length; i < result.Length; i++ )
            {
                result[i] = left[i] + right[i];
            }
        }
    }
}
