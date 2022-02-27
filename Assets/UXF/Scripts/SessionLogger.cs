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
		public static SessionLogger instance { get; private set; }

		public bool setAsMainInstance = true;
		public bool logDebugLogCalls = true;

		private Session session;
		private string[] header = new string[]{ "timestamp", "log_type", "message"};
		private UXFDataTable table;

		void Awake()
		{
			if (setAsMainInstance) instance = this;

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
			table = new UXFDataTable("timestamp", "log_type", "message", "stacktrace");
            if (logDebugLogCalls) Application.logMessageReceived += HandleLog;
			session.preSessionEnd.AddListener(Finalise); // finalise logger when cleaning up the session
		}		

		void HandleLog(string logString, string stackTrace, LogType type)
		{
			var row = new UXFDataRow();

			row.Add(("timestamp", Time.time.ToString()));
			row.Add(("log_type", type.ToString()));
			row.Add(("message", logString.Replace(",", string.Empty)));
			row.Add(("stacktrace", stackTrace.Replace(",", string.Empty).Replace("\n", ".  ").Replace("\r", ".  ")));

			table.AddCompleteRow(row);
		}

		/// <summary>
		/// Manually log a message to the log file.
		/// </summary>
		/// <param name="text">The content you wish to log, expressed as a string.</param>
		/// <param name="logType">The type of the log. This can be any string you choose. Default is \"user\"</param>
		public void Log(string text, string logType = "user")
		{
			var row = new UXFDataRow();

			row.Add(("timestamp", Time.time.ToString()));
			row.Add(("log_type", logType));
			row.Add(("message", text.Replace(",", string.Empty)));
			row.Add(("stacktrace", "NA"));

			table.AddCompleteRow(row);
		}

        /// <summary>
        /// Finalises the session logger, saving the data and detaching its logging method from handling Debug.Log messages  
        /// </summary>
        public void Finalise(Session session)
		{
			if (session.saveData)
			{
				session.SaveDataTable(table, "log", dataType: UXFDataType.SessionLog);
			}

			if (logDebugLogCalls) Application.logMessageReceived -= HandleLog;
			session.preSessionEnd.RemoveListener(Finalise);
        }

	}

}