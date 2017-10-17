using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class MakeAudioClip : ThreadedJob
{
    public string FilePath;
    public AudioClip clip;

    protected override void ThreadFunction()
    {
        clip = NAudioPlayer.GetAudioClip(FilePath);
    }

    protected override void OnFinished()
    {
        
    }
}
