using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Specialized;
using UnityEngine.Events;
using System.Data;


namespace ExpMngr
{
    /// <summary>
    /// Class which manages File I/O in a seperate thread to avoid hitches.
    /// </summary>
    public class FileIOManager
    {
        BlockingQueue<System.Action> bq = new BlockingQueue<System.Action>();
        Thread t;
        ExperimentSession parentExperiment;

        /// <summary>
        /// Creates FileIOmanager for an associated session
        /// </summary>
        /// <param name="experiment"></param>
        public FileIOManager(ExperimentSession experiment)
        {
            parentExperiment = experiment;
            t = new Thread(Worker);
            t.Start();
        }

        /// <summary>
        /// Adds a new command to a queue which is executed in a worker thread when it is available.
        /// </summary>
        /// <param name="command"></param>
        public void Manage(System.Action action)
        {
            bq.Enqueue(action);
        }

        void Worker()
        {
            // performs FileIO tasks in seperate thread
            foreach (var action in bq)
            {
                try
                {
                    action.Invoke();
                    //Debug.Log(string.Format("Manging command: {0}", command.function));
                    //switch (command.function)
                    //{
                    //    case FileIOFunction.CopyFile:
                    //        CopyFile(command.parameters);
                    //        break;
                    //    case FileIOFunction.WriteJson:
                    //        WriteJson(command.parameters);
                    //        break;
                    //    case FileIOFunction.WriteMovementData:
                    //        WriteMovementData(command.parameters);
                    //        break;
                    //    case FileIOFunction.WriteTrials:
                    //        WriteTrials(command.parameters);
                    //        break;
                    //    case FileIOFunction.ReadCSV:
                    //        ReadCSV(ref command.refObject, command.parameters);
                    //        break;
                    //    case FileIOFunction.WriteCSV:
                    //        WriteCSV(command.parameters);
                    //        break;
                    //    case FileIOFunction.Quit:
                    //        Quit();
                    //        break;
                    //    default:
                    //        Debug.LogError(string.Format("Unhandled command {0}", command.function));
                    //        break;
                    //}
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (IOException e)
                {
                    Debug.LogError(string.Format("Error, file may be in use! Will keep retrying. Exception: {0}", e));
                    Thread.Sleep(2000);
                    Manage(action);
                }
                catch (System.Exception e)
                {
                    // stops thread aborting
                    Debug.LogError(e);
                }
            }
        }

        public void CopyFile(string sourceFileName, string destFileName)
        {
            File.Copy(sourceFileName, destFileName);
        }

        public void WriteJson(string destFileName, object serializableObject)
        {            
            string ppJson = MiniJSON.Json.Serialize(serializableObject);
            File.WriteAllText(destFileName, ppJson);
        }

        public void WriteTrials(List<OrderedResultDict> dataDict, string fpath)
        {
            string[] csvRows = new string[dataDict.Count + 1];
            csvRows[0] = string.Join(",", parentExperiment.headers.ToArray());
            object[] row = new object[parentExperiment.headers.Count];

            for (int i = 1; i <= dataDict.Count; i++)
            {
                try
                {
                    dataDict[i - 1].Values.CopyTo(row, 0);
                    csvRows[i] = string.Join(",", row.Select(v => v.ToString()).ToArray());
                }
                catch (System.NullReferenceException)
                {
                    
                }              
                
            }

            File.WriteAllLines(fpath, csvRows);
        }

        public void WriteMovementData(List<float[]> data, string fpath)
        {
            string[] csvRows = new string[data.Count + 1];
            csvRows[0] = string.Join(",", Tracker.header);
            for (int i = 1; i <= data.Count; i++)
                csvRows[i] = string.Join(",", data[i-1].Select(f => f.ToString("0.####")).ToArray());

            File.WriteAllLines(fpath, csvRows);
        }


        public void ReadCSV(string fpath, System.Action<DataTable> callback)
        {
            // This code assumes the file is on disk, and the first row of the file
            // has the names of the columns on it

            DataTable data = null;
            try
            {
                data = CSVFile.CSV.LoadDataTable(fpath);
            }
            catch (FileNotFoundException)
            {

            }

            System.Action action = new System.Action(() => callback.Invoke(data));
            parentExperiment.executeOnMainThreadQueue.Enqueue(action);
        }

        public void WriteCSV(DataTable data, string fpath)
        {
            var writer = new CSVFile.CSVWriter(fpath);
            writer.Write(data, true);
            writer.Dispose();
        }


        public void Quit()
        {
            t.Abort();
        }

    }

}
