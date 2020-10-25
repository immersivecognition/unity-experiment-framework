﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.IO;
using System.Threading;
using System.Linq;
using System.Runtime.InteropServices;

namespace UXF
{
    /// <summary>
    /// Component which uploads data to an AWS DynamoDB on Web based builds.
    /// </summary>
    public class WebAWSDynamoDB : DataHandler
    {

        public bool endSessionWhenTryCloseTab = true;

        [SubjectNerd.Utilities.EditScriptable]
        public AWSCredentials credentials;


        [DllImport("__Internal")]
        private static extern void DDB_Setup(string region, string identityPool, string callbackGameObjectName);

        [DllImport("__Internal")]
        private static extern void DDB_CreateTable(string tableName, string primaryKeyName, string sortKeyName, string callbackGameObjectName);

        [DllImport("__Internal")]
        private static extern void DDB_PutItem(string tableName, string jsonItem, string callbackGameObjectName);

        [DllImport("__Internal")]
        private static extern void DDB_BatchWriteItem(string tableName, string jsonRequests, string callbackGameObjectName);

        [DllImport("__Internal")]
        private static extern void DDB_GetItem(string tableName, string jsonItem, string callbackGameObjectName, string guid);

        [DllImport("__Internal")]
        private static extern void DDB_Cleanup();

        private const string primaryKey = "ppid_session_dataname";
        private const string sortKey = "trial_num";


        /// <summary>
        /// When we read from the database, we use SendMessage to send the data back. This dictionary stores the user's callback
        /// and a unique string as the key to make sure the data gets sent to the right callback. 
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, Action<Dictionary<string, object>>> requestCallbackMap = new Dictionary<string, Action<Dictionary<string, object>>>();


        public override void SetUp()
        {
            bool ok = CheckCurrentTargetOK();
            if (!ok) return;
            if (credentials == null) Debug.LogError("Credentials have not been set!");
            DDB_Setup(credentials.region, credentials.cognitoIdentityPool, gameObject.name);
            
            var allDataTypes = Enum.GetValues(typeof(UXFDataType)); 
            foreach (UXFDataType dt in allDataTypes)
            {
                string tableName = GetTableName(session.experimentName, dt);
                UXFDataLevel dl = DatabaseDataLevel(dt);
                string newSortKey = dl == UXFDataLevel.PerTrial ? sortKey : string.Empty;
                DDB_CreateTable(tableName, primaryKey, newSortKey, gameObject.name);
            }
        }


        public override bool CheckIfRiskOfOverwrite(string experiment, string ppid, int sessionNum, string rootPath = "")
        {
            // maybe implement db read here
            return false;
        }

        /// <summary>
        /// Handles a UXF Data Table. Should not normally be called by the user. Instead, call session.SaveDataTable() or trial.SaveDataTable().
        /// </summary>
        /// <param name="table"></param>
        /// <param name="experiment"></param>
        /// <param name="ppid"></param>
        /// <param name="sessionNum"></param>
        /// <param name="dataName"></param>
        /// <param name="dataType"></param>
        /// <param name="optionalTrialNum"></param>
        public override string HandleDataTable(UXFDataTable table, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum)
        {
            if (dataType == UXFDataType.TrialResults)
            {
                // special case, one item per trial, but multiple items
                // so we need BatchWriteItem
                string primaryKeyValue = GetFormattedPrimaryKeyValue(ppid, sessionNum, dataName);
                string tableName = GetTableName(experiment, dataType);

                if (!table.Headers.Contains("trial_num"))
                {
                    Debug.LogError("Data supplied is supposed to be per-trial but does not contain 'trial_num' column!");
                    return "error";
                }
                var dataList = table.GetAsListOfDict()
                    .Select(item => {
                        item[primaryKey] = primaryKeyValue;
                        return item;
                    })
                    .Cast<object>()
                    .ToList();

                string req = MiniJSON.Json.Serialize(dataList);
                DDB_BatchWriteItem(tableName, req, gameObject.name);
                return string.Format("dynamodb:{0}:{1}", tableName, primaryKeyValue);
            }
            else
            {
                Dictionary<string, object> dataDict = table
                            .GetAsDictOfList()
                            .ToDictionary(kvp => kvp.Key, 
                                          kvp => (object)kvp.Value);
                
                return HandleJSONSerializableObject(dataDict, experiment, ppid, sessionNum, dataName, dataType, optionalTrialNum);
            }
            
        }

        /// <summary>
        /// Handle a JSON-Serializable object. Should not normally be called by the user. Instead, call session.SaveJSONSerializableObject() or trial.SaveJSONSerializableObject().
        /// </summary>
        /// <param name="serializableObject"></param>
        /// <param name="experiment"></param>
        /// <param name="ppid"></param>
        /// <param name="sessionNum"></param>
        /// <param name="dataName"></param>
        /// <param name="dataType"></param>
        /// <param name="optionalTrialNum"></param>
        /// <returns></returns>
        public override string HandleJSONSerializableObject(List<object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum)
        {
            // turn list<object> into Dict with one key: data
            var newDict = new Dictionary<string, object>()
            {
                { dataName, new List<object>(serializableObject) } // copy
            };

            return HandleJSONSerializableObject(newDict, experiment, ppid, sessionNum, dataName, dataType, optionalTrialNum);
        }

