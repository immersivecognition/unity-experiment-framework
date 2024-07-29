﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.IO;
using System.Threading;
using System.Linq;
using System.Globalization;

namespace UXF
{
    /// <summary>
    /// Component which manages File I/O in a seperate thread to avoid hitches.
    /// </summary>
    public class FileSaver : LocalFileDataHander
    {

        [Tooltip("Enable to sort session files into folders. The trial_results CSV is never put into a folder.")]
        public bool sortDataIntoFolders = true;

        /// <summary>
        /// Enable to force the data to save with an english-US format (i.e. `,` to serapate values,
        /// and `.` to separate decimal points).
        /// </summary>
        [Tooltip("Enable to force the data to save with an english-US format (i.e. `,` to serapate values, and `.` to separate decimal points).")]
        public bool forceENUSLocale = true;

        /// <summary>
        /// Enable to print debug messages to the console.
        /// </summary>
        [Tooltip("Enable to print debug messages to the console.")]
        public bool verboseDebug = false;

        /// <summary>
        /// An action which does nothing.
        /// </summary>
        /// <returns></returns>
        public static System.Action doNothing = () => { };

        public bool IsActive { get { return parallelThread != null && parallelThread.IsAlive; } }


        BlockingQueue<System.Action> bq = new BlockingQueue<System.Action>();
        Thread parallelThread;

        bool quitting = false;


        /// <summary>
        /// Starts the FileSaver Worker thread.
        /// </summary>
        public override void SetUp()
        {
            if (forceENUSLocale)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            }

            quitting = false;
            Directory.CreateDirectory(base.StoragePath);

            if (!IsActive)
            {
                parallelThread = new Thread(Worker);
                if (forceENUSLocale)
                {
                    parallelThread.CurrentCulture = new CultureInfo("en-US");
                }
                parallelThread.Start();
            }
            else
            {
                Utilities.UXFDebugLogWarning("Parallel thread is still active!");
            }
        }

        /// <summary>
        /// Adds a new command to a queue which is executed in a separate worker thread when it is available.
        /// Warning: The Unity Engine API is not thread safe, so do not attempt to put any Unity commands here.
        /// </summary>
        /// <param name="action"></param>
        public void ManageInWorker(System.Action action)
        {

            if (quitting)
            {
                throw new System.InvalidOperationException(
                    string.Format(
                        "Cannot add action to FileSaver, is currently quitting. Action: {0}.{1}",
                        action.Method.ReflectedType.FullName,
                        action.Method.Name
                        )
                );
            }

            bq.Enqueue(action);
        }

