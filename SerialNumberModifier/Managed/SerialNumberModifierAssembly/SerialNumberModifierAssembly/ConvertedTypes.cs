using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SerialNumberModifierAssembly
{
    public static class CommonReflectedTypeInfo
    {
        static CommonReflectedTypeInfo()
        {
        }

        public static void DebugLog(string message, params object[] args)
        {
            var debugstring = $"[Serial Number Modifier] {message}";
            Debug.LogFormat(debugstring, args);
        }


        public static SerialNumberWidget SerialNumberWidget;
        public static bool InitializeSerialNumberWidget(SerialNumberModWidget serialnumberWidget)
        {
            if (serialnumberWidget == null)
            {
                DebugLog("Can't replace serial number, because the main widget transform is missing");
                if (SerialNumberWidget != null) Object.Destroy(SerialNumberWidget);
                return false;
            }

            if (serialnumberWidget.Tags == null)
            {
                DebugLog("Can't replace serial number, because the tags list is null");
                if (SerialNumberWidget != null) Object.Destroy(SerialNumberWidget);
                return false;
            }

            for (var i = 0; i < serialnumberWidget.Tags.Count; i++)
            {
                var t = serialnumberWidget.Tags[i];
                if (t.SerialNumber == null)
                {
                    serialnumberWidget.Tags.Remove(t);
                    i--;
                    continue;
                }

                t.gameObject.SetActive(false);
                t.Screws = t.Screws == null 
                    ? new Transform[0] 
                    : t.Screws.Where(x => x != null).Select(x => x).ToArray();
            }

            if (serialnumberWidget.Tags.Count > 0)
            {
                SerialNumberWidget = serialnumberWidget.GetComponent<SerialNumberWidget>();
                if (SerialNumberWidget != null)
                    return true;

                SerialNumberWidget = serialnumberWidget.gameObject.AddComponent<SerialNumberWidget>();
                SerialNumberWidget.SizeX = 2;
                SerialNumberWidget.SizeZ = 1;
                SerialNumberWidget.Tags = serialnumberWidget.Tags;
                return true;
            }

            DebugLog("Can't replace serial number, because there are no tags defined. (tags.Count = {0})", serialnumberWidget.Tags.Count);
            if (SerialNumberWidget != null) Object.Destroy(SerialNumberWidget);
            return false;
        }

        public static IEnumerator AddWidgetToBomb()
        {
            if (SerialNumberWidget == null)
            {
                DebugLog("Serial number widget not initialized.");
                yield break;
            }

            DebugLog("Replacing Serial number widget");

            var generator = Object.FindObjectOfType<WidgetGenerator>();
            while (generator == null)
            {
                yield return null;
                generator = Object.FindObjectOfType<WidgetGenerator>();
            }

            DebugLog("Replacing main serial number widget with custom widget");
            if (!generator.RequiredWidgets.Contains(SerialNumberWidget))
            {
                var osnw = generator.RequiredWidgets.FirstOrDefault(x => x.GetType() == typeof(SerialNumber)) as SerialNumber;
                generator.RequiredWidgets.Remove(osnw);

                try
                {
                    DebugLog("Capturing original serial number widget");
                    var newsnw = Object.Instantiate(osnw, Vector3.zero, Quaternion.identity, SerialNumberWidget.transform);
                    var osnwt = newsnw.transform;
                    osnwt.name = "Vanilla Serial Number";
                    var tag = osnwt.gameObject.AddComponent<SerialNumberTag>();
                    tag.SerialNumber = newsnw.SerialTextMesh;
                    Object.DestroyImmediate(newsnw, false);
                    SerialNumberWidget.Tags.Add(tag);
                    SerialNumberWidget.OriginalSerialNumberWidgetCaptured = osnwt;
                    DebugLog("Original serial number widget captured");
                }
                catch (Exception ex)
                {
                    DebugLog("Could not capture the original serial number widget Due to the following exception: {0}:{1}\n Stack Trace: {2}", ex.GetType().Name, ex.Message, ex.StackTrace);
                }

                generator.RequiredWidgets.Add(SerialNumberWidget);
            }
        }

        public static void RemoveCapturedSerialTag()
        {
            if (SerialNumberWidget.OriginalSerialNumberWidgetCaptured == null) return;

            Object.Destroy(SerialNumberWidget.OriginalSerialNumberWidgetCaptured);
            SerialNumberWidget.OriginalSerialNumberWidgetCaptured = null;
            SerialNumberWidget.Tags.Remove(SerialNumberWidget.Tags.Last());
        }


    }
}
