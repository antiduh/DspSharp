using System;

namespace DspSharp.SineGenerator
{
    public class SineGeneratorF64
    {
        private readonly double amplitude;
        private readonly double offset;

        private readonly double angleStep;

        private double angle;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frequency">The frequency to generate</param>
        /// <param name="sampleRate">The number of samples per second.</param>
        public SineGeneratorF64( double amplitude, double frequency, double offset, int sampleRate )
        {
            this.amplitude = amplitude;
            this.offset = offset;
            this.angleStep = Math.Tau * frequency / sampleRate;
        }

        public void Process( Span<double> buffer )
        {
            for( int i = 0; i < buffer.Length; i++ )
            {
                buffer[i] = amplitude * Math.Sin( this.angle ) + offset;

                this.angle += this.angleStep;
            }

            // Renormalize angle to [-Tau, +Tau] so it doesn't go off to infinity and lose precision.
            this.angle = this.angle % Math.Tau;
        }
    }
}
