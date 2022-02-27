using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Specialized;
using UnityEngine.Events;
using SubjectNerd.Utilities;

namespace UXF
{
    /// <summary>
    /// The Session represents a single "run" of an experiment, and contains all information about that run. 
    /// </summary>
    [ExecuteInEditMode]
    public class Session : MonoBehaviour, IExperimentUnit, IDataAssociatable
    {
        /// <summary>
        /// Enable to automatically safely end the session when the application is quitting.
        /// </summary>
        [Tooltip("Enable to automatically safely end the session when the application is quitting.")]
        public bool endOnQuit = true;

        /// <summary>
        /// Enable to automatically safely end the session when this object is destroyed.
        /// </summary>
        [Tooltip("Enable to automatically safely end the session when this object is destroyed.")]
        public bool endOnDestroy = true;

        /// <summary>
        /// Enable to automatically end the session when the final trial has ended.
        /// </summary>
        [Tooltip("Enable to automatically end the session when the final trial has ended.")]
        public bool endAfterLastTrial = false;
        
        /// <summary>
        /// If enabled, you do not need to reference this session component in a public field, you can simply call "Session.instance". This object will be destroyed if another session component is the main instance.
        /// </summary>
        [Tooltip("If enabled, you do not need to reference this session component in a public field, you can simply call \"Session.instance\". This object will be destroyed if another session component is the main instance.")]
        public bool setAsMainInstance = true;

        /// <summary>
        /// If enabled, this GameObject will not be destroyed when you load a new scene.
        /// </summary>
        [Tooltip("If enabled, this GameObject will not be destroyed when you load a new scene.")]
        public bool dontDestroyOnLoadNewScene = false;

        /// <summary>
        /// List of blocks for this experiment
        /// </summary>
        [HideInInspector]
        public List<Block> blocks = new List<Block>();

        /// <summary>
        /// Enable to save a copy of the session.settings dictionary to the session folder as a `.json` file. This is written just as the session begins.
        /// </summary>
        [Tooltip("Enable to save a copy of the session.settings dictionary to the session folder as a .json file. This is written just as the session begins.")]
        public bool storeSessionSettings = true;

        /// <summary>
        /// Enable to save a copy of the session.participantDetails dictionary to the session folder as a `.csv` file. This is written just as the session begins.
        /// </summary>
        [Tooltip("Enable to save a copy of the session.participantDetails dictionary to the session folder as a .csv file. This is written just as the session begins.")]
        public bool storeParticipantDetails = true;

        /// <summary>
        /// List of dependent variables you plan to measure in your experiment. Once set here, you can add the observations to your results dictionary on each trial.
        /// </summary>
        [Tooltip("List of dependent variables you plan to measure in your experiment. Once set here, you can add the observations to your results dictionary on each trial.")]
        [Reorderable]
        public List<string> customHeaders = new List<string>();

        /// <summary>
        /// List of settings (independent variables) you wish to log to the behavioural file for each trial.
        /// </summary>
        /// <returns></returns>
        [Tooltip("List of settings (independent variables) you wish to log to the behavioural data output for each trial.")]
        [Reorderable]
        public List<string> settingsToLog = new List<string>();

        /// <summary>
        /// List of tracked objects. Add a tracker to a GameObject in your scene and set it here to track position and rotation of the object on each Update().
        /// </summary>
        [Tooltip("List of tracked objects. Add a tracker to a GameObject in your scene and set it here to track position and rotation of the object on each Update().")]
        [Reorderable]
        public List<Tracker> trackedObjects = new List<Tracker>();

        /// <summary>
        /// Event(s) to trigger when the session is initialised. Can pass the instance of the Session as a dynamic argument
        /// </summary>
        /// <returns></returns>
        [Tooltip("Items in this event will be triggered when the session begins. Useful generating your trials & blocks, setting up the scene, and triggering the first trial.")]
        public SessionEvent onSessionBegin = new SessionEvent();

