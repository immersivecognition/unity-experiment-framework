using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UXF.UI
{
	/// <summary>
	/// A script to control the NotesPanel
	/// </summary>
	public class NotesController : MonoBehaviour 
	{
		public GameObject panel;
		public GameObject contentParent;
		public GameObject notePrefab;

		public InputField newNoteInput;

		public ScrollRect notesScrollRect;

		public Toggle ignoreCheckbox;

		[Space]
		public Session session;

		void Start()
		{
			session.preSessionEnd.AddListener(WriteNotes); // will write notes when session is finished
		}

		/// <summary>
		/// Instantiates a new note prefab with the text currently in the new note input field
		/// </summary>
		public void AddNewNote()
		{
			if (!string.IsNullOrEmpty(newNoteInput.text))
			{
				GameObject newNote = Instantiate(notePrefab, notePrefab.transform.position, notePrefab.transform.rotation, contentParent.transform);
				newNote.GetComponentInChildren<InputField>().text = newNoteInput.text;

				newNoteInput.text = "";
			}
		}

		/// <summary>
		/// Starts a coroutine that yields 1 frame to allow the canvas to update, and then moves the notes scroll rect to the latest element added
		/// </summary>
		public void MoveScrollbarToBottom()
		{
			StartCoroutine(DelayedMoveScrollbarToBottom());
		}

		private IEnumerator DelayedMoveScrollbarToBottom()
		{
			yield return null;
			notesScrollRect.verticalNormalizedPosition = 0;
		}

		/// <summary>
		/// Performs actions when the NotePanel is turned off and on
		/// </summary>
		public void ToggleVisibility()
		{
			panel.SetActive(!panel.activeSelf);
		}
		
		/// <summary>
		/// Writes the session notes to a json file. File includes whether the session is marked as bad and any note added by the experimenter
		/// </summary>
		private void WriteNotes(Session session)
		{
			Dictionary<string, object> sessionNotes = new Dictionary<string, object>()
			{
				{ "session_marked_as_bad", ignoreCheckbox.isOn }
			};

			string notesKey = "notes";
			List<string> notesValue = new List<string>();

			foreach (Transform child in contentParent.transform)
			{
				string value = child.GetComponentInChildren<InputField>().text;

				if (string.IsNullOrEmpty(value))
					continue;
				
				notesValue.Add(value);
			}

			sessionNotes.Add(notesKey, notesValue);
			session.SaveJSONSerializableObject(sessionNotes, "notes");
		}

		/// <summary>
		/// Reset the notes panel to default values
		/// </summary>
		[ContextMenu("Reset Notes Panel")]
		public void ResetNotes()
		{
			ignoreCheckbox.isOn = false;

			newNoteInput.text = "";

			foreach(Transform child in contentParent.transform)
				Destroy(child.gameObject);
		}
	}
}
