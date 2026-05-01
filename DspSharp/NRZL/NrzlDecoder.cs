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

        private readonly Window dcRemover;
        private readonly Window bitIntegrator;

        private double phase;
        private double nextSamplePhase;

        private double remainingPhase;

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
            this.nextSamplePhase = 1.0; // Trigger sampling at the end of the bit after integrating the signal.
            this.remainingPhase = 1.0;
            this.prevSample = 0.0;

            this.dcRemover = new Window( 30 );
            this.bitIntegrator = new Window( 3 );
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

            for( int i = 0; i < samples.Length; i++ ) 
            {
                double currSample = samples[i];

                this.dcRemover.Add( currSample );
                currSample -= this.dcRemover.Avg();

                double lastBit = 0.0;

                // Advance the phase by the dynamically tracked frequency
                phase += measuredFreq;

                // Detect zero-crossing transition
                if( prevSample != 0.0 && currSample != 0.0 && Math.Sign( prevSample ) != Math.Sign( currSample ) )
                {
                    // Linearly interpolate to find the exact fraction between samples where crossing occurred
                    double fraction = Math.Abs( prevSample ) / ( Math.Abs( prevSample ) + Math.Abs( currSample ) );

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
                remainingPhase -= this.measuredFreq;

                if( remainingPhase < 0 )
                {
                    // Somewhere between prevSample and currSample is exactly where phase 1.0 is,
                    // aka, the exact end of the bit.
                    //
                    // We need to interpolate between prevSample and currSample to figure out how
                    // much to add to our integral of the bit's samples.
                    //
                    // Keep in mind that remainingPhase is negative. It's negative by the amount
                    // that currSample is past the end of the bit.
                    //
                    // `phase - remainingPhase` gives us the value phase would have been when the
                    // bit ended.




                    // In NRZ-L, > 0 is typically Logic 1, <= 0 is Logic 0
                    bool bit = this.bitIntegrator.Sum() > 0;

                    bits[numDecodedBits] = bit;

                    Debug.Bit( bit );

                    lastBit = bits[numDecodedBits] ? +1.0 : -1.0;
                    numDecodedBits++;

                    // Set threshold for the center of the next bit
                    remainingPhase = 1.0;
                }
                else
                {
                    this.bitIntegrator.Add( currSample );
                    this.Debug.Integrator( this.bitIntegrator.Sum() );
                }

                // Wrap phases to prevent floating-point precision loss over long periods
                //if( phase >= 1.0 && nextSamplePhase >= 1.5 )
                //{
                //    phase -= 1.0;
                //    nextSamplePhase -= 1.0;
                //}

                prevSample = currSample;

                Debug.EndSample();
            }

            return numDecodedBits;
        }

        private static double Interpolate( double prevSample, double currSample )
        {
            return Math.Abs( prevSample ) / ( Math.Abs( prevSample ) + Math.Abs( currSample ) );
        }
    }
}