        /// <summary>
        /// Handles a byte array. Should not normally be called by the user. Instead, call session.SaveBytes() or trial.SaveBytes().
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="experiment"></param>
        /// <param name="ppid"></param>
        /// <param name="sessionNum"></param>
        /// <param name="dataName"></param>
        /// <param name="dataType"></param>
        /// <param name="optionalTrialNum"></param>
        /// <returns></returns>
        public override string HandleBytes(byte[] bytes, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum)
        {
            string content = System.Text.Encoding.UTF8.GetString(bytes);
            return HandleText(content, experiment, ppid, sessionNum, dataName, dataType, optionalTrialNum);
        }

        /// <summary>
        /// Handles a string. Should not normally be called by the user. Instead, call session.SaveBytes() or trial.SaveBytes().
        /// </summary>
        /// <param name="text"></param>
        /// <param name="experiment"></param>
        /// <param name="ppid"></param>
        /// <param name="sessionNum"></param>
        /// <param name="dataName"></param>
        /// <param name="dataType"></param>
        /// <param name="optionalTrialNum"></param>
        /// <returns></returns>

        public override string HandleText(string text, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum)
        {
            var newDict = new Dictionary<string, object>()
            {
                { dataName, text }
            };

            return HandleJSONSerializableObject(newDict, experiment, ppid, sessionNum, dataName, dataType, optionalTrialNum);
        }

        /// <summary>
        /// Handle a JSON-Serializable object. Should not normally be called by the user. Instead, call session.SaveJSONSerializableObject() or trial.SaveJSONSerializableObject().
        /// </summary>
        /// <param name="serializableObject"></param>
        /// <param name="experiment"></param>
        /// <param name="ppid"></param>
        /// <param name="sessionNum"></param>
        /// <param name="dataName"></param>
        /// <param name="dataType"></param>
        /// <param name="optionalTrialNum"></param>
        /// <returns></returns>
        public override string HandleJSONSerializableObject(Dictionary<string, object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum)
        {
            // most of the other methods end up calling this

            serializableObject = new Dictionary<string, object>(serializableObject); // copy

            string primaryKeyValue = GetFormattedPrimaryKeyValue(ppid, sessionNum, dataName);
            string tableName = GetTableName(experiment, dataType);
            string outName;

            serializableObject.Add(primaryKey, primaryKeyValue);

            if (DatabaseDataLevel(dataType) == UXFDataLevel.PerTrial)
            {
                serializableObject.Add(sortKey, optionalTrialNum);
                outName = string.Format("dynamodb:{0}:{1}:{2}", tableName, primaryKeyValue, optionalTrialNum);
            }
            else
            {
                outName = string.Format("dynamodb:{0}:{1}", tableName, primaryKeyValue);
            }

            string req = MiniJSON.Json.Serialize(serializableObject);
            DDB_PutItem(tableName, req, gameObject.name);
            return outName;
        }


        public override void CleanUp()
        {
            DDB_Cleanup();
            requestCallbackMap.Clear();
        }


        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="item"></param>
        public void PutCustomDataInDB(string tableName, Dictionary<string, object> item)
        {
            bool ok = CheckCurrentTargetOK();
            if (!ok) return;

            string req = MiniJSON.Json.Serialize(item);
            DDB_PutItem(tableName, req, gameObject.name);
        }


        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="experimentName"></param>
        /// <param name="dataType"></param>
        /// <param name="callbackFunction"></param>
        /// <param name="optionalTrialNumber"></param>
        public void GetUXFDataFromDB(string experimentName, UXFDataType dataType, string ppid, int sessionNum, string dataName, Action<Dictionary<string, object>> callbackFunction, int optionalTrialNumber = 0)
        {
            bool ok = CheckCurrentTargetOK();
            if (!ok) return;


            if (DatabaseDataLevel(dataType) == UXFDataLevel.PerTrial)
            {
                GetCustomDataFromDB(
                    GetTableName(experimentName, dataType),
                    primaryKey,
                    GetFormattedPrimaryKeyValue(ppid, sessionNum, dataName),
                    callbackFunction,
                    sortKey,
                    optionalTrialNumber
                );
            }
            else
            {
                GetCustomDataFromDB(
                    GetTableName(experimentName, dataType),
                    primaryKey,
                    GetFormattedPrimaryKeyValue(ppid, sessionNum, dataName),
                    callbackFunction
                );
            }            
        }


        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="experimentName"></param>
        /// <param name="dataType"></param>
        /// <param name="callback"></param>
        /// <param name="optionalTrialNumber"></param>
        public void GetCustomDataFromDB(string tableName, string primaryKeyName, object primaryKeyValue, Action<Dictionary<string, object>> callback, string optionalSortKeyName = "", object optionalSortKeyValue = null)
        {
            bool ok = CheckCurrentTargetOK();
            if (!ok) return;

            Dictionary<string, object> request = new Dictionary<string, object>();
            request.Add(primaryKeyName, primaryKeyValue);
            if (optionalSortKeyName != "")
            {
                // add sort key in this case
                request.Add(optionalSortKeyName, optionalSortKeyValue);
            }

            string guid = Guid.NewGuid().ToString();
            string jsonRequest = MiniJSON.Json.Serialize(request);
            DDB_GetItem(tableName, jsonRequest, gameObject.name, guid);
            requestCallbackMap.Add(guid, callback);
        }


