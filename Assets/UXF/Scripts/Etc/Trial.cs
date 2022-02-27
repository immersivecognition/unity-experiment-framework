using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections.Specialized;


namespace UXF
{
    /// <summary>
    /// The base unit of experiments. A Trial is usually a singular attempt at a task by a participant after/during the presentation of a stimulus.
    /// </summary>
    [Serializable]
    public class Trial : IExperimentUnit, IDataAssociatable
    {

        /// <summary>
        /// Returns non-zero indexed trial number. This is generated based on its position in the block, and the ordering of the blocks within the session.
        /// </summary>
        public int number { get { return session.Trials.ToList().IndexOf(this) + 1; } }

        /// <summary>
        /// Returns non-zero indexed trial number for the current block.
        /// </summary>
        public int numberInBlock { get { return block.trials.IndexOf(this) + 1; } }

        /// <summary>
        /// Status of the trial (enum)
        /// </summary>
        public TrialStatus status = TrialStatus.NotDone;

        /// <summary>
        /// The block associated with this session
        /// </summary>
        public Block block;
        float startTime, endTime;

        /// <summary>
        /// The session associated with this trial
        /// </summary>
        /// <returns></returns>
        public Session session { get; private set; }
        
        /// <summary>
        /// Trial settings. These will override block settings if set.
        /// </summary>
        public Settings settings { get; protected set; }

        /// <summary>
        /// Should data be saved for this session?
        /// </summary>
        public bool saveData
        {
            get => settings.GetBool(Constants.SAVE_DATA_SETTING_NAME, true);
            set => settings.SetValue(Constants.SAVE_DATA_SETTING_NAME, value);
        }

        /// <summary>
        /// Dictionary of results in a order.
        /// </summary>
        public ResultsDictionary result;

        /// <summary>
        /// Manually create a trial. When doing this you need to add this trial to a block with block.trials.Add(trial)
        /// </summary>
        internal Trial(Block trialBlock)
        {
            settings = Settings.empty;
            SetReferences(trialBlock);
        }

        /// <summary>
        /// Set references for the trial.
        /// </summary>
        /// <param name="trialBlock">The block the trial belongs to.</param>
        private void SetReferences(Block trialBlock)
        {
            block = trialBlock;
            session = block.session;
            settings.SetParent(block);
        }

        /// <summary>
        /// Begins the trial, updating the current trial and block number, setting the status to in progress, starting the timer for the trial, and beginning recording positions of every object with an attached tracker
        /// </summary>
        public void Begin()
        {
            if (!session.hasInitialised)
            {
                throw new InvalidOperationException("Cannot begin trial, session is is not ready! Session has not been started yet with session.Begin() (or via the UI), or session has already ended.");
            }
            if (session.InTrial) session.CurrentTrial.End();

            session.currentTrialNum = number;
            session.currentBlockNum = block.number;

            status = TrialStatus.InProgress;
            startTime = Time.time;
            result = new ResultsDictionary(session.Headers, true);

            result["experiment"] = session.experimentName;
            result["ppid"] = session.ppid;
            result["session_num"] = session.number;
            result["trial_num"] = number;
            result["block_num"] = block.number;
            result["trial_num_in_block"] = numberInBlock;
            result["start_time"] = startTime;

            foreach (Tracker tracker in session.trackedObjects)
            {
                try
                {
                    tracker.StartRecording();
                }
                catch (NullReferenceException)
                {
                    Utilities.UXFDebugLogWarning("An item in the Tracked Objects field of the UXF session if empty (null)!");
                }
            }
            session.onTrialBegin.Invoke(this);
        }

        /// <summary>
        /// Ends the Trial, queues up saving results to output file, stops and saves tracked object data.
        /// </summary>
        public void End()
        {
            status = TrialStatus.Done;
            endTime = Time.time;
            result["end_time"] = endTime;
            
            if (saveData)
            {
                SaveData();
            }

            session.onTrialEnd.Invoke(this);
        }

