using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// This is an example custom data handler. It implements the required DataHandler methods and just prints out data to the console. 
/// You can copy this script, but do some other action instead of printing out the data.
/// You can attach this script to a GameObject then reference it in the Data Handling tab of the UXF Session component.

namespace UXF.Examples
{
    public class ExampleCustomDataHandler : DataHandler
    {

        public override bool CheckIfRiskOfOverwrite(string experiment, string ppid, int sessionNum, string rootPath = "")
        {
            return false; // You could write a condition to return true when a ppid already exists.
        }

        public override void SetUp()
        {
            // No setup is needed in this case. But you can add any code here that will be run when the session begins.
        }

        public override string HandleDataTable(UXFDataTable table, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            Debug.LogFormat("Handling a data table for session: {0}, {1}, {2}", experiment, ppid, sessionNum);
            Debug.LogFormat("The data name is: {0}, and the type is: ", dataName, dataType);
            
            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial)
                Debug.LogFormat("Data is per-trial, trial number is {0}.", optionalTrialNum);
            else
                Debug.Log("Data is per-session.");

            // get data as text
            string[] lines = table.GetCSVLines();
            string text = string.Join("\n", lines);

            // here we "write" our data, you could upload to database, or do whatever.
            Debug.Log(text);

            // return a string representing the location of the data. Will be stored in the trial_results output.
            return "Data printed to console."; 
        }

        public override string HandleJSONSerializableObject(List<object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            Debug.LogFormat("Handling a JSON Serializale Object for session: {0}, {1}, {2}", experiment, ppid, sessionNum);
            Debug.LogFormat("The data name is: {0}, and the type is: ", dataName, dataType);
            
            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial)
                Debug.LogFormat("Data is per-trial, trial number is {0}.", optionalTrialNum);
            else
                Debug.Log("Data is per-session.");

            // get data as text
            string text = MiniJSON.Json.Serialize(serializableObject);

            // here we "write" our data, you could upload to database, or do whatever.
            Debug.Log(text); 

            // return a string representing the location of the data. Will be stored in the trial_results output.
            return "Data printed to console."; 
        }

        public override string HandleJSONSerializableObject(Dictionary<string, object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            Debug.LogFormat("Handling a JSON Serializale Object for session: {0}, {1}, {2}", experiment, ppid, sessionNum);
            Debug.LogFormat("The data name is: {0}, and the type is: ", dataName, dataType);
            
            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial)
                Debug.LogFormat("Data is per-trial, trial number is {0}.", optionalTrialNum);
            else
                Debug.Log("Data is per-session.");

            // get data as text
            string text = MiniJSON.Json.Serialize(serializableObject);

            // here we "write" our data, you could upload to database, or do whatever.
            Debug.Log(text); 

            // return a string representing the location of the data. Will be stored in the trial_results output.
            return "Data printed to console."; 
        }

        public override string HandleText(string text, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            Debug.LogFormat("Handling a JSON Serializale Object for session: {0}, {1}, {2}", experiment, ppid, sessionNum);
            Debug.LogFormat("The data name is: {0}, and the type is: ", dataName, dataType);
            
            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial)
                Debug.LogFormat("Data is per-trial, trial number is {0}.", optionalTrialNum);
            else
                Debug.Log("Data is per-session.");

            // here we "write" our data, you could upload to database, or do whatever.
            Debug.Log(text); 

            // return a string representing the location of the data. Will be stored in the trial_results output.
            return "Data printed to console."; 
        }

        public override string HandleBytes(byte[] bytes, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            Debug.LogFormat("Handling a JSON Serializale Object for session: {0}, {1}, {2}", experiment, ppid, sessionNum);
            Debug.LogFormat("The data name is: {0}, and the type is: ", dataName, dataType);
            
            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial)
                Debug.LogFormat("Data is per-trial, trial number is {0}.", optionalTrialNum);
            else
                Debug.Log("Data is per-session.");

            // here we "write" our data, you could upload to database, or do whatever.
            string text = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log(text); 

            // return a string representing the location of the data. Will be stored in the trial_results output.
            return "Data printed to console."; 
        }

        public override void CleanUp()
        {
            // No cleanup is needed in this case. But you can add any code here that will be run when the session finishes.
        }
        
    }

}
