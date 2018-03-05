using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VanillaRuleModifierAssembly
{
    public class ManualFileName
    {
        public string Name;
        public bool IsText;
        public byte[] Bytes;
        public string Text;

        public ManualFileName(string name, byte[] bytes)
        {
            Name = name;
            IsText = false;
            Bytes = bytes;
        }

        public ManualFileName(string name, string text)
        {
            Name = name;
            IsText = true;
            Text = text;
        }

        public static void DebugLog(string message, params object[] args)
        {
            CommonReflectedTypeInfo.DebugLog(message, args);
        }

        public void WriteFile(string path, List<ReplaceText> textReplacements = null)
        {
            DebugLog("Writing Manaul file: {0}", Name);
            var manualpath = Path.Combine(path, Name);
            if (string.IsNullOrEmpty(manualpath))
            {
                DebugLog("Manual path is empty or null");
                return;
            }
            var dirname = Path.GetDirectoryName(manualpath);
            if (string.IsNullOrEmpty(dirname))
            {
                DebugLog("Directory name is empty or null");
                return;
            }

            if (string.IsNullOrEmpty(Text) && Bytes == null)
            {
                DebugLog("Can't write {0} as it is null.", Name);
                return;
            }

            Directory.CreateDirectory(dirname);
            try
            {
                if (textReplacements == null)
                    textReplacements = new List<ReplaceText>();
                if (IsText)
                {
                    File.WriteAllText(manualpath, textReplacements.Aggregate(Text, (current, replacement) => current.Replace(replacement.Original, replacement.Replacement)));
                }
                else
                    File.WriteAllBytes(manualpath, Bytes);
            }
            catch (Exception ex)
            {
                DebugLog("Failed to write file due to Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
            }
        }
    }
}
