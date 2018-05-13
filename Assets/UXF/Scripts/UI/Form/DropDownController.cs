using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace UXF
{
    public class DropDownController : FormElementController
    {
        public Dropdown dropdown;

        public List<string> optionNames
        {
            get { return dropdown.options.Select(x => x.text).ToList(); }
        }

        protected override void Setup()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(entry.dropDownValues);
        }

        public override object GetContents()
        {
            return optionNames[dropdown.value];
        }

        public override void SetContents(object newContents)
        {
            int index = optionNames.IndexOf(newContents.ToString());
            if (index == -1)
            {
                Clear();
                DisplayFault();
                return;
            }
            dropdown.value = index;
        }

        public void SetItems(List<string> values)
        {
            dropdown.options = new List<Dropdown.OptionData>();
            dropdown.AddOptions(values);
        }

        public override object GetDefault()
        {
            return optionNames[0];
        }

        public override void Clear()
        {
            dropdown.value = 0;
        }

    }
}