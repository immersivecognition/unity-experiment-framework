using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UXF.UI
{
    public class FormElement : MonoBehaviour
    {
        public Text title;

        [SerializeField]
        private UnityEvent onDisplayFault = new UnityEvent();
        private Func<object> getContentsAction = () => { return null; };
        private Action<object> setContentsAction = (o) => { };

        public void Initialise(Func<object> getContentsAction, Action<object> setContentsAction)
        {
            this.getContentsAction = getContentsAction;
            this.setContentsAction = setContentsAction;
        }

        public void DisplayFault()
        {
            onDisplayFault.Invoke();
        }

        public object GetContents()
        {
            return getContentsAction.Invoke();
        }

        public void SetContents(object contents)
        {
            setContentsAction.Invoke(contents);
        }

    }

}