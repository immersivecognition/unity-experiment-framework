using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;


namespace UXF
{
    public class HTTPPost : DataHandler
    {

        [Tooltip("Key used in the form for a desired filepath.")]
        public string filepathKey = "filepath";

        [Tooltip("Key used in the form for the data.")]
        public string dataKey = "data";

        [Tooltip("URL of the form to POST to.")]
        public string url = "http://127.0.0.1:5000/";

        [Space]
        [Tooltip("Enable to use username and password to add Basic HTTP Authentication to the request.")]
        public bool httpBasicAuthentication = true;
        [BasteRainGames.HideIf("httpBasicAuthentication", false)]
        public string username = "susan";
        [BasteRainGames.HideIf("httpBasicAuthentication", false)]
        public string password = "password";


        public override bool CheckIfRiskOfOverwrite(string experiment, string ppid, int sessionNum, string rootPath = "")
        {
            return false;
        }

        public override string HandleDataTable(UXFDataTable table, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            // get data as text
            string[] lines = table.GetCSVLines();
            string text = string.Join("\n", lines);

            string ext  = Path.GetExtension(dataName);
            dataName = Path.GetFileNameWithoutExtension(dataName);

            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial) dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string directory = GetSessionPathRelative(experiment, ppid, sessionNum);
            if (dataType != UXFDataType.TrialResults) directory = Path.Combine(directory, dataType.ToLower());

            string name = string.IsNullOrEmpty(ext) ? string.Format("{0}.csv", dataName) : string.Format("{0}{1}", dataName, ext);
            string savePath = Path.Combine(directory, name);
            savePath = savePath.Replace('\\', '/');

            // here we send our data request
            AuthenticatedRequest(savePath, text);

            // return a string representing the location of the data. Will be stored in the trial_results output.
            return savePath; 
        }

        public override string HandleJSONSerializableObject(List<object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            // get data as text
            string text = MiniJSON.Json.Serialize(serializableObject);

            string ext  = Path.GetExtension(dataName);
            dataName = Path.GetFileNameWithoutExtension(dataName);

            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial) dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string directory = GetSessionPathRelative(experiment, ppid, sessionNum);
            if (dataType != UXFDataType.TrialResults) directory = Path.Combine(directory, dataType.ToLower());

            string name = string.IsNullOrEmpty(ext) ? string.Format("{0}.json", dataName) : string.Format("{0}{1}", dataName, ext);
            string savePath = Path.Combine(directory, name);
            savePath = savePath.Replace('\\', '/');

            // here we send our data request
            AuthenticatedRequest(savePath, text);

            // return a string representing the location of the data. Will be stored in the trial_results output.
            return savePath; 
        }

        public override string HandleJSONSerializableObject(Dictionary<string, object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            // get data as text
            string text = MiniJSON.Json.Serialize(serializableObject);

            string ext  = Path.GetExtension(dataName);
            dataName = Path.GetFileNameWithoutExtension(dataName);

            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial) dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string directory = GetSessionPathRelative(experiment, ppid, sessionNum);
            if (dataType != UXFDataType.TrialResults) directory = Path.Combine(directory, dataType.ToLower());

            string name = string.IsNullOrEmpty(ext) ? string.Format("{0}.json", dataName) : string.Format("{0}{1}", dataName, ext);
            string savePath = Path.Combine(directory, name);
            savePath = savePath.Replace('\\', '/');

            // here we send our data request
            AuthenticatedRequest(savePath, text);

            // return a string representing the location of the data. Will be stored in the trial_results output.
            return savePath; 
        }

        public override string HandleText(string text, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            string ext  = Path.GetExtension(dataName);
            dataName = Path.GetFileNameWithoutExtension(dataName);

            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial) dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string directory = GetSessionPathRelative(experiment, ppid, sessionNum);
            if (dataType != UXFDataType.TrialResults) directory = Path.Combine(directory, dataType.ToLower());

            string name = string.IsNullOrEmpty(ext) ? string.Format("{0}.txt", dataName) : string.Format("{0}{1}", dataName, ext);
            string savePath = Path.Combine(directory, name);
            savePath = savePath.Replace('\\', '/');

            // here we send our data request
            AuthenticatedRequest(savePath, text);

            // return a string representing the location of the data. Will be stored in the trial_results output.
            return savePath; 
        }

        public override string HandleBytes(byte[] bytes, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            // get data as text
            string text = System.Text.Encoding.UTF8.GetString(bytes);
            
            string ext  = Path.GetExtension(dataName);
            dataName = Path.GetFileNameWithoutExtension(dataName);

            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial) dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string directory = GetSessionPathRelative(experiment, ppid, sessionNum);
            if (dataType != UXFDataType.TrialResults) directory = Path.Combine(directory, dataType.ToLower());

            string name = string.IsNullOrEmpty(ext) ? string.Format("{0}.txt", dataName) : string.Format("{0}{1}", dataName, ext);
            string savePath = Path.Combine(directory, name);
            savePath = savePath.Replace('\\', '/');

            // here we send our data request
            AuthenticatedRequest(savePath, text);

            // return a string representing the location of the data. Will be stored in the trial_results output.
            return savePath; 
        }

        public override void CleanUp()
        {
            // No cleanup is needed in this case. But you can add any code here that will be run when the session finishes.
        }

        void AuthenticatedRequest(string filepath, string data)
        {
            WWWForm form = new WWWForm();
            form.AddField(filepathKey, filepath);
            form.AddField(dataKey, data);

            UnityWebRequest www = UnityWebRequest.Post(url, form);

            if (httpBasicAuthentication)
            {
                string auth = username + ":" + password;
                auth = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
                auth = "Basic " + auth;
                www.SetRequestHeader("AUTHORIZATION", auth);
            }

            StartCoroutine(SendRequest(www));
        }

        IEnumerator SendRequest(UnityWebRequest www)
        {
            yield return www.SendWebRequest();

            bool error;
#if UNITY_2020_OR_NEWER
            error = www.result != UnityWebRequest.Result.Success;
#else
#pragma warning disable
            error = www.isHttpError || www.isNetworkError;
#pragma warning restore
#endif
            if (error)
            {
                Utilities.UXFDebugLogError(www.error);
            }
        }

        public string GetSessionPathRelative(string experiment, string ppid, int sessionNum)
        {
            string path = Path.Combine(experiment, ppid, FileSaver.SessionNumToName(sessionNum));
            return path;
        }
    }
}
