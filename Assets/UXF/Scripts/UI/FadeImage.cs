using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SubjectNerd.Utilities;

namespace UXF.UI
{
    public class FadeImage : MonoBehaviour
    {

        public float duration = 1f;
        public AnimationCurve curve;
        public CanvasGroup group;

        public void BeginFade()
        {
            StopAllCoroutines();
            StartCoroutine(FadeSequence());
        }

        IEnumerator FadeSequence()
        {
            float startTime = Time.time;
            float t = 0;
            
            while ((t = (Time.time - startTime) / duration) < 1)
            {
                group.alpha = curve.Evaluate(t);
                yield return null;
            }

            group.alpha = 0;
        }

    }

}