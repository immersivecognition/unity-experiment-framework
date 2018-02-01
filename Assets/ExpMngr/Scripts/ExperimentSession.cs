using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Specialized;
using System.Data;
using UnityEngine.Events;

namespace ExpMngr
{
    /// <summary>
    /// The main class used to manage your experiment. Attach this to a gameobject, and it will manage your experiment "session".
    /// </summary>
    public class ExperimentSession : MonoBehaviour
    {

        /// <summary>
        /// Enable to automatically safely end the session when the application stops running.
        /// </summary>
        public bool endExperimentOnQuit = true;

        /// <summary>
        /// List of blocks for this experiment
        /// </summary>
        [HideInInspector]
        public List<Block> blocks = new List<Block>();

        [Header("Data logging")]
        // serialzed private + public getter trick allows setting in inspector without being publicly settable
        [SerializeField] private List<Tracker> _trackedObjects = new List<Tracker>();
        /// <summary>
        /// List of tracked objects. Add a tracker to a gameobject and set it here to track position and rotation of the object.
        /// </summary>
        public List<Tracker> trackedObjects { get { return _trackedObjects; } }

        [SerializeField] private List<string> _customHeaders = new List<string>();
        /// <summary>
        /// List of variables you want to measure in your experiment. Once set here, you can add the observations to your results dictionary on each trial.
        /// </summary>
        public List<string> customHeaders { get { return _customHeaders; } }

        [SerializeField] private List<string> _settingsToLog = new List<string>();
        /// <summary>
        /// List of settings you wish to log to the behavioural file for each trial.
        /// </summary>
        public List<string> settingsToLog { get { return _settingsToLog; } }

        [Header("Events")]
        /// <summary>
        /// Event(s) to trigger when the session is started via the UI
        /// </summary>
        public SessionEvent onSessionStart;

        /// <summary>
        /// Event(s) to trigger when a trial begins
        /// </summary>
        public TrialEvent onTrialBegin;

        /// <summary>
        /// Event(s) to trigger when a trial ends
        /// </summary>
        public TrialEvent onTrialEnd;

        bool hasInitialised = false;

        /// <summary>
        /// Settings for the experiment. These are automatically loaded from file on initialisation of the session.
        /// </summary>
        public Settings settings;

        /// <summary>
        /// Returns true if current trial is in progress
        /// </summary>
        public bool inTrial { get { return (trialNum != 0) && (currentTrial.status == TrialStatus.InProgress); } }

        /// <summary>
        /// Alias of GetTrial()
        /// </summary>
        public Trial currentTrial { get { return GetTrial(); } }

        /// <summary>
        /// Alias of NextTrial()
        /// </summary>
        public Trial nextTrial { get { return NextTrial(); } }

        /// <summary>
        /// Alias of PrevTrial()
        /// </summary>
        public Trial prevTrial { get { return PrevTrial(); } }

        /// <summary>
        /// Alias of LastTrial()
        /// </summary>
        public Trial lastTrial { get { return LastTrial(); } }

        /// <summary>
        /// Alias of GetBlock()
        /// </summary>
        public Block currentBlock { get { return GetBlock(); } }

        /// <summary>
        /// return trials for all blocks
        /// </summary>
        public List<Trial> trials
        {
            get
            {
                List<Trial> ts = new List<Trial>();
                foreach (Block block in blocks)
                    ts.AddRange(block.trials);
                return ts;
            }
        }

        [HideInInspector]
        public string experimentName;

        /// <summary>
        /// Unique string for this participant (participant ID)
        /// </summary>
        [HideInInspector]
        public string ppid;

        /// <summary>
        /// Current session number for this participant
        /// </summary>
        [HideInInspector]
        public int sessionNum;
        private string sessionFolderName { get { return string.Format("S{0:000}", sessionNum); } }

        /// <summary>
        /// Currently active trial number.
        /// </summary>
        [HideInInspector]
        public int trialNum = 0;

        /// <summary>
        /// Currently active block number.
        /// </summary>
        [HideInInspector]
        public int blockNum = 0;

        FileIOManager fileIOManager;

        List<string> baseHeaders = new List<string> { "ppid", "session_num", "trial_num", "block_num", "trial_num_in_block", "start_time", "end_time" };

        string basePath;

        /// <summary>
        /// Path to the folder used for reading settings and storing the output. 
        /// </summary>
        public string experimentPath { get { return Path.Combine(basePath, experimentName); } }
        /// <summary>
        /// Path within the experiment path for this particular particpant.
        /// </summary>
        public string ppPath { get { return Path.Combine(experimentPath, ppid); } }
        /// <summary>
        /// Path within the particpant path for this particular session.
        /// </summary>
        public string sessionPath { get { return Path.Combine(ppPath, sessionFolderName); } }

