using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Linq;


namespace UXF
{
    /// <summary>
    /// Component which manages File I/O in a seperate thread to avoid hitches.
    /// </summary>
    public class FileSaver : DataHandler
    {
        [Space]

        [Tooltip("Should the location the data is stored in be: Acquired via the UI, or, a fixed path?")]
        public DataSaveLocation dataSaveLocation;

        [Tooltip("If fixed path is selected, where should the data be stored?")]
        [BasteRainGames.HideIfEnumValue("dataSaveLocation", BasteRainGames.HideIf.Equal, (int) DataSaveLocation.AcquireFromUI)]
        public string fixedSaveLocation = "~";


        [Tooltip("Enable to sort session files into folders. The trial_results CSV is never put into a folder.")]
        public bool sortDataIntoFolders = true;

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

        public override bool RequiresDataPathUIElement { get { return dataSaveLocation == DataSaveLocation.AcquireFromUI; } }

        BlockingQueue<System.Action> bq = new BlockingQueue<System.Action>();
        Thread parallelThread;

        bool quitting = false;


        /// <summary>
        /// Starts the FileIOManager Worker thread.
        /// </summary>
        public override void SetUp()
        {
            quitting = false;
            Directory.CreateDirectory(base.storageLocation);

            if (!IsActive)
            {
                parallelThread = new Thread(Worker);
                parallelThread.Start();
            }
            else
            {
                Debug.LogWarning("Parallel thread is still active!");
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
                        "Cant add action to FileIOManager, is currently quitting. Action: {0}.{1}",
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
                Debug.Log("Started worker thread");

            // performs FileIO tasks in seperate thread
            foreach (var action in bq)
            {
                if (verboseDebug)
                    Debug.LogFormat("Managing action");

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
                    Debug.LogError(string.Format("Error, file may be in use! Exception: {0}", e));
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
                Debug.Log("Finished worker thread");
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


        public override string HandleDataTable(UXFDataTable table, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.SessionInfo)
        {
            string[] lines = table.GetCSVLines();
            
            string directory = GetSessionPath(experiment, ppid, sessionNum);
            if (sortDataIntoFolders && dataType != DataType.TrialResults) directory = Path.Combine(directory, dataType.ToLower());
            Directory.CreateDirectory(directory);
            string savePath = Path.Combine(directory, string.Format("{0}.csv", dataName));
            
             if (verboseDebug) Debug.LogFormat("Queuing save of file: {0}", savePath);

            ManageInWorker(() => { File.WriteAllLines(savePath, lines); });
            return savePath;
        }

        public override string HandleJSONSerializableObject(List<object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.SessionInfo)
        {
            string text = MiniJSON.Json.Serialize(serializableObject);

            string directory = GetSessionPath(experiment, ppid, sessionNum);
            if (sortDataIntoFolders && dataType != DataType.TrialResults) directory = Path.Combine(directory, dataType.ToLower());
            Directory.CreateDirectory(directory);
            string savePath = Path.Combine(directory, string.Format("{0}.csv", dataName));
            
             if (verboseDebug) Debug.LogFormat("Queuing save of file: {0}", savePath);

            ManageInWorker(() => { File.WriteAllText(savePath, text); });
            return savePath;
        }

        public override string HandleJSONSerializableObject(Dictionary<string, object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.SessionInfo)
        {
            string text = MiniJSON.Json.Serialize(serializableObject);

            string directory = GetSessionPath(experiment, ppid, sessionNum);
            if (sortDataIntoFolders && dataType != DataType.TrialResults) directory = Path.Combine(directory, dataType.ToLower());
            Directory.CreateDirectory(directory);
            string savePath = Path.Combine(directory, string.Format("{0}.csv", dataName));
            
             if (verboseDebug) Debug.LogFormat("Queuing save of file: {0}", savePath);

            ManageInWorker(() => { File.WriteAllText(savePath, text); });
            return savePath;
        }

        public override string HandleText(string text, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.SessionInfo)
        {
            string directory = GetSessionPath(experiment, ppid, sessionNum);
            if (sortDataIntoFolders && dataType != DataType.TrialResults) directory = Path.Combine(directory, dataType.ToLower());
            Directory.CreateDirectory(directory);
            string savePath = Path.Combine(directory, string.Format("{0}.csv", dataName));
            
             if (verboseDebug) Debug.LogFormat("Queuing save of file: {0}", savePath);

            ManageInWorker(() => { File.WriteAllText(savePath, text); });
            return savePath;
        }

        public override string HandleBytes(byte[] bytes, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.SessionInfo)
        {
            string directory = GetSessionPath(experiment, ppid, sessionNum);
            if (sortDataIntoFolders && dataType != DataType.TrialResults) directory = Path.Combine(directory, dataType.ToLower());
            Directory.CreateDirectory(directory);
            string savePath = Path.Combine(directory, string.Format("{0}.txt", dataName));  

             if (verboseDebug) Debug.LogFormat("Queuing save of file: {0}", savePath);

            ManageInWorker(() => { File.WriteAllBytes(savePath, bytes); });
            return savePath;
        }


        public string GetSessionPath(string experiment, string ppid, int sessionNum)
        {
            string storageLocationSafe = base.storageLocation;
            if (!System.IO.Directory.Exists(base.storageLocation))
            {
                storageLocationSafe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "UXF_Data");
                Directory.CreateDirectory(storageLocationSafe);
                Debug.LogErrorFormat("Selected storage location ({0}) does not exist! Defaulting to {1}.", base.storageLocation, storageLocationSafe);
            }
            return Path.Combine(storageLocationSafe, experiment, ppid, SessionNumToName(sessionNum));
        }


        public static string SessionNumToName(int num)
        {
            return string.Format("S{0:000}", num);
        }

        /// <summary>
        /// Aborts the FileIOManager's thread and joins the thread to the calling thread.
        /// </summary>
        public override void CleanUp()
        {
            if (verboseDebug)
                Debug.Log("Joining FileIOManagerThread");
            quitting = true;
            bq.Enqueue(doNothing); // ensures bq breaks from foreach loop
            parallelThread.Join();
        }
    }

    public enum DataSaveLocation
    {
        AcquireFromUI, Fixed
    }

}
