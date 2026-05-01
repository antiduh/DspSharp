using System;
using System.Linq;
using DspSharp.Collections;

namespace DspSharp.Statistics
{
    public class SampleIntegrator
    {
        private readonly int windowSize;

        private readonly CircularList<double> samples;

        public SampleIntegrator( int windowSize )
        {
            this.windowSize = windowSize;
            this.samples = new CircularList<double>( windowSize );
        }

        public void Add( double sample )
        {
            this.samples.Add( sample );

            if( this.samples.Count > this.windowSize )
            {
                this.samples.RemoveFirst();
            }
        }

        public double Sum()
        {
            return this.samples.Sum();
        }

        public double Avg()
        {
            return this.samples.Average();
        }

        public bool IsFull()
        {
            return this.samples.Count == this.windowSize;
        }
    }
}
