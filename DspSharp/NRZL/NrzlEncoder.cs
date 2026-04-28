using System;

namespace DspSharp.NRZL
{
    /// <summary>
    /// Encodes bits to NRZL samples.
    /// </summary>
    public class NrzlEncoder
    {
        private readonly double nominalFreq;

        private double phase;

        /// <summary>
        /// Initializes a new instance of <see cref="NrzlEncoder"/>.
        /// </summary>
        /// <param name="bitrate">Target bitrate.</param>
        /// <param name="sampleRate">Audio sample rate.</param>
        public NrzlEncoder( int bitrate, int sampleRate )
        {
            if( bitrate > sampleRate )
            {
                throw new ArgumentException( "Bitrate may not be faster than sample rate." );
            }

            this.nominalFreq = (double)bitrate / sampleRate;
            this.phase = 0.0;
        }

        /// <summary>
        /// Processses the provided bits into samples.
        /// </summary>
        /// <param name="bits">Bits to encode.</param>
        /// <param name="samples">Buffer to write samples to.</param>
        public void Run( bool[] bits, double[] samples )
        {
            int currBit = 0;

            for( int si = 0; si < samples.Length; si++ )
            {
                if( bits.Length > currBit )
                {
                    samples[si] = bits[currBit] ? +1.0 : -1.0;

                    this.phase += this.nominalFreq;

                    if( this.phase > 1.0 )
                    {
                        currBit++;
                        this.phase -= 1.0;
                    }
                }
                else
                {
                    samples[si] = 0.0;
                }
            }
        }
    }
}
