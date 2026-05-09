using System;
using DspSharp.Statistics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DspSharp.NRZL
{
    public class NrzlDecoder
    {
        /// <summary>
        /// Specifies the highest acceptable bitrate frequency offset as a ratio.
        /// </summary>
        private const double freqOffsetHigh = 1.20;

        /// <summary>
        /// Specifies the lowest acceptable bitrate frequency offset as a ratio.
        /// </summary>
        private const double freqOffsetLow = 0.80;

        private readonly double nominalFreq;
        private readonly double alpha;
        private readonly double beta;

        private readonly Window dcRemover;

        private double phase;
        private double measuredFreq;

        private double prevSample;
        private double nextSamplePhase;
        private double bitIntegral;



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
            this.prevSample = 0.0;

            this.dcRemover = new Window( 30 );
            this.bitIntegral = 0.0;
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
            double error = 0.0;

            for( int i = 0; i < samples.Length; i++ ) 
            {
                double currSample = samples[i];

                this.dcRemover.Add( currSample );
                currSample -= this.dcRemover.Avg();

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
                    /*double*/ error = Math.Round( zcPhase ) - zcPhase;

                    // Adjust phase (Proportional) and frequency (Integral)
                    phase += alpha * error;
                    measuredFreq += beta * error;

                    // Clamp frequency to +/- 10% of nominal to prevent the PLL from drifting into noise
                    // during long periods of silence or absence of transitions.
                    measuredFreq = Math.Clamp( measuredFreq, nominalFreq * freqOffsetLow, nominalFreq * freqOffsetHigh );
                }
                this.Debug.Phase( error );

                this.Debug.Freq( this.measuredFreq - this.nominalFreq );

                // remaining symbol time measured in phases (0.0 to 1.0)
                double rst = this.nextSamplePhase - this.phase;

                if( rst < 0 )
                {
                    // Somewhere between prevSample and currSample is the end of the current bit and
                    // the start of the next bit. We need to split the integral in half.
                    // See diagrams in notes doc.
                    
                    // fraction: Ratio indicating how close in time the end of the bit was to currSample.
                    // * 0.1 would mean very close to prevSample.
                    // * 0.5 would mean halfway between prevSample and currSample.
                    // * 0.9 would mean very close to currSample. Keep in mind rst is negative.
                    //
                    // Example. Lets say:
                    // * mf is 0.333 (one sample is 1/3 of a bit) and
                    // * rst is -0.050 (bit ended 50 units prior to the current phase).
                    //
                    // Then mf + (-.050) gives us 0.283.  0.283 / 0.333 == 0.849. 
                    // The end of the bit was 84.9% through the time between prevSample and currSample.
                    double fraction = ( measuredFreq + rst ) / measuredFreq;

                    // bitEnd: The X axis value when the bit ended.
                    // - Note that when processing alternating bits, this value is nearly zero if the
                    //   data is well behaved and the DPLL is working well.
                    double bitEnd = prevSample * ( 1 - fraction ) + currSample * fraction;

                    // Compute the contribution of the end of the bit to the bit's integral.
                    this.bitIntegral += fraction * ( prevSample + bitEnd ) / 2.0;

                    // Declare the bit.
                    bool bit = this.bitIntegral > 0.0;
                    bits[numDecodedBits] = bit;
                    numDecodedBits++;
                    Debug.Bit( bit );

                    // Reset stats and start working on the next bit.
                    bitIntegral = 0;
                    nextSamplePhase += 1.0;

                    // Figure out how much of the prevSample-currSample interval contributes to the
                    // *new* bit's value.
                    bitIntegral += ( 1 - fraction ) * ( bitEnd + currSample ) / 2.0;
                }
                else
                {
                    // Both prevSample and currSample are part of the current bit. Just calculate
                    // their contribution to the bit's integral.

                    bitIntegral += ( this.prevSample + currSample ) / 2.0;
                }

                Debug.Integrator( bitIntegral );

                // Wrap phases to prevent floating-point precision loss over long periods
                if( phase >= 1.0 && nextSamplePhase >= 1.5 )
                {
                    phase -= 1.0;
                    nextSamplePhase -= 1.0;
                }

                prevSample = currSample;

                Debug.EndSample();
            }

            return numDecodedBits;
        }
    }
}