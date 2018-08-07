using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Specialized;
using System.Data;


namespace UXF
{
    /// <summary>
    /// Component which manages File I/O in a seperate thread to avoid hitches.
    /// </summary>
    public class FileIOManager : MonoBehaviour
    {
        /// <summary>
        /// Enable to print debug messages to the console.
        /// </summary>
        [Tooltip("Enable to print debug messages to the console.")]
        public bool debug = false;

        /// <summary>
        /// Event(s) to trigger when we write a file. Not performed in amin thread so cannot include most Unity actions.
        /// </summary>
        /// <returns></returns>
        [Tooltip("Event(s) to trigger when we write a file. Not performed in amin thread so cannot include most Unity actions.")]
        public WriteFileEvent onWriteFile = new WriteFileEvent();

        /// <summary>
        /// Queue of actions which gets emptied on each frame in the main thread.
        /// </summary>
        public readonly Queue<System.Action> executeOnMainThreadQueue = new Queue<System.Action>();

        /// <summary>
        /// An action which does nothing. Useful if a method requires an Action
        /// </summary>
        /// <returns></returns>
        public static System.Action doNothing = () => { };

        public bool IsActive { get { return t != null && t.IsAlive; } }

        BlockingQueue<System.Action> bq = new BlockingQueue<System.Action>();
        Thread t;

        bool quitting = false;


        void Awake()
        {   
            if (!IsActive) Begin();
        }

        /// <summary>
        /// Starts the FileIOManager Worker thread.
        /// </summary>
        public void Begin()
        {
            if (IsActive)
                throw new ThreadStateException("Cannot Begin. FileIOManager thread has already been started!");

            quitting = false;
            t = new Thread(Worker);
            t.Start();
        }

        void Update()
        {
            ManageInMain();
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
            if (debug)
                Debug.Log("Started worker thread");

            // performs FileIO tasks in seperate thread
            foreach (var action in bq)
            {

                if (debug && action != null)
                    Debug.LogFormat("Managing action: {0}.{1}", action.Method.ReflectedType.FullName, action.Method.Name);

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

            if (debug)
                Debug.Log("Finished worker thread");
        }

        /// <summary>
        /// Copy file from one place to another.
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>
        public void CopyFile(string sourceFileName, string destFileName)
        {
            File.Copy(sourceFileName, destFileName);
        }

        /// <summary>
        /// Reads a JSON file from a path then calls a given action with the deserialzed object as the first argument 
        /// </summary>
        /// <param name="fpath"></param>
        /// <param name="callback"></param>
        public void ReadJSON(string fpath, System.Action<Dictionary<string, object>> callback)
        {
            Dictionary<string, object> dict = null;
            try
            {
                string dataAsJson = File.ReadAllText(fpath);
                dict = MiniJSON.Json.Deserialize(dataAsJson) as Dictionary<string, object>;
            }
            catch (FileNotFoundException)
            {
                string message = string.Format(".json file not found in {0}!", fpath);
                Debug.LogWarning(message);
            }

            System.Action action = new System.Action(() => callback.Invoke(dict));
            executeOnMainThreadQueue.Enqueue(action);
        }

        /// <summary>
        /// Serializes an object using MiniJSON and writes to a given path
        /// </summary>
        /// <param name="destFileName"></param>
        /// <param name="serializableObject"></param>
        public void WriteJson(object serializableObject, WriteFileInfo writeFileInfo)
        {
            string ppJson = MiniJSON.Json.Serialize(serializableObject);
            File.WriteAllText(writeFileInfo.FullPath, ppJson);
            executeOnMainThreadQueue.Enqueue(() => onWriteFile.Invoke(writeFileInfo));
        }

        /// <summary>
        /// Writes trial data (List of OrderedResultsDict) to file at fpath
        /// </summary>
        /// <param name="dataDict"></param>
        /// <param name="headers"></param>
        /// <param name="fpath"></param>
        public void WriteTrials(List<OrderedResultDict> dataDict, string[] headers, WriteFileInfo writeFileInfo)
        {
            string[] csvRows = new string[dataDict.Count + 1];
            csvRows[0] = string.Join(",", headers.ToArray());
            object[] row = new object[headers.Length];

            for (int i = 1; i <= dataDict.Count; i++)
            {
                OrderedResultDict dict = dataDict[i - 1];
                if (dict != null)
                {
                    dict.Values.CopyTo(row, 0);
                    csvRows[i] = string.Join(",", row.Select(v => System.Convert.ToString(v)).ToArray());
                }
            }

            File.WriteAllLines(writeFileInfo.FullPath, csvRows);
            executeOnMainThreadQueue.Enqueue(() => onWriteFile.Invoke(writeFileInfo));
        }

        /// <summary>
        /// Writes a list of string arrays with a given header to a file at given path.
        /// </summary>
        /// <param name="header">Row of headers</param>
        /// <param name="data"></param>
        /// <param name="fpath"></param>
        public void WriteCSV(string[] header, IList<string[]> data, WriteFileInfo writeFileInfo)
        {
            string[] csvRows = new string[data.Count + 1];
            csvRows[0] = string.Join(",", header);
            for (int i = 1; i <= data.Count; i++)
                csvRows[i] = string.Join(",", data[i - 1]);

            File.WriteAllLines(writeFileInfo.FullPath, csvRows);
            executeOnMainThreadQueue.Enqueue(() => onWriteFile.Invoke(writeFileInfo));
        }

        /// <summary>
        /// Read a CSV file from path, then runs an action that takes a DataTable as an argument. This code assumes the file is on disk, and the first row of the file has the names of the columns on it. Returns null if not found
        /// </summary>
        /// <param name="fpath"></param>
        /// <param name="callback"></param>
        public void ReadCSV(string fpath, System.Action<DataTable> callback)
        {
            DataTable data = null;
            try
            {
                data = CSVFile.CSV.LoadDataTable(fpath);
            }
            catch (FileNotFoundException)
            {
                Debug.LogErrorFormat("Cannot find file {0}", fpath);
            }

            System.Action action = new System.Action(() => callback.Invoke(data));
            executeOnMainThreadQueue.Enqueue(action);
        }

        /// <summary>
        /// Writes a DataTable to file to a path.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fpath"></param>
        public void WriteCSV(DataTable data, WriteFileInfo writeFileInfo)
        {
            var writer = new CSVFile.CSVWriter(writeFileInfo.FullPath);
            writer.Write(data, true);
            writer.Dispose();
            executeOnMainThreadQueue.Enqueue(() => onWriteFile.Invoke(writeFileInfo));
        }

        /// <summary>
        /// Handles any actions which are enqueued to run on Unity's main thread.
        /// </summary>
        public void ManageInMain()
        {
            while (executeOnMainThreadQueue.Count > 0)
            {
                executeOnMainThreadQueue.Dequeue().Invoke();
            }
        }

        void OnDestroy()
        {
            End();
        }

        /// <summary>
        /// Aborts the FileIOManager's thread and joins the thread to the calling thread.
        /// </summary>
        public void End()
        {
            if (debug)
                Debug.Log("Joining FileIOManagerThread");
            quitting = true;
            bq.Enqueue(doNothing); // ensures bq breaks from foreach loop
            t.Join();
            ManageInMain(); // empties main thread queue
        }

    }

}