        /// <summary>
        /// List of file headers generated based on tracked objects.
        /// </summary>
        public List<string> trackingHeaders { get { return trackedObjects.Select(t => t.objectNameHeader).ToList(); } }
        /// <summary>
        /// Stores combined list of headers.
        /// </summary>
        [HideInInspector] public List<string> headers;

        /// <summary>
        /// Queue of actions which gets emptied on each frame in the main thread.
        /// </summary>
        public readonly Queue<System.Action> executeOnMainThreadQueue = new Queue<System.Action>();

        /// <summary>
        /// Dictionary of objects for datapoints collected via the UI.
        /// </summary>
        public Dictionary<string, object> participantDetails;

        void Awake()
        {
            // start FileIOManager
            fileIOManager = new FileIOManager(this);

            // create headers
            headers = baseHeaders.Concat(customHeaders).Concat(trackingHeaders).Concat(settingsToLog).ToList();
        }

        // Update is called once per frame
        void Update()
        {
            ManageActions();
        }

        /// <summary>
        /// Any actions which are enqueued to run on Unity's main thread
        /// </summary>
        void ManageActions()
        {
            while (executeOnMainThreadQueue.Count > 0)
            {
                executeOnMainThreadQueue.Dequeue().Invoke();
            }
        }

        internal List<Tracker> GetTrackedObjects()
        {
            return trackedObjects;
        }

        /// <summary>
        /// Folder error checks (creates folders, has set save folder, etc)     
        /// </summary>
        void InitFolder()
        {
            if (!System.IO.Directory.Exists(experimentPath))
                System.IO.Directory.CreateDirectory(experimentPath);
            if (!System.IO.Directory.Exists(ppPath))
                System.IO.Directory.CreateDirectory(ppPath);
            if (!System.IO.Directory.Exists(sessionPath))
                System.IO.Directory.CreateDirectory(sessionPath);
            else
                Debug.LogWarning("Warning session already exists! Continuing will overwrite");
        }

        /// <summary>
        /// Save tracking data for this trial
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="data"></param>
        /// <returns>Path to the file</returns>
        public string SaveTrackingData(string objectName, List<float[]> data)
        {
            string fname = string.Format("movement_{0}_T{1:000}.csv", objectName, trialNum);
            string fpath = Path.Combine(sessionPath, fname);

            fileIOManager.Manage(new System.Action(() => fileIOManager.WriteMovementData(data, fpath)));

            // return relative path so it can be saved
            Uri fullPath = new Uri(fpath);
            Uri basePath = new Uri(experimentPath);
            return basePath.MakeRelativeUri(fullPath).ToString();
        }

        /// <summary>
        /// Copies a file to the folder for this session
        /// </summary>
        /// <param name="filePath"></param>
        public void CopyFileToSessionFolder(string filePath)
        {
            string newPath = Path.Combine(sessionPath, Path.GetFileName(filePath));
            fileIOManager.Manage(new System.Action(() => fileIOManager.CopyFile(filePath, newPath)));
        }

        /// <summary>
        /// Copies a file to the folder for this session
        /// </summary>
        /// <param name="filePath">Path to the file to copy to the session folder</param>
        /// <param name="newName">New name of the file</param>
        public void CopyFileToSessionFolder(string filePath, string newName)
        {
            string newPath = Path.Combine(sessionPath, newName);
            fileIOManager.Manage(new System.Action(() => fileIOManager.CopyFile(filePath, newPath)));
        }

        /// <summary>
        /// Write a dictionary object to a JSON file in the session folder
        /// </summary>
        /// <param name="dict">Dictionary object to write</param>

        /// <param name="objectName">Name of the object (is used for file name)</param>
        public void WriteDictToSessionFolder(Dictionary<string, object> dict, string objectName)
        {
            string fileName = string.Format("{0}.json", objectName);
            string filePath = Path.Combine(sessionPath, fileName);
            fileIOManager.Manage(new System.Action(() => fileIOManager.WriteJson(filePath, dict)));
        }

        /// <summary>
        /// Checks if a session folder already exists for this participant
        /// </summary>
        /// <param name="participantId"></param>
        /// <param name="sessionNumber"></param>
        /// <param name="baseFolder"></param>
        /// <returns></returns>
        public bool CheckSessionExists(string participantId, int sessionNumber, string baseFolder)
        {
            ppid = participantId;
            sessionNum = sessionNumber;
            basePath = baseFolder;
            return System.IO.Directory.Exists(sessionPath);
        }

