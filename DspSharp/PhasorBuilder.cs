using System;
using System.Numerics;
using DspSharp.Buffers;

namespace DspSharp
{
    internal class PhasorBuilder
    {
        private readonly int memAlignment;
        private readonly int sampleRate;
        private readonly int blockSize;
        private readonly int maxSamples;
        private readonly double epsilon;

        public PhasorBuilder( int sampleRate, int blockSize, int maxSamples, double epsilon, int memAlignment )
        {
            this.memAlignment = memAlignment;
            this.sampleRate = sampleRate;
            this.blockSize = blockSize;
            this.maxSamples = maxSamples;
            this.epsilon = epsilon;
        }

        public Complex64Array Build64( int freq )
        {
            int strideLength = 2 * FindStrideLength( freq );

            return BuildBuffer64( freq, strideLength );
        }

        public Complex32Array Build32( int freq )
        {
            int strideLength = 2 * FindStrideLength( freq );

            return BuildBuffer32( freq, strideLength );
        }

        private int FindStrideLength( int newFreq )
        {
            double phaseVelocity = Math.Tau * newFreq * 1.0 / sampleRate;

            Complex angle = new Complex( 1, 0 );
            Complex phasor = Complex.FromPolarCoordinates( 1.0, phaseVelocity );

            // Step 1 - We want our minimum stride length to be at least one buffer length, so run
            // the angle math until we're past one buffer.

            int i = 0;

            for( ; i < this.blockSize; i++ )
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
                    // If we did `i` multiplications, we make `i+1` samples.
                    return i + 1;
                }
            }

            throw new Exception( $"{nameof( PhasorBuilder )} failed - unable to find suitable stride length." );

        }

        private Complex64Array BuildBuffer64( int newFreq, int strideLength )
        {
            Complex64Array buffer = new( strideLength, this.memAlignment );
            Span<Complex> bufferSpan = buffer;

            double phaseVelocity = Math.Tau * newFreq * 1.0 / sampleRate;

            Complex angle = new Complex( 1, 0 );
            Complex phasor = Complex.FromPolarCoordinates( 1.0, phaseVelocity );

            for( int i = 0; i < strideLength; i++ )
            {
                bufferSpan[i] = angle;

                angle *= phasor;

                if( i % this.blockSize == 0 )
                {
                    Reunity( ref angle );
                }
            }

            return buffer;
        }


        private Complex32Array BuildBuffer32( int newFreq, int strideLength )
        {
            Complex32Array buffer = new( strideLength, this.memAlignment );
            Span<Complex32> bufferSpan = buffer;

            double phaseVelocity = Math.Tau * newFreq * 1.0 / sampleRate;

            Complex angle = new Complex( 1, 0 );
            Complex phasor = Complex.FromPolarCoordinates( 1.0, phaseVelocity );

            for( int i = 0; i < strideLength; i++ )
            {
                bufferSpan[i] = new Complex32( (float)angle.Real, (float)angle.Imaginary );

                angle *= phasor;

                if( i % this.blockSize == 0 )
                {
                    Reunity( ref angle );
                }
            }

            return buffer;
        }

        private static void Reunity( ref Complex value )
        {
            value = value / value.Magnitude;
        }
    }
}