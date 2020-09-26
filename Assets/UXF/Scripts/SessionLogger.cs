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
		private FileIOManager fileIOManager;
		private string[] header = new string[]{ "timestamp", "log_type", "message"};
		private UXFDataTable table;

		void Awake()
		{
			AttachReferences(
				newFileIOManager: GetComponent<FileIOManager>(),
				newSession: GetComponent<Session>()
			);
			Initialise();
		}

        /// <summary>
        /// Provide references to other components 
        /// </summary>
        /// <param name="newFileIOManager"></param>
        /// <param name="newSession"></param>
        public void AttachReferences(FileIOManager newFileIOManager = null, Session newSession = null)
        {
            if (newFileIOManager != null) fileIOManager = newFileIOManager;
            if (newSession != null) session = newSession;
        }

		/// <summary>
		/// Initialises the session logger, creating the internal data structures, and attaching its logging method to handle Debug.Log messages 
		/// </summary>
		public void Initialise()
		{
			table = new UXFDataTable("timestamp", "log_type", "message");
            Application.logMessageReceived += HandleLog;
			session.cleanUp += Finalise; // finalise logger when cleaning up the session
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
        public void Finalise()
		{
            WriteFileInfo fileInfo = new WriteFileInfo(
                WriteFileType.Log,
                session.BasePath,
                session.experimentName,
                session.ppid,
                session.FolderName,
                "log.csv"
                );

			string[] lines = table.GetCSVLines();

			fileIOManager.ManageInWorker(() => fileIOManager.WriteAllLines(lines, fileInfo));

            Application.logMessageReceived -= HandleLog;
			session.cleanUp -= Finalise;
        }

	}

}