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
		private List<Dictionary<string, string>> table;

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
			table = new List<Dictionary<string, string>>();
            Application.logMessageReceived += HandleLog;
			session.cleanUp += Finalise; // finalise logger when cleaning up the session
		}		

		void HandleLog(string logString, string stackTrace, LogType type)
		{
			table.Add(
				new Dictionary<string, string>()
				{
					{ "timestamp", Time.time.ToString() },
					{ "log_type", type.ToString() },
					{ "message" , logString.Replace(",", string.Empty) }
				}
			);
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

			string[] lines = new string[table.Count + 1];
			lines[0] = string.Join(",", header);
			
			int i = 1;
			foreach (var line in table)
			{
				lines[i++] = string.Join(",", 
					header
						.Select((h) => line[h])
						.ToArray()
				);
			}

			fileIOManager.ManageInWorker(() => fileIOManager.WriteAllLines(lines, fileInfo));

            Application.logMessageReceived -= HandleLog;
			session.cleanUp -= Finalise;
        }

	}

}