        /// <summary>
        /// Event(s) to trigger when a trial begins. Can pass the instance of the Trial as a dynamic argument
        /// </summary>
        /// <returns></returns>
        [Tooltip("Items in this event will be triggered each time a trial begins. Useful for setting up the scene according to the settings in the trial (e.g. displaying stimuli).")]
        public TrialEvent onTrialBegin = new TrialEvent();

        /// <summary>
        /// Event(s) to trigger when a trial ends. Can pass the instance of the Trial as a dynamic argument
        /// </summary>
        /// <returns></returns>
        [Tooltip("Items in this event will be triggered each time a trial ends. Useful for collecting results from the trial as well as showing feedback.")]
        public TrialEvent onTrialEnd = new TrialEvent();

        /// <summary>
        /// Event(s) to trigger just before the session has ended. If you wish to perform any summary statistics or write any final session data this is the time to do it. Do not use this event to quit the application.
        /// </summary>
        /// <returns></returns>
        [Tooltip("Items in this event will be triggered just before the session ends. Useful for performing any summary statistics, or writing any final session data. Do not use this event to quit the application, or data will be lost.")]
        public SessionEvent preSessionEnd = new SessionEvent();

        /// <summary>
        /// Event(s) to trigger when the session has ended and all jobs have finished. It is safe to quit the application beyond this event. You cannot perform data operations in this event.
        /// </summary>
        /// <returns></returns>
        [Tooltip("Items in this event will be triggered just as the session ends, after data has been written. It is safe to quit the application using this event. You should not perform manual data operations in this event.")]
        public SessionEvent onSessionEnd = new SessionEvent();

        /// <summary>
        /// Returns true when the session is in the process of ending. It is useful to query this in On Trial End events, since you may not need to perform some behaviour if the session is ending.
        /// </summary>
        /// <value></value>
        public bool isEnding { get; private set; } = false;

        [SerializeField]
        private bool _hasInitialised = false;

        /// <summary>
        /// Returns true if session has been intialised
        /// </summary>
        /// <returns></returns>
        public bool hasInitialised { get { return _hasInitialised; } }

        /// <summary>
        /// Name of the experiment. Data is saved in a folder with this name.
        /// </summary>
        public string experimentName;

        /// <summary>
        /// Unique string for this participant (participant ID)
        /// </summary>
        public string ppid;

        /// <summary>
        /// Current session number for this participant
        /// </summary>
        public int number;

        /// <summary>
        /// Currently active trial number. Be careful of modifying this.
        /// </summary>
        public int currentTrialNum = 0;

        /// <summary>
        /// Currently active block number.
        /// </summary>
        public int currentBlockNum = 0;

        /// <summary>
        /// Settings for the experiment. These are provided on initialisation of the session.
        /// </summary>
        public Settings settings { get; private set; }

        /// <summary>
        /// Returns true if current trial is in progress
        /// </summary>
        public bool InTrial { get { return (currentTrialNum != 0) && (CurrentTrial.status == TrialStatus.InProgress); } }

        /// <summary>
        /// Returns the current trial object.
        /// </summary>
        public Trial CurrentTrial { get { return GetTrial(); } }

        /// <summary>
        /// Returns the next trial object (i.e. trial with trial number currentTrialNum + 1 ).
        /// </summary>
        public Trial NextTrial { get { return GetNextTrial(); } }

        /// <summary>
        /// Get the trial before the current trial.
        /// </summary>
        public Trial PrevTrial { get { return GetPrevTrial(); } }

        /// <summary>
        /// Get the first trial in the first block of the session.
        /// </summary>
        public Trial FirstTrial { get { return GetFirstTrial(); } }

        /// <summary>
        /// Get the last trial in the last block of the session.
        /// </summary>
        public Trial LastTrial { get { return GetLastTrial(); } }

        /// <summary>
        /// Returns the current block object.
        /// </summary>
        public Block CurrentBlock { get { return GetBlock(); } }

        /// <summary>
        /// Returns a list of trials for all blocks.  Modifying the order of this list will not affect trial order. Modify block.trials to change order within blocks.
        /// </summary>
        public IEnumerable<Trial> Trials { get { return blocks.SelectMany(b => b.trials); } }
        
        /// <summary>
        /// The path in which the experiment data are stored.
        /// </summary>
        public string BasePath { get; private set; }

