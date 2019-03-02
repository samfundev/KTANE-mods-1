using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

using NAudio.Wave;

public static class NAudioPlayer {
    public static string[] AudioExtensions = {
        ".wav",
        ".mp3",
        ".ogg",
        ".s3m",".xm",".mod",".it"
    };

    public static AudioClip GetAudioClip(string filePath)
    {
        if(filePath == null)
            throw new NotSupportedException("FilePath cannot be null");
        var ext = Path.GetExtension(filePath).ToLowerInvariant();

        switch (ext)
        {
            case ".mp3":
                return FromMp3Data(File.ReadAllBytes(filePath));
            case ".wav":
            case ".ogg":
            case ".s3m":
            case ".xm":
            case ".mod":
            case ".it":
                AudioClip clip = new WWW("file:///" + filePath).GetAudioClipCompressed();
                while (clip.loadState != AudioDataLoadState.Loaded)
                {
                    if (clip.loadState == AudioDataLoadState.Failed)
                        throw new Exception("Failed to load the audio clip.");
                }
                return clip;
            default:
                throw new NotSupportedException($"File type {ext} is not supported.");
        }
    }

    public static bool IsSupportedFileType(string filePath)
    {
        return filePath != null && AudioExtensions.Contains(Path.GetExtension(filePath).ToLowerInvariant());
    }

    private static AudioClip FromMp3Data(byte[] data)
    {
        // Load the data into a stream
        MemoryStream mp3Stream = new MemoryStream(data);
        // Convert the data in the stream to WAV format
        Mp3FileReader mp3Audio = new Mp3FileReader(mp3Stream);
        WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream(mp3Audio);
        // Convert to WAV data
        WAV wav = new WAV(AudioMemStream(waveStream).ToArray());
        AudioClip audioClip = AudioClip.Create("AudioClip", wav.SampleCount, 1, wav.Frequency, false);
        audioClip.SetData(wav.LeftChannel, 0);
        // Return the clip
        return audioClip;
    }

    private static MemoryStream AudioMemStream(WaveStream waveStream)
    {
        MemoryStream outputStream = new MemoryStream();
        using (WaveFileWriter waveFileWriter = new WaveFileWriter(outputStream, waveStream.WaveFormat)) 
        { 
            byte[] bytes = new byte[waveStream.Length]; 
            waveStream.Position = 0;
            waveStream.Read(bytes, 0, Convert.ToInt32(waveStream.Length)); 
            waveFileWriter.Write(bytes, 0, bytes.Length); 
            waveFileWriter.Flush(); 
        }
        return outputStream;
    }
}

/* From http://answers.unity3d.com/questions/737002/wav-byte-to-audioclip.html */
public class WAV  {

    // convert two bytes to one float in the range -1 to 1
    private static float BytesToFloat(byte firstByte, byte secondByte) {
        // convert two bytes to one short (little endian)
        var s = (short)((secondByte << 8) | firstByte);
        // convert to range from -1 to (just below) 1
        return s / 32768.0F;
    }

    private static int BytesToInt(IList<byte> bytes,int offset=0){
        var value=0;
        for(var i=0;i<4;i++){
            value |= bytes[offset+i]<<(i*8);
        }
        return value;
    }
    // properties
    public float[] LeftChannel{get; internal set;}
    public float[] RightChannel{get; internal set;}
    public int ChannelCount {get;internal set;}
    public int SampleCount {get;internal set;}
    public int Frequency {get;internal set;}

    public WAV(byte[] wav){

        // Determine if mono or stereo
        ChannelCount = wav[22];     // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels

        // Get the frequency
        Frequency = BytesToInt(wav,24);

        // Get past all the other sub chunks to get to the data subchunk:
        var pos = 12;   // First Subchunk ID from 12 to 16

        // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
        while(!(wav[pos]==100 && wav[pos+1]==97 && wav[pos+2]==116 && wav[pos+3]==97)) {
            pos += 4;
            var chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
            pos += 4 + chunkSize;
        }
        pos += 8;

        // Pos is now positioned to start of actual sound data.
        SampleCount = (wav.Length - pos)/2;     // 2 bytes per sample (16 bit sound mono)
        if (ChannelCount == 2) SampleCount /= 2;        // 4 bytes per sample (16 bit stereo)

        // Allocate memory (right will be null if only mono sound)
        LeftChannel = new float[SampleCount];
        RightChannel = ChannelCount == 2 ? new float[SampleCount] : null;

        // Write to double array/s:
        var i=0;
        while (pos < wav.Length) {
            LeftChannel[i] = BytesToFloat(wav[pos], wav[pos + 1]);
            pos += 2;
            if (ChannelCount == 2) {
                if(RightChannel != null)
                    RightChannel[i] = BytesToFloat(wav[pos], wav[pos + 1]);
                pos += 2;
            }
            i++;
        }
    }

    public override string ToString ()
    {
        return string.Format ("[WAV: LeftChannel={0}, RightChannel={1}, ChannelCount={2}, SampleCount={3}, Frequency={4}]", LeftChannel, RightChannel, ChannelCount, SampleCount, Frequency);
    }
}