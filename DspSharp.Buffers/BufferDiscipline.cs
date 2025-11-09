using System;

namespace DspSharp.Buffers
{
    public readonly struct BufferDiscipline
    {
        public BufferDiscipline( int bufferSize, int memAlignment )
        {
            this.BufferSize = bufferSize;
            this.MemAlignment = memAlignment;
        }

        public int BufferSize { get; }
        public int MemAlignment { get; }
    }
}