        /// <summary>
        /// Path to the folder used for reading settings and storing the output. 
        /// </summary>
        public string ExperimentPath { get { return Path.Combine(Path.GetFullPath(BasePath), experimentName); } }

        /// <summary>
        /// Path within the experiment path for this particular particpant.
        /// </summary>
        public string ParticipantPath { get { return Path.Combine(ExperimentPath, ppid); } }

        /// <summary>
        /// Stores combined list of headers for the behavioural output.
        /// </summary>
        public List<string> Headers { get { return baseHeaders.Concat(settingsToLog).Concat(customHeaders).Distinct().ToList(); } }

        /// <summary>
        /// Dictionary of objects for datapoints collected via the UI, or otherwise.
        /// </summary>
        public Dictionary<string, object> participantDetails;

        /// <summary>
        /// A reference to the main session instance that is currently active.
        /// </summary>
        public static Session instance;

        /// <summary>
        /// The headers that are always included in the trial_results output.
        /// </summary>
        static List<string> baseHeaders = new List<string> { "experiment", "ppid", "session_num", "trial_num", "block_num", "trial_num_in_block", "start_time", "end_time" };

        /// <summary>
        /// Reference to the associated DataHandlers which handles saving data to the cloud, etc.
        /// </summary>
        [Reorderable]
        public DataHandler[] dataHandlers = new DataHandler[]{};

        /// <summary>
        /// Get the currently selected dataHandlers for this session.
        /// </summary>
        public IEnumerable<DataHandler> ActiveDataHandlers { get { return dataHandlers.Where(d => d != null && d.active).Distinct(); }}
         
        /// <summary>
        /// Should data be saved for this session?
        /// </summary>
        public bool saveData
        {
            get => settings.GetBool(Constants.SAVE_DATA_SETTING_NAME, true);
            set => settings.SetValue(Constants.SAVE_DATA_SETTING_NAME, value);
        }

        /// <summary>
        /// Provide references to other components 
        /// </summary>
        void Awake()
        {
            if (setAsMainInstance)
            {
                if (instance != null && !ReferenceEquals(instance, this))
                {
                    DestroyImmediate(this.gameObject);
                    return;
                }
                instance = this;
            }
            if (dontDestroyOnLoadNewScene && Application.isPlaying) DontDestroyOnLoad(gameObject);            
            if (endAfterLastTrial) onTrialEnd.AddListener(EndIfLastTrial);
        }

        /// <summary>
        /// Checks if there is a risk of overwriting data for this participant and session number
        /// </summary>
        /// <param name="experimentName"></param>
        /// <param name="participantId"></param>
        /// <param name="baseFolder"></param>
        /// <param name="sessionNumber"></param>
        /// <returns></returns>
        public bool CheckSessionExists(string rootPath, string experimentName, string participantId, int sessionNumber)
        {
            foreach (var dataHandler in ActiveDataHandlers)
            {
                bool overwriteRisk = dataHandler.CheckIfRiskOfOverwrite(experimentName, participantId, sessionNumber, rootPath: rootPath);
                if (overwriteRisk) return true;
            }

            return false;
        }


        /// <summary>
        /// Initialises a Session
        /// </summary>
        /// <param name="experimentName">A name for the experiment</param>
        /// <param name="participantId">A unique ID associated with a participant</param>
        /// <param name="baseFolder">Location where data should be stored</param>
        /// <param name="sessionNumber">A number for the session (optional: default 1)</param>
        /// <param name="participantDetails">Dictionary of information about the participant to be used within the experiment (optional: default null)</param>
        /// <param name="settings">A Settings instance (optional: default empty settings)</param>
        public void Begin(string experimentName, string participantId, int sessionNumber = 1, Dictionary<string, object> participantDetails = null, Settings settings = null)
        {
            this.experimentName = experimentName;
            ppid = participantId;
            number = sessionNumber;

            if (participantDetails == null)
                participantDetails = new Dictionary<string, object>();
            this.participantDetails = participantDetails;

            if (settings == null)
                settings = Settings.empty;
            this.settings = settings;

            // Initialise DataHandlers
            foreach (var dataHandler in ActiveDataHandlers)
            {
                dataHandler.Initialise(this);
                dataHandler.SetUp();
            }
            _hasInitialised = true;

            Utilities.UXFDebugLog("Beginning session.");

            // raise the session events
            onSessionBegin.Invoke(this);
        }

