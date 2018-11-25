using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Threading;

namespace LeBoyLib
{
    /// <summary>
    /// Emulates a Z80 Gameboy CPU, more specifically a Sharp LR35902 which is a Z80 minus a few instructions, with more logical operations and a sound generator.
    /// </summary>

    public class SineWaveProvider32 : WaveProvider32
    {
        int sample;

        public SineWaveProvider32()
        {
            Frequency = 1000;
            Amplitude = 0.25f; // let's not hurt our ears            
        }

        public float Frequency { get; set; }
        public float Amplitude { get; set; }

        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            int sampleRate = WaveFormat.SampleRate;
            for (int n = 0; n < sampleCount; n++)
            {
                buffer[n + offset] = (float)(Amplitude * Math.Sin((2 * Math.PI * sample * Frequency) / sampleRate));
                sample++;
                if (sample >= sampleRate) sample = 0;
            }
            return sampleCount;
        }
    }

    public abstract class WaveProvider32 : IWaveProvider
    {
        private WaveFormat waveFormat;

        public WaveProvider32()
            : this(44100, 1)
        {
        }

        public WaveProvider32(int sampleRate, int channels)
        {
            SetWaveFormat(sampleRate, channels);
        }

        public void SetWaveFormat(int sampleRate, int channels)
        {
            this.waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            WaveBuffer waveBuffer = new WaveBuffer(buffer);
            int samplesRequired = count / 4;
            int samplesRead = Read(waveBuffer.FloatBuffer, offset / 4, samplesRequired);
            return samplesRead * 4;
        }

        public abstract int Read(float[] buffer, int offset, int sampleCount);

        public WaveFormat WaveFormat
        {
            get { return waveFormat; }
        }
    }

    public partial class GBZ80
    {
        // The GB has 4 stereo channels
        // Ch1: Quadrangular wave patterns with sweep and envelope functions.
        // Ch2: Quadrangular wave patterns with envelope functions.
        // Ch3: Voluntary wave patterns from wave RAM (4 bits per sample).
        // Ch4: White noise with an envelope function.

        private void Sound_Step()
        {


            // Ch2
            /*
            byte NR21 = Memory[0xFF16];
            float duty = (((NR21 & 0xC0) >> 6) + 1) / 8.0f;
            int t1 = NR21 & 0x3F;

            byte NR22 = Memory[0xFF17];
            float initialVolume = ((NR22 & 0xF0) >> 4) / 15.0f;
            bool increasing = (NR22 & 0x8) != 0;
            int sweeps = NR22 & 0x7;
            */

            // Ch4
            byte NR41 = Memory[0xFF20];
            byte NR42 = Memory[0xFF21];
            byte NR43 = Memory[0xFF22];
            byte NR44 = Memory[0xFF23];

            if ((NR44 & 0x80) != 0)
            {
                Console.WriteLine("boop");
                Memory[0xFF23] = (byte)(NR44 & ~0x80);

                //Test
                var sineWaveProvider = new SineWaveProvider32();
                sineWaveProvider.SetWaveFormat(131072, 4); // 131072Hz mono
                sineWaveProvider.Frequency = 1000;
                sineWaveProvider.Amplitude = 0.25f;
                WaveOut wo = new WaveOut();
                wo.Init(sineWaveProvider);
                wo.Play();
            }
        }

        private double PulseWave(double time, double frequency, double duty, double amplitude)
        {
            double period = 1.0 / frequency;
            double timeModulusPeriod = time - Math.Floor(time / period) * period;
            double phase = timeModulusPeriod / period;
            if (phase <= duty)
                return amplitude;
            else
                return -amplitude;
        }
    }
}
