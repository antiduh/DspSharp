using System;
using System.Collections.Generic;
using System.Text;

namespace DspSharp.NRZL
{
    public interface INrzlDecoderDebug
    {
        void EndSample();

        void Bit( bool bit );

        void Integrator( double value );

        void Phase( double value );
        void Freq( double value );
    }
}
