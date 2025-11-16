using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using DspSharp.Buffers;
using DspSharp.Simd;

namespace DspSharp.FreqShift
{
    public class FastFreqShift64
    {
        private Complex64Array? currBuffer;

        private int position;

        private Complex64Array? newBuffer;

        private PhasorBuilder builder;

        public FastFreqShift64( int sampleRate, int blockSize )
            : this( sampleRate, blockSize, blockSize * 10, 0.000_000_000_1, 128 )
        {
        }

        public FastFreqShift64( int sampleRate, int blockSize, int maxSamples, double epsilon, int memAlignment )
        {
            this.builder = new PhasorBuilder( sampleRate, blockSize, maxSamples, epsilon, memAlignment );
        }

        public void PrepareNewSettings( int freq )
        {
            this.newBuffer = this.builder.Build64( freq );
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
            Complex64Simd.ComplexMultiply( output, output, source );

            outputRemaining -= source.Length;

            if( outputRemaining > 0 )
            {
                int outputPos = source.Length;

                source = GetChunk( outputRemaining );

                output = output.Slice( outputPos );

                Complex64Simd.ComplexMultiply( output, output, source );
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
    }
}
