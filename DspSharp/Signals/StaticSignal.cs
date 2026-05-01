using System;
using System.Numerics;

namespace DspSharp.Signals
{
    public class StaticSignal<T> : ISignal<T> where T : struct, INumber<T>
    {
        private readonly T value;

        public StaticSignal( T value )
        {
            this.value = value;
        }

        public void Process( Span<T> buffer )
        {
            for( int i = 0; i < buffer.Length; i++ )
            {
                buffer[i] = value;
            }
        }
    }
}