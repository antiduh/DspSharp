using System;
using System.Runtime.InteropServices;

namespace DspSharp.FFTW.Float64
{
    /// <summary>
    /// Provides definitions for FFTW's float64 native methods.
    /// </summary>
    internal static unsafe class NativeMethods64
    {
        [DllImport( "fftw3", EntryPoint = "fftw_plan_dft_1d", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl )]
        public static extern nint plan_dft_1d(
            int n,
            void* input,
            void* output,
            Direction direction,
            Options flags
        );

        [DllImport( "fftw3", EntryPoint = "fftw_execute", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl )]
        public static extern void Execute( nint handle );

        [DllImport( "fftw3", EntryPoint = "fftw_destroy_plan", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl )]
        public static extern void DestroyPlan( nint handle );

        [DllImport( "fftw3", EntryPoint = "fftw_import_wisdom_from_filename", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode )]
        public static extern int ImportWisdom( string filename );

        [DllImport( "fftw3", EntryPoint = "fftw_export_wisdom_to_filename", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode )]
        public static extern int ExportWisdom( string filename );
    }
}