using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF
{
	/// <summary>
	/// A script attached to a NoteElement
	/// </summary>
	class NoteElementController : MonoBehaviour 
	{
		public void DestroySelf()
		{
			Destroy(this.gameObject);
		}
	}
}
