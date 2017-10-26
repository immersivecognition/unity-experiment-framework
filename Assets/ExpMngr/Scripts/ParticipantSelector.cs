using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExpMngr
{
    public class ParticipantSelector : MonoBehaviour
    {
        public ExperimentStartupController startup;
        public FillableFormController form;
        public DropDownController participantDropdown;
        public DirectorySelection dirSelect;

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

        public void SelectNew()
        {
            participantDropdown.Clear();
        }


        public void SelectItem(int value)
        {
            if (value == 0) // "new participant"
            {
                form.Clear();
            }
            else
            {
                dirSelect.UpdateFormByPPID((string) participantDropdown.GetContents());
            }
        }

        public bool IsNewSelected()
        {
            return participantDropdown.dropdown.value == 0;
        }
    }
}