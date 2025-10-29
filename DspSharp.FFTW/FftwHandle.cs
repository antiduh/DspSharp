using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DspSharp.FFTW
{
    public class FftwHandle : SafeHandle
    {
        public override bool IsInvalid => throw new NotImplementedException();

        protected override bool ReleaseHandle()
        {
            throw new NotImplementedException();
        }
    } 
}
