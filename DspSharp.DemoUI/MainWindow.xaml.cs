using System.Numerics;
using System.Windows;
using DspSharp.Buffers;
using DspSharp.FFTW;
using DspSharp.FFTW.Float64;
using DspSharp.FreqGen;
using DspSharp.NRZL;
using DspSharp.SineGenerator;
using ScottPlot.Plottables;

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


            //BuildPlotNrzlEncoder();
            BuildPlotNRZLDecoder();
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
            int numSamples = 512;
            int bitrate = 16000;
            int sampleRate = 48000;

            double genFreq = 0.48 * bitrate;
            double genOffset = 0.0;
            double genAmp = 1.0;

            double[] signal = new double[numSamples];

            SineGeneratorF64 gen = new( genAmp, genFreq, genOffset, sampleRate );

            gen.Process( signal );
            //ClipSamples( signal );

            DecoderDebugger debugger = new DecoderDebugger( numSamples + 1 );

            NrzlDecoder nrzl = new( bitrate, sampleRate )
            {
                Debug = debugger
            };

            bool[] bits = new bool[numSamples];
            int numBits = nrzl.Run( signal, bits );

            Signal sigPlot = this.Plot.Plot.Add.Signal( signal );
            sigPlot.LegendText = "Signal";
            SetPlotStyle( sigPlot );

            //Signal phasePlot = this.Plot.Plot.Add.Signal( debugger.PhaseSamples );
            //phasePlot.LegendText = "Phase";
            //SetPlotStyle( phasePlot );

            Signal bitsPlot = this.Plot.Plot.Add.Signal( debugger.Bits );
            bitsPlot.LegendText = "Bits";
            bitsPlot.Data.YScale = 2.0;
            SetPlotStyle( bitsPlot );

            //Signal intPlot = this.Plot.Plot.Add.Signal( debugger.IntegratorSamples );
            //intPlot.LegendText = "Integral";
            //intPlot.Data.YOffset = 3.0;
            //SetPlotStyle( intPlot );

            this.Plot.Refresh();
        }

        private static void SetPlotStyle( Signal sig )
        {
            sig.MaximumMarkerSize = 5;
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

        public class DecoderDebugger : INrzlDecoderDebug
        {
            private int currentSample;

            public DecoderDebugger( int numSamples )
            {
                this.currentSample = 0;

                this.Bits = new double[ numSamples ];
                this.IntegratorSamples = new double[numSamples];
                this.PhaseSamples = new double[numSamples];
            }

            public double[] Bits { get; }

            public double[] IntegratorSamples { get; }

            public double[] PhaseSamples { get; }

            public void Bit( bool bit )
            {
                this.Bits[currentSample] = bit ? +1 : -1;
            }

            public void Integrator( double value )
            {
                this.IntegratorSamples[currentSample] = value;
            }

            public void Phase( double value )
            {
                this.PhaseSamples[currentSample] = value;
            }

            public void EndSample()
            {
                this.currentSample++;
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