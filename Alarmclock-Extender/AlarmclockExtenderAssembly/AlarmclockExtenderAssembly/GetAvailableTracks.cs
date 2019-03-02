using System.Collections.Generic;
using System.IO;
using AlarmClockExtenderAssembly;


public class GetAvailableTracks : ThreadedJob
{
    public string FilePath;
    public List<string> AudioFiles = new List<string>();
    public List<string> IgnoredTracks = new List<string>();
    public List<string> DebugStrings = new List<string>();

    private void DebugLog(string message, params object[] args)
    {
        DebugStrings.Add(string.Format(message, args));
    }

    protected override void ThreadFunction()
    {
        if (!Directory.Exists(FilePath))
        {
            Directory.CreateDirectory(FilePath);
            return;
        }
        DirSearch(FilePath);
    }

    protected override void OnFinished()
    {
        AlarmClockHandler.DebugLog(string.Join("\n",DebugStrings.ToArray()));
        DebugStrings.Clear();
    }

    private void DirSearch(string sDir)
    {
        DebugLog("Reading Directory: {0}", sDir);
        try
        {
            foreach (var d in Directory.GetDirectories(sDir))
            {
                DirSearch(d);
            }

            foreach (var f in Directory.GetFiles(sDir))
            {
                if (IgnoredTracks.Contains(f) || AudioFiles.Contains(f) || !NAudioPlayer.IsSupportedFileType(f))
                    continue;

                DebugLog("Found File: {0}", Path.GetFileName(f));
                AudioFiles.Add(f);
            }
        }
        catch (System.Exception ex)
        {
            DebugLog("Failed to find files due to Exception: {0}", ex.Message);
        }
    }
}
