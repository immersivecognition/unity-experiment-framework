using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Specialized;

namespace ExpMngr
{
    /// <summary>
    /// Class which manages File I/O in a seperate thread to avoid hitches.
    /// </summary>
    public class FileIOManager
    {
        BlockingQueue<FileIOCommand> bq = new BlockingQueue<FileIOCommand>();
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
        public void Manage(FileIOCommand command)
        {
            bq.Enqueue(command);
        }

        void Worker()
        {
            // performs FileIO tasks in seperate thread
            foreach (FileIOCommand command in bq)
            {
                try
                {
                    //Debug.Log(string.Format("Manging command: {0}", command.function));
                    switch (command.function)
                    {
                        case FileIOFunction.CopyFile:
                            CopyFile(command.parameters);
                            break;
                        case FileIOFunction.WriteJson:
                            WriteJson(command.parameters);
                            break;
                        case FileIOFunction.WriteMovementData:
                            WriteMovementData(command.parameters);
                            break;
                        case FileIOFunction.WriteTrials:
                            WriteTrials(command.parameters);
                            break;
                        case FileIOFunction.Quit:
                            Quit();
                            break;
                        default:
                            Debug.LogError(string.Format("Unhandled command {0}", command.function));
                            break;
                    }
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        void CopyFile(object[] parameters)
        {
            File.Copy((string) parameters[0], (string) parameters[1]);
        }

        void WriteJson(object[] parameters)
        {            
            string ppJson = MiniJSON.Json.Serialize(parameters[1]);
            File.WriteAllText((string) parameters[0], ppJson);
        }

        void WriteTrials(object[] parameters)
        {
            List<OrderedResultDict> dataDict = (List<OrderedResultDict>) parameters[0];
            string fpath = (string) parameters[1];

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

        void WriteMovementData(object[] parameters)
        {
            List<float[]> data = (List<float[]>) parameters[0];
            string fpath = (string) parameters[1];

            string[] csvRows = new string[data.Count + 1];
            csvRows[0] = string.Join(",", Tracker.header);
            for (int i = 1; i <= data.Count; i++)
                csvRows[i] = string.Join(",", data[i-1].Select(f => f.ToString("0.####")).ToArray());

            File.WriteAllLines(fpath, csvRows);
        }

        void Quit()
        {
            t.Abort();
        }

    }

}
