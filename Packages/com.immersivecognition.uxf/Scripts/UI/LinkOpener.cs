using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF.UI
{
    public class LinkOpener : MonoBehaviour
    {
		public void OpenURL(string url)
		{
			Application.OpenURL(url);
		}
    }
}