        public bool CheckDataTypeIsValid(string dataName, UXFDataType dataType)
        {
            if (dataType.GetDataLevel() != UXFDataLevel.PerTrial)
            {
                Utilities.UXFDebugLogErrorFormat(
                    "Error trying to save data '{0}' of type UXFDataType.{1} associated with the Trial. The valid types for this method are {2}. Reverting to type UXFDataType.OtherTrialData.",
                    dataName,
                    dataType,
                    string.Join(", ", UXFDataLevel.PerTrial.GetValidDataTypes())
                    );
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Saves a DataTable to the storage locations(s) for this trial. A column will be added in the trial_results CSV listing the location(s) of these data.
        /// </summary>
        /// <param name="table">The data to be saved.</param>
        /// <param name="dataName">Name to be used in saving. It will be appended with the trial number.</param>
        /// <param name="dataType"></param>
        public void SaveDataTable(UXFDataTable table, string dataName, UXFDataType dataType = UXFDataType.OtherTrialData)
        {
            if (!CheckDataTypeIsValid(dataName, dataType)) dataType = UXFDataType.OtherTrialData;

            int i = 0;
            foreach(var dataHandler in session.ActiveDataHandlers)
            {
                string location = dataHandler.HandleDataTable(table, session.experimentName, session.ppid, session.number, dataName, dataType, number);
                result[string.Format("{0}_location_{1}", dataName, i++)] = location.Replace("\\", "/");
            }
        }

        /// <summary>
        /// Saves a JSON Serializable Object to the storage locations(s) for this trial. A column will be added in the trial_results CSV listing the location(s) of these data.
        /// </summary>
        /// <param name="serializableObject">The data to be saved.</param>
        /// <param name="dataName">Name to be used in saving. It will be appended with the trial number.</param>
        /// <param name="dataType"></param>
        public void SaveJSONSerializableObject(List<object> serializableObject, string dataName, UXFDataType dataType = UXFDataType.OtherTrialData)
        {
            if (!CheckDataTypeIsValid(dataName, dataType)) dataType = UXFDataType.OtherTrialData;

            int i = 0;
            foreach(var dataHandler in session.ActiveDataHandlers)
            {              
                string location = dataHandler.HandleJSONSerializableObject(serializableObject, session.experimentName, session.ppid, session.number, dataName, dataType, number);
                result[string.Format("{0}_location_{1}", dataName, i++)] = location.Replace("\\", "/");
            }
        }

        /// <summary>
        /// Saves a JSON Serializable Object to the storage locations(s) for this trial. A column will be added in the trial_results CSV listing the location(s) of these data.
        /// </summary>
        /// <param name="serializableObject">The data to be saved.</param>
        /// <param name="dataName">Name to be used in saving. It will be appended with the trial number.</param>
        /// <param name="dataType"></param>
        public void SaveJSONSerializableObject(Dictionary<string, object> serializableObject, string dataName, UXFDataType dataType = UXFDataType.OtherTrialData)
        {
            if (!CheckDataTypeIsValid(dataName, dataType)) dataType = UXFDataType.OtherTrialData;

            int i = 0;
            foreach(var dataHandler in session.ActiveDataHandlers)
            {
                string location = dataHandler.HandleJSONSerializableObject(serializableObject, session.experimentName, session.ppid, session.number, dataName, dataType, number);
                result[string.Format("{0}_location_{1}", dataName, i++)] = location.Replace("\\", "/");
            }
        }

        /// <summary>
        /// Saves a string of text to the storage locations(s) for this trial. A column will be added in the trial_results CSV listing the location(s) of these data.
        /// </summary>
        /// <param name="text">The data to be saved.</param>
        /// <param name="dataName">Name to be used in saving. It will be appended with the trial number.</param>
        /// <param name="dataType"></param>
        public void SaveText(string text, string dataName, UXFDataType dataType = UXFDataType.OtherTrialData)
        {
            if (!CheckDataTypeIsValid(dataName, dataType)) dataType = UXFDataType.OtherTrialData;
            
            int i = 0;
            foreach(var dataHandler in session.ActiveDataHandlers)
            {
                string location = dataHandler.HandleText(text, session.experimentName, session.ppid, session.number, dataName, dataType, number);
                result[string.Format("{0}_location_{1}", dataName, i++)] = location.Replace("\\", "/");
            }
        }

        /// <summary>
        /// Saves an array of bytes to the storage locations(s) for this trial. A column will be added in the trial_results CSV listing the location(s) of these data.
        /// </summary>
        /// <param name="bytes">The data to be saved.</param>
        /// <param name="dataName">Name to be used in saving. It will be appended with the trial number.</param>
        /// <param name="dataType"></param>
        public void SaveBytes(byte[] bytes, string dataName, UXFDataType dataType = UXFDataType.OtherTrialData)
        {            
            if (!CheckDataTypeIsValid(dataName, dataType)) dataType = UXFDataType.OtherTrialData;
            
            int i = 0;
            foreach(var dataHandler in session.ActiveDataHandlers)
            {
                string location = dataHandler.HandleBytes(bytes, session.experimentName, session.ppid, session.number, dataName, dataType, number);
                result[string.Format("{0}_location_{1}", dataName, i++)] = location.Replace("\\", "/");
            }
        }

        private void SaveData()
        {
            // check no duplicate trackers
            List<string> duplicateTrackers = session.trackedObjects.Where(tracker => tracker != null)
              .GroupBy(tracker => tracker.DataName)
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList();

            if (duplicateTrackers.Any()) throw new InvalidOperationException(string.Format("Two or more trackers in the Tracked Objects field in the Session Inspector have the following object name and descriptor pair, please change the object name fields on the trackers to make them unique: {0}", string.Join(",", duplicateTrackers)));

            // log tracked objects
            foreach (Tracker tracker in session.trackedObjects)
            {
                try
                {
                    tracker.StopRecording();
                    SaveDataTable(tracker.Data, tracker.DataName, dataType: UXFDataType.Trackers);
                }
                catch (NullReferenceException)
                {
                    Utilities.UXFDebugLogWarning("An item in the Tracked Objects field of the UXF session if empty (null)!");
                }
            }

            // log any settings we need to for this trial
            foreach (string s in session.settingsToLog)
            {
                result[s] = settings.GetObject(s, string.Empty);
            }
        }
    }

    

    /// <summary>
    /// Status of a trial
    /// </summary>
    public enum TrialStatus
    {
        NotDone,
        InProgress,
        Done
    }


}