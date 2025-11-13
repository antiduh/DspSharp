using System.Numerics;
using DspSharp.Buffers;

namespace DspSharp.FreqGen
{
    public class NaiveFreqGenerator32
    {
        private Complex32 phasor;

        private Complex32 angle;

        public NaiveFreqGenerator32( int sampleRate, int frequency )
        {
            this.angle = new Complex32( 1.0f, 0.0f );

            float angularStep = (float)(2.0f * Math.PI * frequency / sampleRate);

            this.phasor = Complex32.FromPolarCoordinates( 1.0f, angularStep );
        }

        public void Process( Span<Complex32> buffer )
        {
            for( int i = 0; i < buffer.Length; i++ )
            {
                buffer[i] = this.angle;
                this.angle *= this.phasor;
            }

            // Recalibrate the value to the unit circle since the multiplication above will make it
            // drift over time.
            this.angle = this.angle / this.angle.Magnitude;
        }
    }
}
