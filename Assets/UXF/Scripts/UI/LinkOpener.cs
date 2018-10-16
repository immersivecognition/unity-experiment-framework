using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF
{
    public class LinkOpener : MonoBehaviour
    {
		public void OpenURL(string url)
		{
			Application.OpenURL(url);
		}
    }
}