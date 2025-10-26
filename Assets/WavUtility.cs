using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] wavFile, string name = "wav")
    {
        if (wavFile == null || wavFile.Length < 44)
            return null;

        int channels = BitConverter.ToInt16(wavFile, 22);
        int sampleRate = BitConverter.ToInt32(wavFile, 24);
        int bitsPerSample = BitConverter.ToInt16(wavFile, 34);
        int dataStartIndex = 44;

        int bytesPerSample = bitsPerSample / 8;
        int sampleCount = (wavFile.Length - dataStartIndex) / bytesPerSample;
        float[] data = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            int offset = dataStartIndex + i * bytesPerSample;
            short sample = BitConverter.ToInt16(wavFile, offset);
            data[i] = sample / 32768f;
        }

        AudioClip audioClip = AudioClip.Create(name, sampleCount / channels, channels, sampleRate, false);
        audioClip.SetData(data, 0);
        return audioClip;
    }
}
