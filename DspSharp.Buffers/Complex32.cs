using System.Runtime.InteropServices;

namespace DspSharp.Buffers
{
    /// <summary>
    /// Stores a Complex value composed of a 32-bit floating point real value and a 32-bit floating
    /// point imaginary value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
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

        public float Magnitude
        {
            get
            {
            }
        }
    }
}