        /// <summary>
        /// Create and return 1 Block, which then gets automatically added to Session.blocks  
        /// </summary>
        /// <returns></returns>
        public Block CreateBlock()
        {
            return new Block(0, this);
        }


        /// <summary>
        /// Create and return block containing a number of trials, which then gets automatically added to Session.blocks  
        /// </summary>
        /// <param name="numberOfTrials">Number of trials. Must be greater than or equal to 1.</param>
        /// <returns></returns>
        public Block CreateBlock(int numberOfTrials)
        {
            if (numberOfTrials >= 0)
                return new Block((uint)numberOfTrials, this);
            else
                throw new Exception("Invalid number of trials supplied");
        }

        /// <summary>
        /// Get currently active trial. When not in a trial, gets previous trial.
        /// </summary>
        /// <returns>Currently active trial.</returns>
        public Trial GetTrial()
        {
            if (currentTrialNum == 0)
            {
                throw new NoSuchTrialException("There is no trial zero. Did you try to perform operations on the current trial before the first one started? If you are the start of the experiment please use NextTrial to get the first trial. ");
            }
            return Trials.ToList()[currentTrialNum - 1];
        }

        /// <summary>
        /// Get trial by trial number (non zero indexed)
        /// </summary>
        /// <returns></returns>
        public Trial GetTrial(int trialNumber)
        {
            return Trials.ToList()[trialNumber - 1];
        }

        /// <summary>
        /// Get next Trial
        /// </summary>
        /// <returns></returns>
        Trial GetNextTrial()
        {
            // non zero indexed
            try
            {
                return Trials.ToList()[currentTrialNum];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new NoSuchTrialException("There is no next trial. Reached the end of trial list. If you are at the start of the session, perhaps you tried to start the next trial before generating your trials? If you are at the end, you can use BeginNextTrialSafe to do nothing if there is no next trial.");
            }
        }

        /// <summary>
        /// Ends currently running trial. Useful to call from an inspector event
        /// </summary>
        public void EndCurrentTrial()
        {
            CurrentTrial.End();
        }

        /// <summary>
        /// Begins next trial. Useful to call from an inspector event
        /// </summary>
        public void BeginNextTrial()
        {
            NextTrial.Begin();
        }

        /// <summary>
        /// Begins next trial (if one exists). Useful to call from an inspector event
        /// </summary>
        public void BeginNextTrialSafe()
        {            
            if (CurrentTrial != LastTrial)
            {
                BeginNextTrial();
            }
        }

        /// <summary>
        /// Ends the session if the supplied trial is the last trial.
        /// </summary>
        public void EndIfLastTrial(Trial trial)
        {
            if (trial == LastTrial)
            {
                End();
            }
        }

        /// <summary>
        /// Get previous Trial.
        /// </summary>
        /// <returns></returns>
        Trial GetPrevTrial()
        {
            try
            {
                // non zero indexed
                return Trials.ToList()[currentTrialNum - 2];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new NoSuchTrialException("There is no previous trial. Probably, currently at the start of session.");
            }
        }

        /// <summary>
        /// Get first Trial in this session.
        /// </summary>
        /// <returns></returns>
        Trial GetFirstTrial()
        {   
            Block firstBlock;
            try
            {
                firstBlock = blocks[0];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new NoSuchTrialException("There is no first trial because no blocks have been created!");
            }

            try
            {
                return firstBlock.trials[0];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new NoSuchTrialException("There is no first trial. No trials exist in the first block.");
            }
        }

