namespace DspSharp.Buffers
{
    public struct Complex32
    {
        public float Real;

        public float Imaginary;

        public Complex32( float real, float imaginary )
        {
            this.Real = real;
            this.Imaginary = imaginary;
        }
    }
}