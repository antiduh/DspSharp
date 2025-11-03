using System;

namespace DspSharp.FFTW
{
    /// <summary>
    /// Specifies the FFT direction.
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// Performs the Forward FFT. FFTW uses the convention that forward transforms use a -1 exponent.
        /// </summary>
        Forward = -1,

        /// <summary>
        /// Performs the Reverse FFT. FFTW uses the convention that reverse transforms use a +1 exponent.
        /// </summary>
        Reverse = +1,
    }

    public enum Options : uint
    {
        /// <summary>
        /// Indicates that FFTW should build a plan quickly by estimating the most efficient execution plan rather than measuring it.
        /// </summary>
        Estimate = 64,

        /// <summary>
        /// Indicates that FFTW should build a plan by measuring the performance of the most common, likely execution plans.
        /// </summary>
        Measure = 0,
        
        /// <summary>
        /// Indicates that FFTW should build a plan by measuring the performance of more execution plans than <see cref="Measure"/>.
        /// </summary>
        Patient = 32,

        /// <summary>
        /// Indicates that FFTW should build a plan by measuring the performance of all possible execution plans. Requires considerable time.
        /// </summary>
        Exhaustive = 8,

    }
}
