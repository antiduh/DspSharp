using System;

namespace DspSharp.FFTW
{
    public enum Direction
    {
        Forward = -1,
        Reverse = +1,
    }

    public enum Options : uint
    {
        Estimate = 64,
        Measure = 0,
        Patient = 32,
        Exhaustive = 8,

    }
}
