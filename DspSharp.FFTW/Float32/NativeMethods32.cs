using System;
using System.Runtime.InteropServices;

namespace DspSharp.FFTW.Float32
{
    /// <summary>
    /// Provides definitions for FFTW's float32 native methods.
    /// </summary>
    internal static unsafe class NativeMethods32
    {
        [DllImport( "fftw3f", EntryPoint = "fftwf_plan_dft_1d", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl )]
        public static extern nint plan_dft_1d(
            int n,
            void* input,
            void* output,
            Direction direction,
            Options flags
        );

        [DllImport( "fftwf", EntryPoint = "fftwf_execute", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl )]
        public static extern void Execute( nint handle );

        [DllImport( "fftwf", EntryPoint = "fftwf_destroy_plan", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl )]
        public static extern void DestroyPlan( nint handle );

        [DllImport( "fftwf", EntryPoint = "fftwf_import_wisdom_from_filename", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode )]
        public static extern int ImportWisdom( string filename );

        [DllImport( "fftwf", EntryPoint = "fftwf_export_wisdom_to_filename", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode )]
        public static extern int ExportWisdom( string filename );
    }
}