using System;
using System.Numerics;
using DspSharp.Buffers;

namespace DspSharp
{
    public class FastFreqGenerator64
    {
        private readonly int sampleRate;
        private readonly int bufferSize;
        private readonly int maxSamples;
        private readonly double epsilon;

        private Complex64Array? currBuffer;

        private Complex64Array? newBuffer;

        public FastFreqGenerator64( int sampleRate, int bufferSize, int maxSamples, double epsilon )
        {
            this.sampleRate = sampleRate;
            this.bufferSize = bufferSize;
            this.maxSamples = maxSamples;
            this.epsilon = epsilon;
        }

        public void PrepareNewSettings( double freq )
        {
            int strideLength = FindStrideLength( freq );

            newBuffer = BuildBuffer( strideLength );
        }

        public void ApplyNewSettings()
        {
            if( this.newBuffer == null )
            {
                throw new InvalidOperationException();
            }

            // Prepare
            Complex64Array? oldBuffer = this.currBuffer;

            // Swap
            this.currBuffer = newBuffer;

            // Clean up.
            this.newBuffer = null;
            oldBuffer?.Dispose();
        }

        public void SetFrequency( double freq )
        {
            PrepareNewSettings( freq );
            ApplyNewSettings();
        }

        public void Process( Span<Complex> buffer )
        {
        }

        private int FindStrideLength( double newFreq )
        {
            double phaseVelocity = Math.Tau * newFreq * 1.0 / sampleRate;

            Complex angle = new Complex( 1, 0 );
            Complex phasor = Complex.FromPolarCoordinates( 1.0, phaseVelocity );

            // Step 1 - We want our minimum stride length to be at least one buffer length, so run
            // the angle math until we're past one buffer.

            int i = 0;

            for( ; i < this.bufferSize; i++ )
            {
                angle *= phasor;
            }

            Reunity( ref angle );

            // Step 2 - Run the angle math until we detect that we're exceedingly close to zero.

            for( ; i < maxSamples; i++ )
            {
                angle *= phasor;

                if( double.Abs( angle.Phase ) < epsilon )
                {
                    // If we did i multiplications, we make i+1 samples.
                    return i + 1;
                }
            }

            throw new Exception( $"{nameof(FastFreqGenerator64)} failed - unable to find suitable stride length." );

        }

        private Complex64Array BuildBuffer( int strideLength )
        {
            throw new NotImplementedException();
        }

        private void Reunity( ref Complex value )
        {
            value = value / Complex.Abs( value );
        }
    }
}