        void Worker()
        {
            if (verboseDebug)
                Utilities.UXFDebugLog("Started worker thread");

            // performs FileIO tasks in seperate thread
            foreach (var action in bq)
            {
                if (verboseDebug)
                    Utilities.UXFDebugLogFormat("Managing action");

                try
                {
                    action.Invoke();                
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (IOException e)
                {
                    Utilities.UXFDebugLogError(string.Format("Error, file may be in use! Exception: {0}", e));
                }
                catch (System.Exception e)
                {
                    // stops thread aborting upon an exception
                    Debug.LogException(e);
                }

                if (quitting && bq.NumItems() == 0)
                    break;
            }

            if (verboseDebug)
                Utilities.UXFDebugLog("Finished worker thread");
        }

        /// <summary>
        /// Move `path` to a back up path.
        /// Backup is the path file name with its LastWriteTime as a suffix.
        /// </summary>
        protected virtual void MoveToBackup(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            string suffix = File.GetLastWriteTime(path).ToString("dd-MM-yyyy-HH-mm-FF");
            string ext = Path.GetExtension(path);
            string newPath = Path.Combine(Path.GetDirectoryName(path), $"{fileName}_{suffix}{ext}");
            File.Move(path, newPath);
        }

        /// <summary>
        /// Same as File.WriteAllText, but makes sure files are not overwritten.
        /// If file file exists, the old file will be renamed with a suffix with its LastWriteTime.
        /// </summary>
        protected virtual void SafeFileWriteAllText(string path, string content)
        {
            if (File.Exists(path))
            {
                MoveToBackup(path);
            }
            File.WriteAllText(path, content);
        }

        /// <summary>
        /// Same as File.WriteAllLines, but makes sure files are not overwritten.
        /// If file file exists, the old file will be renamed with a suffix with its LastWriteTime.
        /// </summary>
        protected virtual void SafeFileWriteAllLines(string path, string[] content)
        {
            if (File.Exists(path))
            {
                MoveToBackup(path);
            }
            File.WriteAllLines(path, content);
        }

        /// <summary>
        /// Same as File.WriteAllBytes, but makes sure files are not overwritten.
        /// If file file exists, the old file will be renamed with a suffix with its LastWriteTime.
        /// </summary>
        protected virtual void SafeFileWriteAllBytes(string path, byte[] content)
        {
            if (File.Exists(path))
            {
                MoveToBackup(path);
            }
            File.WriteAllBytes(path, content);
        }

        /// <summary>
        /// Returns true if there may be a risk of overwriting data.
        /// </summary>
        /// <param name="experiment"></param>
        /// <param name="ppid"></param>
        /// <param name="sessionNum"></param>
        /// <returns></returns>
        public override bool CheckIfRiskOfOverwrite(string experiment, string ppid, int sessionNum, string rootPath = "")
        {
            string directory = Path.Combine(rootPath, experiment, ppid, SessionNumToName(sessionNum));
            return System.IO.Directory.Exists(directory);
        }


        public override string HandleDataTable(UXFDataTable table, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            string ext  = Path.GetExtension(dataName);
            dataName = Path.GetFileNameWithoutExtension(dataName);

            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial) dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string[] lines = table.GetCSVLines();
            
            string directory = GetSessionPath(experiment, ppid, sessionNum);
            if (sortDataIntoFolders && dataType != UXFDataType.TrialResults) directory = Path.Combine(directory, dataType.GetFolderName());
            Directory.CreateDirectory(directory);
            string name = string.IsNullOrEmpty(ext) ? string.Format("{0}.csv", dataName) : string.Format("{0}{1}", dataName, ext);
            string savePath = Path.Combine(directory, name);
            
            if (verboseDebug) Utilities.UXFDebugLogFormat("Queuing save of file: {0}", savePath);

            ManageInWorker(() => { SafeFileWriteAllLines(savePath, lines); });
            return GetRelativePath(StoragePath, savePath);
        }

        public override string HandleJSONSerializableObject(List<object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            string ext  = Path.GetExtension(dataName);
            dataName = Path.GetFileNameWithoutExtension(dataName);

            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial) dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string text = MiniJSON.Json.Serialize(serializableObject);

            string directory = GetSessionPath(experiment, ppid, sessionNum);
            if (sortDataIntoFolders && dataType != UXFDataType.TrialResults) directory = Path.Combine(directory, dataType.GetFolderName());
            Directory.CreateDirectory(directory);
            string name = string.IsNullOrEmpty(ext) ? string.Format("{0}.json", dataName) : string.Format("{0}{1}", dataName, ext);
            string savePath = Path.Combine(directory, name);
            
            if (verboseDebug) Utilities.UXFDebugLogFormat("Queuing save of file: {0}", savePath);

