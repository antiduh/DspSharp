using System;

namespace DspSharp.FFTW.Float64
{
    /// <summary>
    /// Stores the lock that protects FFTW planning operations during multi-threaded access.
    /// </summary>
    internal static class Fftw64ApiLock
    {
        /// <summary>
        /// Gets the lock.
        /// </summary>
        public static readonly Lock Lock = new();
    }
}