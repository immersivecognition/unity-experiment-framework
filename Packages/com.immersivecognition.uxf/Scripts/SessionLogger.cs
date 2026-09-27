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
		private bool logCallbackAttached;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void ResetRuntimeState()
		{
			instance = null;
		}

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
            if (newSession == null) return;

            if (object.ReferenceEquals(session, newSession))
            {
                if (table != null)
                {
                    session.preSessionEnd.RemoveListener(Finalise);
                    session.preSessionEnd.AddListener(Finalise);
                }
                return;
            }

            if (table != null && session != null)
            {
                session.preSessionEnd.RemoveListener(Finalise);
            }

            session = newSession;

            if (table != null)
            {
                session.preSessionEnd.RemoveListener(Finalise);
                session.preSessionEnd.AddListener(Finalise);
            }
        }

		/// <summary>
		/// Initialises the session logger, creating the internal data structures, and attaching its logging method to handle Debug.Log messages 
		/// </summary>
		public void Initialise()
		{
			table = new UXFDataTable("timestamp", "log_type", "message", "stacktrace");
			if (logDebugLogCalls && !logCallbackAttached)
            {
                Application.logMessageReceived += HandleLog;
                logCallbackAttached = true;
            }
			else if (!logDebugLogCalls)
			{
				Detach();
			}
			if (session != null)
			{
				session.preSessionEnd.RemoveListener(Finalise);
				session.preSessionEnd.AddListener(Finalise); // finalise logger when cleaning up the session
			}
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
			if (session != null && session.saveData)
			{
				session.SaveDataTable(table, "log", dataType: UXFDataType.SessionLog);
			}

            Detach();
			if (session != null) session.preSessionEnd.RemoveListener(Finalise);
        }

		void OnDestroy()
		{
			Detach();
			if (session != null) session.preSessionEnd.RemoveListener(Finalise);
			if (object.ReferenceEquals(instance, this)) instance = null;
		}

		void Detach()
		{
			if (logCallbackAttached)
			{
				Application.logMessageReceived -= HandleLog;
				logCallbackAttached = false;
			}
		}

	}

}
