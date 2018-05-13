using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UXF
{
    public class ParticipantSelector : MonoBehaviour
    {
        public ExperimentStartupController startup;
        public FillableFormController form;
        public DropDownController participantDropdown;
        public ParticipantListSelection ppListSelect;

        public List<string> ppList;

        void Awake()
        {
            participantDropdown.SetItems(new List<string>() { startup.newParticipantName } ) ;
        }

        public void SetParticipants(List<string> participants)
        {
            ppList = participants;
            ppList.Insert(0, startup.newParticipantName);
            participantDropdown.SetItems(ppList);
        }

        public void SelectNewList()
        {
            participantDropdown.Clear();
        }


        public void SelectItem(int value)
        {
            if (value == 0) // "new participant"
            {
                form.Clear();
                TextFormController ppidText = (TextFormController) form.ppidElement.controller;
                ppidText.SetToTimeNow();
            }
            else
            {
                ppListSelect.UpdateFormByPPID((string) participantDropdown.GetContents());
            }
        }

        public bool IsNewSelected()
        {
            return participantDropdown.dropdown.value == 0;
        }
    }
}