using System.Numerics;
using System.Runtime.InteropServices;

namespace DspSharp.Buffers
{
    /// <summary>
    /// Stores a Complex value composed of a 32-bit floating point real value and a 32-bit floating
    /// point imaginary value.
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public struct Complex32
    {
        public float Real;

        public float Imaginary;

        public Complex32( float real, float imaginary )
        {
            this.Real = real;
            this.Imaginary = imaginary;
        }

        public static Complex32 FromPolarCoordinates( float magnitude, float angle )
        {
            (float sin, float cos) = float.SinCos( angle );

            return new Complex32(
                cos * magnitude,
                sin * magnitude
            );
        }

        public readonly float Magnitude => float.Hypot( this.Real, this.Imaginary );

        public readonly float Phase => float.Atan2( this.Imaginary, this.Real );

        public static Complex32 operator*( Complex32 l, Complex32 r )
        {
            // L = a + bi.
            // R = c + di
            // Z = L * R = (a + bi) * ( c + di )
            // Z = (ac - bd) + (ad + bc)i

            float real = l.Real * r.Real - r.Imaginary * l.Imaginary;
            float imag = l.Real * r.Imaginary + l.Imaginary * r.Real;

            return new Complex32( real, imag );
        }

        public static Complex32 operator /( Complex32 l, float r )
        {
            if( r == 0 )
            {
                return new Complex32( float.NaN, float.NaN );
            }

            if( float.IsFinite( l.Real ) == false )
            {
                if( !float.IsFinite( l.Imaginary ) )
                {
                    return new Complex32( float.NaN, float.NaN );
                }

                return new Complex32( l.Real / r, float.NaN );
            }

            if( float.IsFinite( l.Imaginary ) == false )
            {
                return new Complex32( float.NaN, l.Imaginary / r );
            }

            return new Complex32( l.Real / r, l.Imaginary / r );
        }
    }
}