using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

namespace UXF.UI
{

    public class WebFileElement : MonoBehaviour
    {
        public Text filenameText;
        public Button copyBtn;
        public Button downloadBtn;

# if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void Download(string text, string filename);

        [DllImport("__Internal")]
        private static extern void CopyToClipboard(string text);
# endif
        public void Setup(string text, string filename)
        {
            filenameText.text = filename;
# if !UNITY_EDITOR && UNITY_WEBGL
            copyBtn.onClick.AddListener(() => CopyToClipboard(text));
            downloadBtn.onClick.AddListener(() => Download(text, filename));
# else
            copyBtn.onClick.AddListener(() => GUIUtility.systemCopyBuffer = text );
            downloadBtn.interactable = false;
# endif
        }

    }
}
