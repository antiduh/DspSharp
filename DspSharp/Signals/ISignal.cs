using System;
using System.Numerics;

namespace DspSharp.Signals
{
    public interface ISignal<T> where T : struct, INumber<T>
    {
        void Process( Span<T> buffer );

        T Get();
    }
}
