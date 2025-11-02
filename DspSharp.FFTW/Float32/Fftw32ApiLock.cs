using System;

namespace DspSharp.FFTW.Float32
{
    /// <summary>
    /// Stores the lock that protects FFTW planning operations during multi-threaded access.
    /// </summary>
    internal static class Fftw32ApiLock
    {
        /// <summary>
        /// Gets the lock.
        /// </summary>
        public static readonly Lock Lock = new();
    }
}