using System.Collections.Generic;
using System.IO;


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
        AlarmClockExtender.DebugLog(string.Join("\n",DebugStrings.ToArray()));
        DebugStrings.Clear();
    }

    void DirSearch(string sDir)
    {
        DebugLog("Reading Directory: {0}", sDir);
        try
        {
            foreach (string d in Directory.GetDirectories(sDir))
            {
                foreach (string f in Directory.GetFiles(d))
                {
                    if (!IgnoredTracks.Contains(f) && !AudioFiles.Contains(f) && NAudioPlayer.IsSupportedFileType(f))
                    {
                        DebugLog("Found File: {0}", Path.GetFileName(f));
                        AudioFiles.Add(f);
                    }
                }
                DirSearch(d);
            }
        }
        catch (System.Exception excpt)
        {
            DebugLog("Failed to find files due to Exception: {0}", excpt.Message);
        }
    }
}
