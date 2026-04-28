using System.Numerics;
using System.Windows;
using DspSharp.Buffers;
using DspSharp.FFTW;
using DspSharp.FFTW.Float64;
using DspSharp.FreqGen;
using DspSharp.NRZL;
using DspSharp.SineGenerator;

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

            BuildPlotNrzlEncoder();
        }

        private void BuildPlotNrzlEncoder()
        {
            int numSamples = 128;
            int bitrate = 16000;
            int sampleRate = 48000;

            int numBits = (numSamples * bitrate) / sampleRate;

            double[] signal = new double[numSamples];
            bool[] bits = new bool[numBits];

            FillBits( bits );

            NrzlEncoder nrzl = new( bitrate, sampleRate );

            nrzl.Run( bits, signal );

            this.Plot.Plot.Add.Signal( signal );
            this.Plot.Refresh();
        }

        private void BuildPlotNRZLDecoder()
        {
            int numSamples = 1024;
            int bitrate = 16000;
            int sampleRate = 48000;

            double[] signal = new double[numSamples];

            double[] debugSamples = new double[numSamples + 10];

            SineGeneratorF64 gen = new( 1.0, 0.48*bitrate, sampleRate );

            gen.Process( signal );
            ClipSamples( signal );

            NrzlDecoder nrzl = new( bitrate, sampleRate )
            {
                DebugSamples = debugSamples
            };

            bool[] bits = new bool[numSamples];
            int numBits = nrzl.Run( signal, bits );

            this.Plot.Plot.Add.Signal( signal );
            this.Plot.Plot.Add.Signal( debugSamples );

            this.Plot.Refresh();
        }

        private static void FillBits( bool[] bits )
        {
            int state = 0;
            
            for( int i = 0; i < bits.Length; i++ )
            {
                switch( state )
                {
                    case 0:
                        bits[i] = false;
                        state++;
                        break;
                    case 1:
                        bits[i] = false;
                        state++;
                        break;
                    case 2:
                        bits[i] = true;
                        state = 0;
                        break;
                }
            }
        }

        private static void ClipSamples( double[] samples )
        {
            for( int i = 0; i < samples.Length; i++ )
            {
                samples[i] = Math.Clamp( samples[i] * 1000.0, -1.0, +1.0 );
            }
        }

        private void BuildPlotFFT()
        {
            int length = 16384;
            Complex64Array input = new Complex64Array( length, 128 );
            Complex64Array output = new Complex64Array( length, 128 );

            var plan = Fftw64Builder.Create1( length, input, output, Direction.Forward, Options.Measure );

            input.Clear();

            Complex64Array freq1Data = new Complex64Array( length, 128 );
            Span<Complex> freq1DataSpan = freq1Data;
            //NaiveFreqGenerator64 freq1Gen = new( 48000, 1000 );

            FastFreqGen64 freq1Gen = new( 48000, 16384, 500_000_000, 0.000_000_000_01, 128 );
            freq1Gen.SetFrequency( 4801 );

            freq1Gen.Process( freq1Data );

            Complex64Array freq2Data = new Complex64Array( length, 128 );
            Span<Complex> freq2DataSpan = freq2Data;
            NaiveFreqGen64 freq2Gen = new( 48000, 2000 );

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