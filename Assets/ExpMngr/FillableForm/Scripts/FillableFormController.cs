using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillableFormController : MonoBehaviour {

    public List<FormElementEntry> formElements;

    [Space]

    public GameObject contentParent;
    public GameObject textPrefab;
    public GameObject checkBoxPrefab;

    // Use this for initialization
    void Start () {
        Generate();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    [ContextMenu("Generate items")]
    public void Generate()
    {

        while (contentParent.transform.childCount != 0)
        {
            DestroyImmediate(contentParent.transform.GetChild(0).gameObject);
        }

        float h = 0;
        List<string> names = new List<string>(); 
        foreach  (FormElementEntry f in formElements)
        {
            if (!names.Contains(f.internalName))
            {
                names.Add(f.internalName);
                GameObject createdElement = CreateElement(f);
                f.Initialise(createdElement.GetComponent<FormElementController>());
                createdElement.transform.position += new Vector3(0, -h, 0);
                h += f.controller.height;
            } else
            {
                throw new System.Exception("Duplicated internal names. They must be unique.");
            }
        }
        RectTransform rt = contentParent.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.rect.width, h);


    }


    GameObject CreateElement(FormElementEntry formElementEntry)
    {
        switch (formElementEntry.dataType)
        {
            case FormDataType.String:
            case FormDataType.Int:
            case FormDataType.Float:
                return Instantiate(textPrefab, contentParent.transform);
            case FormDataType.Bool:
                return Instantiate(checkBoxPrefab, contentParent.transform);
            default:
                return null;
        }
    }

    public void CompleteForm()
    {
        GetCompletedForm();
    }

    public Dictionary<string, object> GetCompletedForm()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        bool fault = false;
        foreach (FormElementEntry f in formElements)
        {
            try
            {
                dict.Add(f.internalName, f.controller.GetContents());
                f.controller.ResetTitle();
            }
            catch (System.FormatException e)
            {
                fault = true;
                f.controller.DisplayFault();
            }
        }
        if (!fault)
            return dict;
        else
            return null;
    }    
}

[System.Serializable]
public class FormElementEntry
{
    public string displayName;
    public string internalName;
    public FormDataType dataType;
    public FormElementController controller { get; set; }

    public void Initialise(FormElementController formElementController)
    {
        controller = formElementController;
        controller.Setup(displayName, dataType);
    }

}


public enum FormDataType
{
    String, Float, Int, Bool
}