        /// <summary>
        /// Get last Trial in this session.
        /// </summary>
        /// <returns></returns>
        Trial GetLastTrial()
        {
            if (blocks.Count == 0) throw new NoSuchTrialException("There is no last trial because no blocks have been created!");
            
            Block lastValidBlock;
            int i = blocks.Count - 1;
            while (i >= 0)
            {
                lastValidBlock = blocks[i];
                if (lastValidBlock.trials.Count > 0)
                {
                    return lastValidBlock.trials[lastValidBlock.trials.Count - 1];
                }
                i--;
            }

            throw new NoSuchTrialException("There is no last trial, blocks are present but they are all empty.");
        }

        /// <summary>
        /// Get currently active block.
        /// </summary>
        /// <returns>Currently active block.</returns>
        Block GetBlock()
        {
            return blocks[currentBlockNum - 1];
        }

        /// <summary>
        /// Get block by block number (non-zero indexed).
        /// </summary>
        /// <returns>Block.</returns>
        public Block GetBlock(int blockNumber)
        {
            return blocks[blockNumber - 1];
        }

        public bool CheckDataTypeIsValid(string dataName, UXFDataType dataType)
        {
            if (dataType.GetDataLevel() != UXFDataLevel.PerSession)
            {
                Utilities.UXFDebugLogErrorFormat(
                    "Error trying to save data '{0}' of type UXFDataType.{1} associated with the Session. The valid types for this method are {2}. Reverting to type UXFDataType.OtherSessionData.",
                    dataName,
                    dataType,
                    string.Join(", ", UXFDataLevel.PerSession.GetValidDataTypes())
                    );
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Saves a DataTable to the storage locations(s).
        /// </summary>
        /// <param name="table">The data to be saved.</param>
        /// <param name="dataName">Name to be used in saving.</param>
        /// <param name="dataType"></param>
        public void SaveDataTable(UXFDataTable table, string dataName, UXFDataType dataType = UXFDataType.OtherSessionData)
        {
            if (!CheckDataTypeIsValid(dataName, dataType)) dataType = UXFDataType.OtherSessionData;

            foreach(var dataHandler in ActiveDataHandlers)
            {
                string location = dataHandler.HandleDataTable(table, experimentName, ppid, number, dataName, dataType);
            }
        }

        /// <summary>
        /// Saves a JSON Serializable Object to the storage locations(s).
        /// </summary>
        /// <param name="serializableObject">The data to be saved.</param>
        /// <param name="dataName">Name to be used in saving.</param>
        /// <param name="dataType"></param>
        public void SaveJSONSerializableObject(List<object> serializableObject, string dataName, UXFDataType dataType = UXFDataType.OtherSessionData)
        {
            if (!CheckDataTypeIsValid(dataName, dataType)) dataType = UXFDataType.OtherSessionData;
            
            foreach(var dataHandler in ActiveDataHandlers)
            {
                string location = dataHandler.HandleJSONSerializableObject(serializableObject, experimentName, ppid, number, dataName, dataType);
            }
        }

        /// <summary>
        /// Saves a JSON Serializable Object to the storage locations(s).
        /// </summary>
        /// <param name="serializableObject">The data to be saved.</param>
        /// <param name="dataName">Name to be used in saving.</param>
        /// <param name="dataType"></param>
        public void SaveJSONSerializableObject(Dictionary<string, object> serializableObject, string dataName, UXFDataType dataType = UXFDataType.OtherSessionData)
        {
            if (!CheckDataTypeIsValid(dataName, dataType)) dataType = UXFDataType.OtherSessionData;

            foreach(var dataHandler in ActiveDataHandlers)
            {
                string location = dataHandler.HandleJSONSerializableObject(serializableObject, experimentName, ppid, number, dataName, dataType);
            }
        }

        /// <summary>
        /// Saves a string of text to the storage locations(s).
        /// </summary>
        /// <param name="text">The data to be saved.</param>
        /// <param name="dataName">Name to be used in saving.</param>
        /// <param name="dataType"></param>
        public void SaveText(string text, string dataName, UXFDataType dataType = UXFDataType.OtherSessionData)
        {
            if (!CheckDataTypeIsValid(dataName, dataType)) dataType = UXFDataType.OtherSessionData;

            foreach(var dataHandler in ActiveDataHandlers)
            {
                string location = dataHandler.HandleText(text, experimentName, ppid, number, dataName, dataType);
            }
        }

        /// <summary>
        /// Saves an array of bytes to the storage locations(s).
        /// </summary>
        /// <param name="bytes">The data to be saved.</param>
        /// <param name="dataName">Name to be used in saving.</param>
        /// <param name="dataType"></param>
        public void SaveBytes(byte[] bytes, string dataName, UXFDataType dataType = UXFDataType.OtherSessionData)
        {
            if (!CheckDataTypeIsValid(dataName, dataType)) dataType = UXFDataType.OtherSessionData;
            
            foreach(var dataHandler in ActiveDataHandlers)
            {
                string location = dataHandler.HandleBytes(bytes, experimentName, ppid, number, dataName, dataType);
            }
        }

        /// <summary>
        /// Ends the experiment session.
        /// </summary>
        public void End()
        {
            if (hasInitialised)
            {
                isEnding = true;
                if (InTrial)
                {
                    try { CurrentTrial.End(); }
                    catch (Exception e) { Debug.LogException(e); }
                }
                
                SaveResults();

                try { preSessionEnd.Invoke(this); }
                catch (Exception e) { Debug.LogException(e); }

                if (storeSessionSettings && saveData)
                {
                    // copy Settings to session folder
                    SaveJSONSerializableObject(new Dictionary<string, object>(settings.baseDict), "settings", dataType: UXFDataType.Settings);
                }

                if (storeParticipantDetails && saveData)
                {
                    // copy participant details to session folder
                    // we convert to a DataTable because we know the dictionary will be "flat" (one value per key)

                    UXFDataTable ppDetailsTable = new UXFDataTable(participantDetails.Keys.ToArray());
                    var row = new UXFDataRow();
                    foreach (var kvp in participantDetails) row.Add((kvp.Key, kvp.Value));
                    ppDetailsTable.AddCompleteRow(row);
                    var ppDetailsLines = ppDetailsTable.GetCSVLines();

                    SaveDataTable(ppDetailsTable, "participant_details", dataType: UXFDataType.ParticipantDetails);
                }

                // end DataHandlers - forces completion of tasks
                foreach (var dataHandler in ActiveDataHandlers)
                {
                    try { dataHandler.CleanUp(); }
                    catch (Exception e) { Debug.LogException(e); }
                }
                
                try { onSessionEnd.Invoke(this); }
                catch (Exception e) { Debug.LogException(e); }

                currentTrialNum = 0;
                currentBlockNum = 0;
                blocks = new List<Block>();
                _hasInitialised = false;

                Utilities.UXFDebugLog("Ended session.");
                isEnding = false;
            }
        }

        void SaveResults()
        {
            // generate list of all headers possible
            // hashset keeps unique set of keys
            HashSet<string> resultsHeaders = new HashSet<string>();
            foreach (Trial t in Trials)
                if (t.result != null && t.saveData)
                    foreach (string key in t.result.Keys)
                        resultsHeaders.Add(key);

            UXFDataTable table = new UXFDataTable(Trials.Count(), resultsHeaders.ToArray());
            foreach (Trial t in Trials)
            {
                if (t.result != null && t.saveData)
                {
                    UXFDataRow row = new UXFDataRow();
                    foreach (string h in resultsHeaders)
                    {
                        if (t.result.ContainsKey(h) && t.result[h] != null)
                        {
                            row.Add(( h, t.result[h] ));
                        }
                        else
                        {
                            row.Add(( h, string.Empty ));
                        }
                    }
                    table.AddCompleteRow(row);
                }
            }

            SaveDataTable(table, "trial_results", dataType: UXFDataType.TrialResults);            
        }

        void OnApplicationQuit()
        {
            if (endOnQuit)
            {
                End();
            }
        }

        void OnDestroy()
        {
            if (endOnDestroy)
            {
                End();
            }
        }

    }

    /// <summary>
    /// Exception thrown in cases where we try to access a trial that does not exist.
    /// </summary>
    public class NoSuchTrialException : Exception
    {
        public NoSuchTrialException()
        {
        }

        public NoSuchTrialException(string message)
            : base(message)
        {
        }

        public NoSuchTrialException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }


}


