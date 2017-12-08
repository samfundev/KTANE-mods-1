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

        var locked = true;

        if (CommonReflectedTypeInfo.PlayerSettingsManagerInstanceField != null && CommonReflectedTypeInfo.PlayerSettingLockMouseField != null)
        {
            var psm = CommonReflectedTypeInfo.PlayerSettingsManagerInstanceField.GetValue(null);
            if (psm != null)
            {
                var ps = CommonReflectedTypeInfo.PlayerSettingsManagerPlayerSettingsField.GetValue(psm);
                if (ps != null)
                {
                    locked = (bool) CommonReflectedTypeInfo.PlayerSettingLockMouseField.GetValue(ps);
                    if (locked)
                    {
                        Debug.Log("[MouseUnlocker] Settings the player settings field so that mouse remains unlocked AT ALL TIMES");

                        CommonReflectedTypeInfo.PlayerSettingLockMouseField.SetValue(ps, false);
                        Debug.Log("[MouseUnlocker] Settings changed");

                        CommonReflectedTypeInfo.PlayerSettingManagerSaveSettingsMethod.Invoke(psm, null);
                        Debug.Log("[MouseUnlocker] Settings saved");
                    }
                }
            }
        }

        if (locked)
        {
            ClipCursor(IntPtr.Zero);
        }


        if (!_firstRun)
        {
            Debug.Log("[MouseUnlocker] Cursor unlocked");
            _firstRun = true;
        }
    }
}
