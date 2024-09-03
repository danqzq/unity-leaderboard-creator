using System.Runtime.InteropServices;
using UnityEngine;

namespace Dan
{
    public static class Clipboard
    {
        [DllImport("__Internal")]
        private static extern void CopyToClipboard(string text);
     
        public static void SetText(string text)
        {          
#if UNITY_WEBGL && UNITY_EDITOR == false
            CopyToClipboard(text);
#else
            GUIUtility.systemCopyBuffer = text;
#endif
        }
    }
}