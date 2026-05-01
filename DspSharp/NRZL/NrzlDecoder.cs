using System;
using DspSharp.Statistics;

namespace DspSharp.NRZL
{
    public class NrzlDecoder
    {
        /// <summary>
        /// Specifies the highest acceptable bitrate frequency offset as a ratio.
        /// </summary>
        private const double freqOffsetHigh = 1.10;

        /// <summary>
        /// Specifies the lowest acceptable bitrate frequency offset as a ratio.
        /// </summary>
        private const double freqOffsetLow = 0.90;

        private readonly double nominalFreq;
        private readonly double alpha;
        private readonly double beta;

        private readonly SampleIntegrator integrator;

        private double phase;
        private double nextSamplePhase;
        private double measuredFreq;
        private double prevSample;

        /// <summary>
        /// Initializes a new instance of the NRZ-L Decoder with Clock Recovery.
        /// </summary>
        /// <param name="bitrate">Target bitrate.</param>
        /// <param name="sampleRate">Audio sample rate.</param>
        /// <param name="alpha">Proportional gain for phase adjustment.</param>
        /// <param name="beta">Integral gain for frequency adjustment.</param>
        public NrzlDecoder( int bitrate, int sampleRate, double alpha = 0.05, double beta = 0.005 )
        {
            this.nominalFreq = (double)bitrate / sampleRate;
            this.measuredFreq = nominalFreq;
            this.alpha = alpha;
            this.beta = beta;

            this.phase = 0.0;
            this.nextSamplePhase = 0.5; // Trigger sampling at the end of the bit after integrating the signal.
            this.prevSample = 0.0;

            this.integrator = new SampleIntegrator( 20 );
        }

        public INrzlDecoderDebug Debug { get; set; }

        /// <summary>
        /// Calculates the minimum buffer size to store returned bits
        /// </summary>
        /// <param name="bitrate"></param>
        /// <param name="sampleRate"></param>
        /// <param name="sampleSize"></param>
        /// <returns></returns>
        public static int GetBitBufferSize( int bitrate, int sampleRate, int sampleSize )
        {
            double nominalFreq = (double)bitrate / sampleRate;

            // The most number of bits we'll decode is the nominal bitrate times the maximum
            // allowable bitrate freq offset. 
            double numBits = ( nominalFreq / sampleRate ) * freqOffsetHigh;

            // Add a few extra bits for room, just to make sure we never overflow.
            numBits += 30;

            return (int)Math.Ceiling( numBits );
        }

        /// <summary>
        /// Decodes NRZ-L audio samples into an array of boolean bits. Can be called continuously
        /// with sequential chunks of audio data.
        /// </summary>
        /// <param name="samples">Array of audio samples as doubles.</param>
        /// <param name="bits">
        /// Stores decoded bits. Ensure size is at least <see cref="GetBitBufferSize(int, int, int)"/>.
        /// </param>
        /// <returns>The number of decoded bits</returns>
        public int Run( double[] samples, bool[] bits )
        {
            int numDecodedBits = 0;

            double bitIntegral = 0;

            for( int i = 0; i < samples.Length; i++ ) 
            {
                double sample = samples[i];

                this.integrator.Add( sample );
                sample -= this.integrator.Avg();

                double lastBit = 0.0;

                // Advance the phase by the dynamically tracked frequency
                phase += measuredFreq;

                bitIntegral += sample;

                Debug.Integrator( bitIntegral );

                // Detect zero-crossing transition
                if( prevSample != 0.0 && sample != 0.0 && Math.Sign( prevSample ) != Math.Sign( sample ) )
                {
                    // Linearly interpolate to find the exact fraction between samples where crossing occurred
                    double fraction = Math.Abs( prevSample ) / ( Math.Abs( prevSample ) + Math.Abs( sample ) );

                    // Calculate what our phase was at that exact zero-crossing moment
                    double zcPhase = ( phase - measuredFreq ) + fraction * measuredFreq;

                    // Transitions should theoretically occur exactly at integer boundaries (0.0, 1.0, etc.)
                    // The error is the distance from the actual crossing phase to the nearest integer.
                    double error = Math.Round( zcPhase ) - zcPhase;

                    // Adjust phase (Proportional) and frequency (Integral)
                    phase += alpha * error;
                    measuredFreq += beta * error;

                    // Clamp frequency to +/- 10% of nominal to prevent the PLL from drifting into noise
                    // during long periods of silence or absence of transitions.
                    measuredFreq = Math.Clamp( measuredFreq, nominalFreq * freqOffsetLow, nominalFreq * freqOffsetHigh );
                }

                Debug.Phase( this.phase - nextSamplePhase );

                // If the adjusted phase crosses the sampling threshold, read the bit
                if( phase >= nextSamplePhase )
                {
                    // In NRZ-L, > 0 is typically Logic 1, <= 0 is Logic 0
                    bool bit = sample > 0;

                    bits[numDecodedBits] = bit;
                    bitIntegral = 0;

                    Debug.Bit( bit );

                    lastBit = bits[numDecodedBits] ? +1.0 : -1.0;
                    numDecodedBits++;

                    // Set threshold for the center of the next bit
                    nextSamplePhase += 1.0;

                }

                // Wrap phases to prevent floating-point precision loss over long periods
                //if( phase >= 1.0 && nextSamplePhase >= 1.5 )
                //{
                //    phase -= 1.0;
                //    nextSamplePhase -= 1.0;
                //}

                prevSample = sample;

                Debug.EndSample();
            }

            return numDecodedBits;
        }
    }
}