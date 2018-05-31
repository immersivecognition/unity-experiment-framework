#### `UXF.Session`
*The main class used to manage your experiment. Attach this to a gameobject, and it will manage your experiment "session".*
---
### Fields
`endOnDestroy`: Enable to automatically safely end the session when this object is destroyed (or the application stops running).
`blocks`: List of blocks for this experiment
`settingsToLog`: List of settings you wish to log to the behavioural file for each trial.
`customHeaders`: List of variables you plan to measure in your experiment. Once set here, you can add the observations to your results dictionary on each trial.
`trackedObjects`: List of tracked objects. Add a tracker to a GameObject in your scene and set it here to track position and rotation of the object on each Update().
`onSessionBegin`: Event(s) to trigger when the session is initialised. Can pass the instance of the Session as a dynamic argument
`onTrialBegin`: Event(s) to trigger when a trial begins. Can pass the instance of the Trial as a dynamic argument
`onTrialEnd`: Event(s) to trigger when a trial ends. Can pass the instance of the Trial as a dynamic argument
`experimentName`: Name of the experiment. Data is saved in a folder with this name.
`ppid`: Unique string for this participant (participant ID)
`number`: Current session number for this participant
`currentTrialNum`: Currently active trial number.
`currentBlockNum`: Currently active block number.
`settings`: Settings for the experiment. These are provided on initialisation of the session.
`fileIOManager`: Reference to the associated FileIOManager which deals with inputting and outputting files.
`logger`: Reference to the associated SessionLogger which automatically stores all Debug.Log calls
`participantDetails`: Dictionary of objects for datapoints collected via the UI, or otherwise.
### Properties
`hasInitialised`: Returns true if session has been intialised
`inTrial`: Returns true if current trial is in progress
`currentTrial`: Alias of GetTrial()
`nextTrial`: Alias of NextTrial()
`prevTrial`: Get the trial before the current trial.
`firstTrial`: Get the last trial in the last block of the session.
`lastTrial`: Get the last trial in the last block of the session.
`currentBlock`: Alias of GetBlock()
`trials`: Returns a list of trials for all blocks.  Modifying the order of this list will not affect trial order. Modify block.trials to change order within blocks.
`experimentPath`: Path to the folder used for reading settings and storing the output.
`ppPath`: Path within the experiment path for this particular particpant.
`path`: Path within the particpant path for this particular session.
`trackingHeaders`: List of file headers generated for all referenced tracked objects.
`headers`: Stores combined list of headers for the behavioural output.
### Methods
`AttachReferences(UXF.FileIOManager,UXF.SessionLogger)`
Provide references to other components
`InitFolder`
Folder error checks (creates folders, has set save folder, etc)
`SaveTrackerData(UXF.Tracker)`
Save tracking data for this trial
`CopyFileToSessionFolder(System.String)`
Copies a file to the folder for this session
`WriteDictToSessionFolder(System.Collections.Generic.Dictionary{System.String,System.Object},System.String)`
Write a dictionary object to a JSON file in the session folder (in a new FileIOManager thread)
`CheckSessionExists(System.String,System.String,System.String,System.Int32)`
Checks if a session folder already exists for this participant
`Begin(System.String,System.String,System.String,System.Int32,System.Collections.Generic.Dictionary{System.String,System.Object},UXF.Settings)`
Initialises a Session
`CreateBlock`
Create and return 1 Block, which then gets automatically added to Session.blocks
`CreateBlock(System.Int32)`
Create and return block containing a number of trials, which then gets automatically added to Session.blocks
`GetTrial`
Get currently active trial.
`GetTrial(System.Int32)`
Get trial by trial number (non zero indexed)
`NextTrial`
Get next Trial
`EndCurrentTrial`
Ends currently running trial. Useful to call from an inspector event
`BeginNextTrial`
Begins next trial. Useful to call from an inspector event
`PrevTrial`
Get previous Trial.
`FirstTrial`
Get first Trial in this session.
`LastTrial`
Get last Trial in this session.
`GetBlock`
Get currently active block.
`GetBlock(System.Int32)`
Get block by block number (non-zero indexed).
`End`
Ends the experiment session.
`ReadSettingsFile(System.String,System.Action{System.Collections.Generic.Dictionary{System.String,System.Object}})`
Reads json settings file as Dictionary then calls actioon with Dictionary as parameter
`SessionNumToName(System.Int32)`
Convert a session number to a session name
---
*Note: This file was automatically generated*
