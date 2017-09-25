using System;
using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using Object = UnityEngine.Object;

public class MouseUnlocker : MonoBehaviour
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ClipCursor(IntPtr rect);

    public KMService Service = null;

    void Start()
    {
        Debug.Log("[MouseUnlocker] Started");
        Update();
    }

    void Awake()
    {
        Debug.Log("[MouseUnlocker] is Awake.");
        Update();
    }

    private bool _firstRun;
    void Update()
    {
        if (!_firstRun)
            Debug.Log("[MouseUnlocker] Unlocking the cursor");
        ClipCursor(IntPtr.Zero);
        if (!_firstRun)
        {
            Debug.Log("[MouseUnlocker] Cursor unlocked");
            _firstRun = true;
        }
    }
}