        private void ShowError(string text)
        {
            Debug.LogError(text);
        }

        private void HandleSuccessfulDBRead(string jsonResult)
        {
            Debug.Log(jsonResult);
            Dictionary<string, object> jsonRequest = (Dictionary<string, object>) MiniJSON.Json.Deserialize(jsonResult);
            string guid = (string) jsonRequest["guid"];
            Action<Dictionary<string, object>> callback = requestCallbackMap[guid];

            requestCallbackMap.Remove(guid);
            callback.Invoke((Dictionary<string, object>) jsonRequest["result"]);
        }

        private void HandleUnsuccessfulDBRead(string jsonResult)
        {
            Dictionary<string, object> jsonRequest = (Dictionary<string, object>) MiniJSON.Json.Deserialize(jsonResult);
            string guid = (string) jsonRequest["guid"];
            Action<Dictionary<string, object>> callback = requestCallbackMap[guid];

            requestCallbackMap.Remove(guid);
            callback.Invoke(null);
        }
        
        /// <summary>
        /// This is called when the user tries to close the tab.
        /// </summary>
        private void HandleBeforeUnloadEvent()
        {
            if (endSessionWhenTryCloseTab)
            {
                session.End();
            }
        }

        private UXFDataLevel DatabaseDataLevel(UXFDataType dt)
        {
            UXFDataLevel dl = dt.GetDataLevel();
            // trial results is a special case, session level file, but trial level data
            if (dt == UXFDataType.TrialResults) dl = UXFDataLevel.PerTrial;

            return dl;
        }

        private string GetTableName(string experimentName, UXFDataType dataType)
        {
            return string.Format("UXFData.{0}.{1}", experimentName, dataType.ToString());
        }

        public string GetFormattedPrimaryKeyValue(string ppid, int sessionNum, string dataName)
        {
            return string.Format("{0}_s{1:000}_{2}", ppid, sessionNum, dataName);
        }

        
        private bool CheckCurrentTargetOK()
        {
# if UNITY_EDITOR
            var buildTarget = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
            if (buildTarget != UnityEditor.BuildTarget.WebGL)
            {
                Debug.LogError("Web AWS DynamoDB Data Handler is not compatible with current build target. Please switch your platform in Build Settings to WebGL if you wish to use this.");
            }
            Debug.LogWarning("Unfortunately the Web AWS DynamoDB Data Handler cannot communicate with the database whilst running in the Unity Editor, because it relies on the AWS JavaScript SDK. To test data upload, create a build of your application and test it in a web browser. You can still test in the Unity Editor without database access.");
            return false;
# else
            return true;
# endif
        }

# if UNITY_EDITOR
        /// <summary>
        /// Returns true if this data handler is definitley compatible with this build target.
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public override bool IsCompatibleWith(UnityEditor.BuildTargetGroup buildTarget)
        {
            switch (buildTarget)
            {
                case UnityEditor.BuildTargetGroup.WebGL:
                    return true;
                default:
                    return false;
            }
        }

         /// <summary>
        /// Returns true if this data handler is definitley incompatible with this build target.
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public override bool IsIncompatibleWith(UnityEditor.BuildTargetGroup buildTarget)
        {
            switch (buildTarget)
            {
                case UnityEditor.BuildTargetGroup.Android:
                case UnityEditor.BuildTargetGroup.Facebook:
                case UnityEditor.BuildTargetGroup.iOS:
                case UnityEditor.BuildTargetGroup.Lumin:
                case UnityEditor.BuildTargetGroup.PS4:
                case UnityEditor.BuildTargetGroup.Switch:
                case UnityEditor.BuildTargetGroup.tvOS:
                case UnityEditor.BuildTargetGroup.XboxOne:
                case UnityEditor.BuildTargetGroup.WSA:
                    return true;
                default:
                    return false;
            }
        }
# endif
        
    }

    public class DynamoDBCreationQuery
    {

        public DynamoDBCreationQuery(string tableName)
        {

        }
    }

}
