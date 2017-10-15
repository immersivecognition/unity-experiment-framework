using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FormElementController : MonoBehaviour {

    public RectTransform rectTransform;
    public Text title;
    [HideInInspector] public FormDataType dataType;
    protected string originalTitle;


    public float height {  get { return rectTransform.rect.height; } }

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Setup(string titleText, FormDataType dType)
    {
        title.text = titleText;
        originalTitle = titleText;
        dataType = dType;
        Setup();
    }

    virtual protected void Setup()
    {

    }


    virtual public object GetContents()
    {
        return null;
    }


    public void DisplayFault()
    {
        title.text = string.Format("{0} <b><color=red>(!) Error</color></b>", originalTitle);
        Invoke("ResetTitle", 5);
    }

    public void ResetTitle()
    {
        title.text = originalTitle;
    }

}
