using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UXF.Tests
{
    public class AWSDDBTest : MonoBehaviour
    {
        public Session session;
        public WebAWSDynamoDB ddb;
        public Text showText;

        Dictionary<string, object> dict = new Dictionary<string, object>()
        {
            {"string", "aaa"},
            {"bool", true},
            {"int", 3},
            {"float", 3.14f},
            {"list", new List<object>(){1, 2, 3, 4}},
            {"array", new object[]{1, 2, 3, 4}},
            {"object", new Dictionary<string, object>(){{"a", 1}, {"b", "abc"}}}
        };

        List<object> list = new List<object>()
        {
            new Dictionary<string, object>(){ { "string", "aaa" } },
            new Dictionary<string, object>(){ { "string", "aaa" } },
            new Dictionary<string, object>(){ { "string", "aaa" } }
        };

        public void StartAndEndATrial()
        {
            Block b = session.CreateBlock(1);

            b.firstTrial.Begin();
            b.firstTrial.result["result"] = 12345;
            b.firstTrial.End();
        }

        public void CreateSessionData()
        {
            session.SaveJSONSerializableObject(dict, "dict");
            session.SaveJSONSerializableObject(list, "list");
            session.SaveText("adfiodsfgiusdfgisdfgjsdfgij\nsdfwef", "text");
        }

        public void CreateTrialData()
        {
            session.CurrentTrial.SaveJSONSerializableObject(dict, "dict");
            session.CurrentTrial.SaveJSONSerializableObject(list, "list");
            session.CurrentTrial.SaveText("adfiodsfgiusdfgisdfgjsdfgij\nsdfwef", "text");
        }

        public void GetSessionData()
        {
            ddb.GetUXFDataFromDB(session.experimentName, UXFDataType.OtherSessionData, session.ppid, session.number, "dict", ShowDict);
        }

        public void GetTrialData()
        {
            ddb.GetUXFDataFromDB(session.experimentName, UXFDataType.OtherTrialData, session.ppid, session.number, "dict", ShowDict, session.currentTrialNum);
        }

        void ShowDict(Dictionary<string, object> dict)
        {
            if (dict == null)
            {
                showText.text = "Response was null";
                return;
            }

            Debug.LogFormat("Keys are: ", string.Join(", ", dict.Keys));

            string newText = string.Format("Response at time: {0}\n", Time.realtimeSinceStartup);
            newText += string.Format("string: {0}\n", dict["string"]);
            newText += string.Format("bool: {0}\n", dict["bool"]);
            newText += string.Format("int: {0}\n", dict["int"]);
            newText += string.Format("float: {0}\n", dict["float"]);
            newText += string.Format("list: {0}\n", dict["list"]);
            newText += string.Format("array: {0}\n", dict["array"]);
            newText += string.Format("object: {0}\n", dict["object"]);

            showText.text = newText;
        }

        public void EndSession()
        {
            session.End();
        }

    }

}

