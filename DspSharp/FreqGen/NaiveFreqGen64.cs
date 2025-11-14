using System.Numerics;

namespace DspSharp.FreqGen
{
    public class NaiveFreqGen64
    {
        private Complex phasor;

        private Complex angle;

        public NaiveFreqGen64( int sampleRate, int frequency )
        {
            this.angle = new Complex( 1.0, 0.0 );

            double angularStep = 2.0 * Math.PI * frequency / sampleRate;

            this.phasor = Complex.FromPolarCoordinates( 1.0, angularStep );
        }

        public void Process( Span<Complex> buffer )
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
