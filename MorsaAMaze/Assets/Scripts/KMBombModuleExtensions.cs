using UnityEngine;

public static class KMBombModuleExtensions
{
    private static uint _consecutiveID = 0;

    public static void GenerateLogFriendlyName(this KMBombModule module)
    {
        _consecutiveID++;
        module.name = string.Format("{0} #{1}", module.ModuleDisplayName, _consecutiveID);
    }

    public static void GenerateLogFriendlyName(this KMNeedyModule module)
    {
        _consecutiveID++;
        module.name = string.Format("{0} #{1}", module.ModuleDisplayName, _consecutiveID);
    }

    public static int GetIDNumber(this KMBombModule module)
    {
        int id;
        string[] split = module.name.Split('#');
        if (split.Length < 2) return 0;
        return int.TryParse(split[1], out id) ? id : 0;
    }

    public static int GetIDNumber(this KMNeedyModule module)
    {
        int id;
        string[] split = module.name.Split('#');
        if (split.Length < 2) return 0;
        return int.TryParse(split[1], out id) ? id : 0;
    }

    public static void Log(this KMBombModule module, object message)
    {
        if (module.GetIDNumber() == 0)
            module.GenerateLogFriendlyName();
        Debug.LogFormat("[{0}] {1}", module.name, message);
    }

    public static void LogFormat(this KMBombModule module, string format, params object[] args)
    {
        if (module.GetIDNumber() == 0)
            module.GenerateLogFriendlyName();
        Debug.LogFormat("[{0}] {1}", module.name, string.Format(format, args));
    }

    public static void Log(this KMNeedyModule module, object message)
    {
        if (module.GetIDNumber() == 0)
            module.GenerateLogFriendlyName();
        Debug.LogFormat("[{0}] {1}", module.name, message);
    }

    public static void LogFormat(this KMNeedyModule module, string format, params object[] args)
    {
        if (module.GetIDNumber() == 0)
            module.GenerateLogFriendlyName();
        Debug.LogFormat("[{0}] {1}", module.name, string.Format(format, args));
    }
}
