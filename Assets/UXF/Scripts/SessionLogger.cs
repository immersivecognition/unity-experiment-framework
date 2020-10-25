using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace UXF
{
	/// <summary>
	/// Component that handles collecting all Debug.Log calls
	/// </summary>
	public class SessionLogger : MonoBehaviour
	{	
		private Session session;
		private string[] header = new string[]{ "timestamp", "log_type", "message"};
		private UXFDataTable table;

		void Awake()
		{
			AttachReferences(
				newSession: GetComponent<Session>()
			);
			Initialise();
		}

        /// <summary>
        /// Provide references to other components 
        /// </summary>
        /// <param name="newSession"></param>
        public void AttachReferences(Session newSession = null)
        {
            if (newSession != null) session = newSession;
        }

		/// <summary>
		/// Initialises the session logger, creating the internal data structures, and attaching its logging method to handle Debug.Log messages 
		/// </summary>
		public void Initialise()
		{
			table = new UXFDataTable("timestamp", "log_type", "message");
            Application.logMessageReceived += HandleLog;
			session.preSessionEnd.AddListener(Finalise); // finalise logger when cleaning up the session
		}		

		void HandleLog(string logString, string stackTrace, LogType type)
		{
			var row = new UXFDataRow();

			row.Add(("timestamp", Time.time.ToString()));
			row.Add(("log_type", type.ToString()));
			row.Add(("message", logString.Replace(",", string.Empty)));

			table.AddCompleteRow(row);
		}

		/// <summary>
		/// Manually log a message to the log file.
		/// </summary>
		/// <param name="logType">The type of the log. This can be any string you choose.</param>
		/// <param name="message">The content you wish to log, expressed as a string.</param>
		public void WriteLog(string logType, string value)
		{
			var row = new UXFDataRow();

			row.Add(("timestamp", Time.time.ToString()));
			row.Add(("log_type", logType));
			row.Add(("message", value.Replace(",", string.Empty)));

			table.AddCompleteRow(row);
		}

        /// <summary>
        /// Finalises the session logger, saving the data and detaching its logging method from handling Debug.Log messages  
        /// </summary>
        public void Finalise(Session session)
		{
			session.SaveDataTable(table, "log", dataType: UXFDataType.SessionLog);

            Application.logMessageReceived -= HandleLog;
			session.preSessionEnd.RemoveListener(Finalise);
        }

	}

}