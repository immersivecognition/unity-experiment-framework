using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UXF.UI
{
    public class ParticipantSelector : MonoBehaviour
    {
        public ExperimentStartupController startup;
        public FillableFormController form;
        public DropDownController participantDropdown;
        public DirectorySelector ppListSelect;


        void Awake()
        {
            participantDropdown.SetItems(new List<string>() { startup.newParticipantName } ) ;
        }

        public void SetParticipants(List<string> participants)
        {
            participants.Insert(0, startup.newParticipantName);
            participantDropdown.SetItems(participants);
        }


        public void SelectItem(int value)
        {
            if (value == 0) // "new participant"
            {
                form.Clear();
                TextFormController ppidText = (TextFormController) form.ppidElement.controller;
                ppidText.SetToTimeNow();
            }
        }

        public bool IsNewSelected()
        {
            return participantDropdown.dropdown.value == 0;
        }

        public string GetItem()
        {
            return form.ppidElement.controller.GetContents().ToString();
        }
    }
}