using System.Numerics;
using System.Windows;
using DspSharp.Buffers;
using DspSharp.FFTW;
using DspSharp.FFTW.Float64;

namespace DspSharp.DemoUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.WindowState = WindowState.Maximized;

            BuildPlot();
        }

        private void BuildPlot()
        {
            int length = 16384;
            Complex64Array input = new Complex64Array( length, 128 );
            Complex64Array output = new Complex64Array( length, 128 );

            var plan = Fftw64Builder.Create1( length, input, output, Direction.Forward, Options.Patient );

            input.Clear();

            Complex64Array freq1Data = new Complex64Array( length, 128 );
            Span<Complex> freq1DataSpan = freq1Data;
            NaiveFreqGenerator64 freq1Gen = new NaiveFreqGenerator64( 48000, 1000 );

            freq1Gen.Process( freq1Data );


            Complex64Array freq2Data = new Complex64Array( length, 128 );
            Span<Complex> freq2DataSpan = freq2Data;
            NaiveFreqGenerator64 freq2Gen = new NaiveFreqGenerator64( 48000, 2000 );

            freq2Gen.Process( freq2Data );

            Span<Complex> inputSpan = input.AsSpan();

            for( int i = 0; i < length; i++ )
            {
                inputSpan[i] = freq1DataSpan[i] + freq2DataSpan[i];
            }

            plan.Execute();


            Span<Complex> outputSpan = output.AsSpan();
            double[] mags = new double[length];

            for( int i = 0; i < length; i++ )
            {
                mags[i] = outputSpan[i].Magnitude / length;
            }

            this.Plot.Plot.Add.Signal( mags );
            this.Plot.Refresh();
        }
    }
}