#### `UXF.FileIOManager`
*Component which manages File I/O in a seperate thread to avoid hitches.*
---
### Fields
`debug`: Enable to print debug messages to the console.
`executeOnMainThreadQueue`: Queue of actions which gets emptied on each frame in the main thread.
`doNothing`: An action which does nothing. Useful if a method requires an Action
### Properties
*None*### Methods
`Begin`
Starts the FileIOManager Worker thread.
`ManageInWorker(System.Action)`
Adds a new command to a queue which is executed in a separate worker thread when it is available.
            Warning: The Unity Engine API is not thread safe, so do not attempt to put any Unity commands here.
`CopyFile(System.String,System.String)`
Copy file from one place to another.
`ReadJSON(System.String,System.Action{System.Collections.Generic.Dictionary{System.String,System.Object}})`
Reads a JSON file from a path then calls a given action with the deserialzed object as the first argument
`WriteJson(System.String,System.Object)`
Serializes an object using MiniJSON and writes to a given path
`WriteTrials(System.Collections.Generic.List{OrderedResultDict},System.String[],System.String)`
Writes trial data (List of OrderedResultsDict) to file at fpath
`WriteCSV(System.String[],System.Collections.Generic.IList{System.String[]},System.String)`
Writes a list of string arrays with a given header to a file at given path.
`ReadCSV(System.String,System.Action{System.Data.DataTable})`
Read a CSV file from path, then runs an action that takes a DataTable as an argument. This code assumes the file is on disk, and the first row of the file has the names of the columns on it. Returns null if not found
`WriteCSV(System.Data.DataTable,System.String)`
Writes a DataTable to file to a path.
`ManageInMain`
Handles any actions which are enqueued to run on Unity's main thread.
`End`
Aborts the FileIOManager's thread.
---
*Note: This file was automatically generated*
