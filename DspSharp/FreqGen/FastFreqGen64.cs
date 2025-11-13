using System;
using System.Numerics;
using DspSharp.Buffers;

namespace DspSharp.FreqGen
{
    public class FastFreqGen64
    {
        private readonly int sampleRate;
        private readonly int blockSize;
        private readonly int maxSamples;
        private readonly double epsilon;
        private readonly int memAlignment;

        private Complex64Array? currBuffer;

        private int position;

        private Complex64Array? newBuffer;

        public FastFreqGen64( int sampleRate, int blockSize )
            : this( sampleRate, blockSize, blockSize*10, 0.000_000_001, 128 )
        { }

        public FastFreqGen64( int sampleRate, int blockSize, int maxSamples, double epsilon, int memAlignment )
        {
            this.sampleRate = sampleRate;
            this.blockSize = blockSize;
            this.maxSamples = maxSamples;
            this.epsilon = epsilon;
            this.memAlignment = memAlignment;
        }

        public void PrepareNewSettings( int freq )
        {
            int strideLength = 2 * FindStrideLength( freq );

            this.newBuffer = BuildBuffer( freq, strideLength );
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
            this.position = 0;

            // Clean up.
            this.newBuffer = null;
            oldBuffer?.Dispose();
        }

        public void SetFrequency( int freq )
        {
            PrepareNewSettings( freq );
            ApplyNewSettings();
        }

        public void Process( Span<Complex> output )
        {
            Span<Complex> source;

            int outputRemaining = output.Length;

            source = GetChunk( outputRemaining );
            source.CopyTo( output );

            outputRemaining -= source.Length;

            if( outputRemaining > 0 )
            {
                int outputPos = source.Length;
            
                source = GetChunk( outputRemaining );
                source.CopyTo( output.Slice( outputPos ) );
            }
        }

        private Span<Complex> GetChunk( int maxLength )
        {
            Span<Complex> source = this.currBuffer.AsSpan();

            // Figure out how much we can provide.
            int available = source.Length - this.position;
            int chunkSize = Math.Min( maxLength, available );

            // Slice out the next source chunk.
            Span<Complex> chunk = source.Slice( this.position, chunkSize );

            // Update accounting.
            this.position += chunkSize;

            if( this.position > source.Length - 1 )
            {
                this.position = 0;
            }

            return chunk;
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

            throw new Exception( $"{nameof(FastFreqGen64)} failed - unable to find suitable stride length." );

        }

        private Complex64Array BuildBuffer( int newFreq, int strideLength )
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
                    Reunity(ref angle );
                }
            }

            return buffer;
        }

        private static void Reunity( ref Complex value )
        {
            value = value / Complex.Abs( value );
        }
    }
}