            ManageInWorker(() => { SafeFileWriteAllText(savePath, text); });
            return GetRelativePath(StoragePath, savePath);;
        }

        public override string HandleJSONSerializableObject(Dictionary<string, object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            string ext  = Path.GetExtension(dataName);
            dataName = Path.GetFileNameWithoutExtension(dataName);

            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial) dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string text = MiniJSON.Json.Serialize(serializableObject);

            string directory = GetSessionPath(experiment, ppid, sessionNum);
            if (sortDataIntoFolders && dataType != UXFDataType.TrialResults) directory = Path.Combine(directory, dataType.GetFolderName());
            Directory.CreateDirectory(directory);
            string name = string.IsNullOrEmpty(ext) ? string.Format("{0}.json", dataName) : string.Format("{0}{1}", dataName, ext);
            string savePath = Path.Combine(directory, name);
            
            if (verboseDebug) Utilities.UXFDebugLogFormat("Queuing save of file: {0}", savePath);

            ManageInWorker(() => { SafeFileWriteAllText(savePath, text); });
            return GetRelativePath(StoragePath, savePath);;
        }

        public override string HandleText(string text, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            string ext  = Path.GetExtension(dataName);
            dataName = Path.GetFileNameWithoutExtension(dataName);

            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial) dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string directory = GetSessionPath(experiment, ppid, sessionNum);
            if (sortDataIntoFolders && dataType != UXFDataType.TrialResults) directory = Path.Combine(directory, dataType.GetFolderName());
            Directory.CreateDirectory(directory);

            string name = string.IsNullOrEmpty(ext) ? string.Format("{0}.txt", dataName) : string.Format("{0}{1}", dataName, ext);
            string savePath = Path.Combine(directory, name);
            
            if (verboseDebug) Utilities.UXFDebugLogFormat("Queuing save of file: {0}", savePath);

            ManageInWorker(() => { SafeFileWriteAllText(savePath, text); });
            return GetRelativePath(StoragePath, savePath);;
        }

        public override string HandleBytes(byte[] bytes, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            string ext  = Path.GetExtension(dataName);
            dataName = Path.GetFileNameWithoutExtension(dataName);

            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial) dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string directory = GetSessionPath(experiment, ppid, sessionNum);
            if (sortDataIntoFolders && dataType != UXFDataType.TrialResults) directory = Path.Combine(directory, dataType.GetFolderName());
            Directory.CreateDirectory(directory);

            string name = string.IsNullOrEmpty(ext) ? string.Format("{0}.txt", dataName) : string.Format("{0}{1}", dataName, ext);
            string savePath = Path.Combine(directory, name);

            if (verboseDebug) Utilities.UXFDebugLogFormat("Queuing save of file: {0}", savePath);

            ManageInWorker(() => { SafeFileWriteAllBytes(savePath, bytes); });
            return GetRelativePath(StoragePath, savePath);
        }


        public string GetSessionPath(string experiment, string ppid, int sessionNum)
        {
            string storageLocationSafe = base.StoragePath;
            if (!System.IO.Directory.Exists(base.StoragePath))
            {
                storageLocationSafe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "UXF_Data");
                Directory.CreateDirectory(storageLocationSafe);
                Utilities.UXFDebugLogErrorFormat("Selected storage location ({0}) does not exist! Defaulting to {1}.", base.StoragePath, storageLocationSafe);
            }
            return Path.Combine(storageLocationSafe, experiment, ppid, SessionNumToName(sessionNum));
        }

        public string GetSessionPath(Session session)
        {
            string storageLocationSafe = base.StoragePath;
            if (!System.IO.Directory.Exists(base.StoragePath))
            {
                storageLocationSafe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "UXF_Data");
                Directory.CreateDirectory(storageLocationSafe);
                Utilities.UXFDebugLogErrorFormat("Selected storage location ({0}) does not exist! Defaulting to {1}.", base.StoragePath, storageLocationSafe);
            }
            return Path.Combine(storageLocationSafe, session.experimentName, session.ppid, SessionNumToName(session.number));
        }

        public static string SessionNumToName(int num)
        {
            return string.Format("S{0:000}", num);
        }

        /// <summary>
        /// Aborts the FileSaver's thread and joins the thread to the calling thread.
        /// </summary>
        public override void CleanUp()
        {
            if (verboseDebug)
                Utilities.UXFDebugLog("Joining File Saver Thread");
            quitting = true;
            bq.Enqueue(doNothing); // ensures bq breaks from foreach loop
            parallelThread.Join();
        }

        public static string GetRelativePath(string relativeToDirectory, string path)
        {
            relativeToDirectory = Path.GetFullPath(relativeToDirectory);
            if (!relativeToDirectory.EndsWith("\\")) relativeToDirectory += "\\";

            path = Path.GetFullPath(path);

            Uri path1 = new Uri(relativeToDirectory);
            Uri path2 = new Uri(path);

            Uri diff = path1.MakeRelativeUri(path2);
            return Uri.UnescapeDataString(diff.OriginalString);
        }
    }
}
