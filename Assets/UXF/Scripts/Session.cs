using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Specialized;
using System.Data;
using UnityEngine.Events;

namespace UXF
{
    /// <summary>
    /// The Session represents a single "run" of an experiment, and contains all information about that run. 
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(FileIOManager))]
    public class Session : MonoBehaviour
    {
        /// <summary>
        /// Enable to automatically safely end the session when this object is destroyed (or the application stops running).
        /// </summary>
        [Tooltip("Enable to automatically safely end the session when this object is destroyed (or the application stops running).")]
        public bool endOnDestroy = true;
        
        /// <summary>
        /// List of blocks for this experiment
        /// </summary>
        [HideInInspector]
        public List<Block> blocks = new List<Block>();

        /// <summary>
        /// List of settings you wish to log to the behavioural file for each trial.
        /// </summary>
        /// <returns></returns>
        [Header("Data logging")]
        [Tooltip("List of settings you wish to log to the behavioural data output for each trial.")]
        public List<string> settingsToLog = new List<string>();

        /// <summary>
        /// List of variables you plan to measure in your experiment. Once set here, you can add the observations to your results dictionary on each trial.
        /// </summary>
        [Tooltip("List of variables you plan to measure in your experiment. Once set here, you can add the observations to your results dictionary on each trial.")]
        public List<string> customHeaders = new List<string>();

        /// <summary>
        /// List of tracked objects. Add a tracker to a GameObject in your scene and set it here to track position and rotation of the object on each Update().
        /// </summary>
        [Tooltip("List of tracked objects. Add a tracker to a GameObject in your scene and set it here to track position and rotation of the object on each Update().")]
        public List<Tracker> trackedObjects = new List<Tracker>();

        /// <summary>
        /// Event(s) to trigger when the session is initialised. Can pass the instance of the Session as a dynamic argument
        /// </summary>
        /// <returns></returns>
        [Header("Events")]
        [Tooltip("Event(s) to trigger when the session is initialised. Can pass the instance of the Session as a dynamic argument")]
        public SessionEvent onSessionBegin = new SessionEvent();

        /// <summary>
        /// Event(s) to trigger when a trial begins. Can pass the instance of the Trial as a dynamic argument
        /// </summary>
        /// <returns></returns>
        [Tooltip("Event(s) to trigger when a trial begins. Can pass the instance of the Trial as a dynamic argument")]
        public TrialEvent onTrialBegin = new TrialEvent();

        /// <summary>
        /// Event(s) to trigger when a trial ends. Can pass the instance of the Trial as a dynamic argument
        /// </summary>
        /// <returns></returns>
        [Tooltip("Event(s) to trigger when a trial ends. Can pass the instance of the Trial as a dynamic argument")]
        public TrialEvent onTrialEnd = new TrialEvent();

        [Header("Session information")]

        [ReadOnly]
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
        [ReadOnly]
        public string experimentName;

        /// <summary>
        /// Unique string for this participant (participant ID)
        /// </summary>
        [ReadOnly]
        public string ppid;

        /// <summary>
        /// Current session number for this participant
        /// </summary>
        [ReadOnly]
        public int number;

        /// <summary>
        /// Currently active trial number.
        /// </summary>
        [ReadOnly]
        public int currentTrialNum = 0;

        /// <summary>
        /// Currently active block number.
        /// </summary>
        [ReadOnly]
        public int currentBlockNum = 0;

        /// <summary>
        /// Settings for the experiment. These are provided on initialisation of the session.
        /// </summary>
        public Settings settings;

        /// <summary>
        /// Returns true if current trial is in progress
        /// </summary>
        public bool inTrial { get { return (currentTrialNum != 0) && (currentTrial.status == TrialStatus.InProgress); } }

        /// <summary>
        /// Alias of GetTrial()
        /// </summary>
        public Trial currentTrial { get { return GetTrial(); } }

        /// <summary>
        /// Alias of NextTrial()
        /// </summary>
        public Trial nextTrial { get { return NextTrial(); } }

        /// <summary>
        /// Get the trial before the current trial.
        /// </summary>
        public Trial prevTrial { get { return PrevTrial(); } }

        /// <summary>
        /// Get the last trial in the last block of the session.
        /// </summary>
        public Trial firstTrial { get { return FirstTrial(); } }

        /// <summary>
        /// Get the last trial in the last block of the session.
        /// </summary>
        public Trial lastTrial { get { return LastTrial(); } }

        /// <summary>
        /// Alias of GetBlock()
        /// </summary>
        public Block currentBlock { get { return GetBlock(); } }

        /// <summary>
        /// Returns a list of trials for all blocks.  Modifying the order of this list will not affect trial order. Modify block.trials to change order within blocks.
        ///  
        /// </summary>
        public IEnumerable<Trial> trials { get { return blocks.SelectMany(b => b.trials); } }

        /// <summary>
        /// Reference to the associated FileIOManager which deals with inputting and outputting files.
        /// </summary>
        private FileIOManager fileIOManager;

        /// <summary>
        /// Reference to the associated SessionLogger which automatically stores all Debug.Log calls
        /// </summary>
        private SessionLogger logger;

        List<string> baseHeaders = new List<string> { "experiment", "ppid", "session_num", "trial_num", "block_num", "trial_num_in_block", "start_time", "end_time" };

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
        public string path { get { return Path.Combine(ppPath, folderName); } }

        private string folderName { get { return SessionNumToName(number); } }


        /// <summary>
        /// List of file headers generated for all referenced tracked objects.
        /// </summary>
        public List<string> trackingHeaders { get { return trackedObjects.Select(t => t.pathHeader).ToList(); } }

        /// <summary>
        /// Stores combined list of headers for the behavioural output.
        /// </summary>
        public List<string> headers { get { return baseHeaders.Concat(settingsToLog).Concat(customHeaders).Concat(trackingHeaders).ToList(); }}

        /// <summary>
        /// Dictionary of objects for datapoints collected via the UI, or otherwise.
        /// </summary>
        public Dictionary<string, object> participantDetails;

        void Awake()
        {
            // get components attached to this gameobject and store their references 
            AttachReferences(
                GetComponent<FileIOManager>(),
                GetComponent<SessionLogger>()
                );
        }

        /// <summary>
        /// Provide references to other components 
        /// </summary>
        /// <param name="newFileIOManager"></param>
        /// <param name="newSessionLogger"></param>
        public void AttachReferences(FileIOManager newFileIOManager = null, SessionLogger newSessionLogger = null)
        {
            if (newFileIOManager != null) fileIOManager = newFileIOManager;
            if (newSessionLogger != null) logger = newSessionLogger;
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
            if (System.IO.Directory.Exists(path))
                Debug.LogWarning("Warning: Session already exists! Continuing will overwrite"); 
            else
                System.IO.Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Save tracking data for this trial
        /// </summary>
        /// <param name="tracker">The tracker to take data from to save</param>
        /// <returns>Path to the saved file</returns>
        public string SaveTrackerData(Tracker tracker)
        {
            string fname = string.Format("{0}_{1}_T{2:000}.csv", tracker.objectName, tracker.measurementDescriptor, currentTrialNum);
            string fpath = Path.Combine(path, fname);

            fileIOManager.ManageInWorker(() => fileIOManager.WriteCSV(tracker.header, tracker.GetDataCopy(), fpath));

            // return relative path so it can be stored in behavioural data
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
            string newPath = Path.Combine(path, Path.GetFileName(filePath));
            fileIOManager.ManageInWorker(() => fileIOManager.CopyFile(filePath, newPath));
        }

        /// <summary>
        /// Write a dictionary object to a JSON file in the session folder (in a new FileIOManager thread)
        /// </summary>
        /// <param name="dict">Dictionary object to write</param>

        /// <param name="objectName">Name of the object (is used for file name)</param>
        void WriteDictToSessionFolder(Dictionary<string, object> dict, string objectName)
        {
            string fileName = string.Format("{0}.json", objectName);
            string filePath = Path.Combine(path, fileName);
            fileIOManager.ManageInWorker(() => fileIOManager.WriteJson(filePath, dict));
        }


        /// <summary>
        /// Checks if a session folder already exists for this participant
        /// </summary>
        /// <param name="experimentName"></param>
        /// <param name="participantId"></param>
        /// <param name="baseFolder"></param>
        /// <param name="sessionNumber"></param>
        /// <returns></returns>
        public static bool CheckSessionExists(string experimentName, string participantId, string baseFolder, int sessionNumber)
        {
            string potentialPath = Extensions.CombinePaths(baseFolder, experimentName, participantId, SessionNumToName(sessionNumber));
            return System.IO.Directory.Exists(potentialPath);
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
        public void Begin(string experimentName, string participantId, string baseFolder, int sessionNumber = 1, Dictionary<string, object> participantDetails = null, Settings settings = null)
        {
            baseFolder = Path.IsPathRooted(baseFolder) ? baseFolder : Path.Combine(Directory.GetCurrentDirectory(), baseFolder);

            if (!Directory.Exists(baseFolder))
                throw new DirectoryNotFoundException(string.Format("Initialising session failed, cannot find {0}", baseFolder));

            this.experimentName = experimentName;
            ppid = participantId;
            number = sessionNumber;
            basePath = baseFolder;
            this.participantDetails = participantDetails;

            if (settings == null)
                settings = Settings.empty;
            else
                this.settings = settings;

            // setup folders
            InitFolder();

            // Initialise logger
            if (logger != null)
                logger.Initialise();

            _hasInitialised = true;
            onSessionBegin.Invoke(this);

            // copy Settings to session folder
            
            WriteDictToSessionFolder(
                new Dictionary<string, object>(settings.baseDict), // makes a copy
                "settings");
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
            if (numberOfTrials > 0)
                return new Block((uint) numberOfTrials, this);
            else
                throw new Exception("Invalid number of trials supplied");
        }

        /// <summary>
        /// Get currently active trial.
        /// </summary>
        /// <returns>Currently active trial.</returns>
        public Trial GetTrial()
        {
            if (currentTrialNum == 0)
            {
                throw new NoSuchTrialException("There is no trial zero. If you are the start of the experiment please use nextTrial to get the first trial");
            }
            return trials.ToList()[currentTrialNum - 1];
        }

        /// <summary>
        /// Get trial by trial number (non zero indexed)
        /// </summary>
        /// <returns></returns>
        public Trial GetTrial(int trialNumber)
        {
            return trials.ToList()[trialNumber - 1];
        }

        /// <summary>
        /// Get next Trial
        /// </summary>
        /// <returns></returns>
        Trial NextTrial()
        {
            // non zero indexed
            try
            {
                return trials.ToList()[currentTrialNum];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new NoSuchTrialException("There is no next trial. Reached the end of trial list.");
            }
        }

        /// <summary>
        /// Ends currently running trial. Useful to call from an inspector event
        /// </summary>
        public void EndCurrentTrial()
        {
            currentTrial.End();
        }

        /// <summary>
        /// Begins next trial. Useful to call from an inspector event
        /// </summary>
        public void BeginNextTrial()
        {
            if (hasInitialised)
                nextTrial.Begin();
        }

        /// <summary>
        /// Get previous Trial.
        /// </summary>
        /// <returns></returns>
        Trial PrevTrial()
        {
            try
            { 
                // non zero indexed
                return trials.ToList()[currentTrialNum - 2];
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
        Trial FirstTrial()
        {
            var firstBlock = blocks[0];
            return firstBlock.trials[0];
        }

        /// <summary>
        /// Get last Trial in this session.
        /// </summary>
        /// <returns></returns>
        Trial LastTrial()
        {
            var lastBlock = blocks[blocks.Count- 1];
            return lastBlock.trials[lastBlock.trials.Count - 1];
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


        /// <summary>
        /// Ends the experiment session.
        /// </summary>
        public void End()
        {
            if (hasInitialised)
            {
                if (inTrial)
                    currentTrial.End();
                SaveResults();

                // end logger
                if (logger != null)
                    logger.Finalise();
                
                blocks = new List<Block>();
                _hasInitialised = false;
            }
        }

        void SaveResults()
        {
            List<OrderedResultDict> results = trials.Select(t => t.result).ToList();
            string fileName = "trial_results.csv";
            string filePath = Path.Combine(path, fileName);

            // in this case, write in main thread to block aborting
            fileIOManager.ManageInWorker(() => fileIOManager.WriteTrials(results, headers.ToArray(), filePath));
        }


        /// <summary>
        /// Reads json settings file as Dictionary then calls actioon with Dictionary as parameter
        /// </summary>
        /// <param name="path">Location of .json file to read</param>
        /// <param name="action">Action to call when completed</param>
        public void ReadSettingsFile(string path, System.Action<Dictionary<string, object>> action)
        {
            fileIOManager.ManageInWorker(() => fileIOManager.ReadJSON(path, action));
        }

        void OnDestroy()
        {
            if (endOnDestroy)
            {
                End();
            }
        }
        
        /// <summary>
        /// Convert a session number to a session name
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string SessionNumToName(int num)
        {
            return string.Format("S{0:000}", num);
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


