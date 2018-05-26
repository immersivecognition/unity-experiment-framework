using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;


namespace UXF {

	public class SessionLogger : MonoBehaviour
	{	
		private Session session;
		private FileIOManager fileIOManager;
		private DataTable table;

		void Awake()
		{
			AttachReferences(
				newFileIOManager: GetComponent<FileIOManager>(),
				newSession: GetComponent<Session>()
			);
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

		public void Initialise()
		{
			table = new DataTable();
			table.Columns.Add(
				new DataColumn("timestamp", typeof(float))
			);
            table.Columns.Add(
                new DataColumn("log_type", typeof(string))
            );
            table.Columns.Add(
                new DataColumn("message", typeof(string))
            );

            Application.logMessageReceived += HandleLog;
		}		

		void HandleLog(string logString, string stackTrace, LogType type)
		{
			DataRow row = table.NewRow();
			row["timestamp"] = Time.time;
			row["log_type"] = type.ToString();
			row["message"] = logString.Replace(",", string.Empty);
			table.Rows.Add(row);
		}
		
		public void Finalise()
		{
			string filepath = Path.Combine(session.path, "log.csv");
			fileIOManager.ManageInWorker(() => fileIOManager.WriteCSV(table, filepath));
            Application.logMessageReceived -= HandleLog;
        }

	}

}