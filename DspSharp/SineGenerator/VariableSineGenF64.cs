using System;
using DspSharp.Signals;

namespace DspSharp.SineGenerator
{
    public class VariableSineGenF64
    {
        private readonly ISignal<double> amplitude;
        private readonly ISignal<double> frequency;
        private readonly ISignal<double> offset;
        private readonly int sampleRate;

        private double angle;

        public VariableSineGenF64( ISignal<double> amplitude, ISignal<double> frequency, ISignal<double> offset, int sampleRate )
        {
            this.amplitude = amplitude;
            this.frequency = frequency;
            this.offset = offset;
            this.sampleRate = sampleRate;
            this.angle = 0.0;
        }

        public void Process( Span<double> buffer )
        {
            for( int i = 0; i < buffer.Length; i++ )
            {
                buffer[i] = this.amplitude.Get() * Math.Sin( this.angle ) + offset.Get();

                double angleStep = Math.Tau * frequency.Get() / this.sampleRate;
                this.angle += angleStep;
            }

            // Renormalize angle to [-Tau, +Tau] so it doesn't go off to infinity and lose precision.
            this.angle = this.angle % Math.Tau;
        }
    }
}
