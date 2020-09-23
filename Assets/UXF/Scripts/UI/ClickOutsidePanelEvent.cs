using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UXF.UI
{
	/// <summary>
	/// A script that invokes a UnityEvent if you click outside of the attached UI rectTransform area
	/// Useful for closing a panel when a mouse click is outside of the panel
	/// </summary>
	public class ClickOutsidePanelEvent : MonoBehaviour
	{
		public UnityEvent onClickOutsidePanel;

		private Canvas parentCanvas;

		void Start()
		{
			parentCanvas = this.GetComponentInParent<Canvas>();
		}

		void Update()
		{
			HideIfClickedOutside();
		}

		/// <summary>
		/// Invokes an event if a mouse click was made outside the bounds of the UI the script is attached to
		/// </summary>
		private void HideIfClickedOutside ()
		{
			if (Input.GetMouseButtonDown(0) && CheckVisible() &&
				!RectTransformUtility.RectangleContainsScreenPoint (
					this.GetComponent<RectTransform>(),
					Input.mousePosition,
					null))
			{
				onClickOutsidePanel.Invoke ();
			}
		}

		/// <summary>
		/// Check if the GameObject the script is attached to is visible in the scene
		/// - If there is no parent canvas, the GameObject has to be active in the hierarchy
		/// - If there is a parent canvas, the canvas has to be enabled and the GameObject active in the hierarchy
		/// </summary>
		/// <returns>true if the GameObject is visible in the scene</returns>
		private bool CheckVisible()
		{
			bool visible = false; 

			if (parentCanvas == null)
				visible = true;
			else
				if (parentCanvas.enabled)
					visible = true;
			
			return visible;
		}
	}
}