        /// <summary>
        /// Initialises a session with given name
        /// </summary>
        /// <param name="participantId">Unique participant id</param>
        /// <param name="sessionNumber">Session number for this particular participant</param>
        /// <param name="baseFolder">Path to the folder where data should be stored.</param>
        public void InitSession(string participantId, int sessionNumber, string baseFolder, Dictionary<string, object> participantDetails, Settings settings)
        {
            ppid = participantId;
            sessionNum = sessionNumber;
            basePath = baseFolder;
            this.participantDetails = participantDetails;
            this.settings = settings;

            // setup folders
            InitFolder();

            // copy Settings to session folder
            WriteDictToSessionFolder(settings.baseDict, "settings");

            hasInitialised = true;
            onSessionStart.Invoke(this);
        }

        /// <summary>
        /// Get currently active trial.
        /// </summary>
        /// <returns>Currently active trial.</returns>
        public Trial GetTrial()
        {
            if (trialNum == 0)
            {
                throw new NoSuchTrialException("There is no trial zero. If you are the start of the experiment please use nextTrial to get the first trial");
            }
            return trials[trialNum - 1];
        }

        /// <summary>
        /// Get trial by trial number (non zero indexed)
        /// </summary>
        /// <returns></returns>
        public Trial GetTrial(int trialNumber)
        {
            return trials[trialNumber - 1];
        }

        /// <summary>
        /// Get next Trial
        /// </summary>
        /// <returns></returns>
        public Trial NextTrial()
        {
            // non zero indexed
            try
            {
                return trials[trialNum];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new NoSuchTrialException("There is no next trial. Reached the end of trial list.");
            }
        }

        /// <summary>
        /// Begins next trial. Useful to call from an inspector event
        /// </summary>
        public void BeginNextTrial()
        {
            nextTrial.Begin();
        }

        /// <summary>
        /// Get previous Trial
        /// </summary>
        /// <returns></returns>
        public Trial PrevTrial()
        {
            // non zero indexed
            try
            {
                return trials[trialNum - 2];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new NoSuchTrialException("There is no previous trial. Probably currently at the start of experiment.");
            }
        }

        /// <summary>
        /// Get last Trial in experiment
        /// </summary>
        /// <returns></returns>
        public Trial LastTrial()
        {
            return trials[trials.Count - 1];
        }

        /// <summary>
        /// Get currently active block.
        /// </summary>
        /// <returns>Currently active block.</returns>
        public Block GetBlock()
        {
            return blocks[blockNum - 1];
        }

        /// <summary>
        /// Get block by block number (non-zero indexed).
        /// </summary>
        /// <returns>Currently active block.</returns>
        public Block GetBlock(int blockNumber)
        {
            return blocks[blockNumber - 1];
        }


        /// <summary>
        /// Ends the experiment session.
        /// </summary>
        public void EndExperiment()
        {
            if (hasInitialised)
            {
                if (inTrial)
                    currentTrial.End();
                SaveResults();
                fileIOManager.Manage(new System.Action(fileIOManager.Quit));
            }
        }

        void SaveResults()
        {
            List<OrderedResultDict> results = trials.Select(t => t.result).ToList();
            string fileName = "trial_results.csv";
            string filePath = Path.Combine(sessionPath, fileName);
            fileIOManager.Manage(new System.Action(() => fileIOManager.WriteTrials(results, filePath)));
        }

        /// <summary>
        /// Reads CSV file as DataTable then calls action with DataTable as parameter
        /// </summary>
        /// <param name="path">Path to CSV file</param>
        /// <param name="action">Action to call when completed</param>
        public void ReadCSVFile(string path, System.Action<DataTable> action)
        {
            fileIOManager.Manage(new System.Action(() => fileIOManager.ReadCSV(path, action)));
        }

        /// <summary>
        /// Writes DataTable to CSV file
        /// </summary>
        /// <param name="data">DataTable containing data to write</param>
        /// <param name="path">Path top store new CSV file</param>
        public void WriteCSVFile(DataTable data, string path)
        {
            fileIOManager.Manage(new System.Action(() => fileIOManager.WriteCSV(data, path)));
        }

        /// <summary>
        /// Reads json settings file as Dictionary then calls actioon with Dictionary as parameter
        /// </summary>
        /// <param name="path">Location of .json file to read</param>
        /// <param name="action">Action to call when completed</param>
        public void ReadSettingsFile(string path, System.Action<Dictionary<string, object>> action)
        {
            fileIOManager.Manage(new System.Action(() => fileIOManager.ReadJSON(path, action)));
        }

        void OnDestroy()
        {
            if (endExperimentOnQuit)
            {
                EndExperiment();
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


