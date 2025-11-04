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
            Span<Complex> inputSpan = input;

            Complex64Array output = new Complex64Array( length, 128 );
            Span<Complex> outputSpan = output;

            var plan = Fftw64Builder.Create1( length, input, output, Direction.Forward, Options.Patient );

            input.Clear();
            FreqGenerator64 freq = new FreqGenerator64( 48000, -1000 );
            freq.Process( input.AsSpan() );

            plan.Execute();


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