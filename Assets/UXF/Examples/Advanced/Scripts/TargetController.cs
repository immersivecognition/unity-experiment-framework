using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

namespace UXFExamples
{

    public class TargetController : MonoBehaviour
    {

        Color normalColor;
        public Color highlightedColor = Color.yellow;

        SpriteRenderer spriteRenderer;
        TargetManager manager;

		public bool? isCorrect = null;

        void Awake()
        {
			manager = GetComponentInParent<TargetManager>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            normalColor = spriteRenderer.color;
            ResetToNormal();
        }

        public void ResetToNormal()
        {
            isCorrect = null;
            spriteRenderer.color = normalColor;
            enabled = false;
        }

        public void Setup(bool isCorrect)
        {
			this.isCorrect = isCorrect;
            enabled = true;
        }

        public void Highlight()
        {
            spriteRenderer.color = highlightedColor;
        }

        void OnMouseEnter()
        {
			if (enabled)
            	manager.TargetHit(this);
        }


    }

}