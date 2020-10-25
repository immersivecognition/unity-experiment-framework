using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.TestTools;
using System.Collections.Generic;
using NUnit.Framework;
using System.Collections;
using System.IO;

namespace UXF.Tests
{

    public class TestFileSaver
    {

        string experiment = "fileSaver_test";
        string ppid = "test_ppid";
        int sessionNum = 1;
		FileSaver fileSaver;

        [SetUp]
        public void SetUp()
        {
			var gameObject = new GameObject();
			fileSaver = gameObject.AddComponent<FileSaver>();
            fileSaver.storagePath = "example_output";
			fileSaver.verboseDebug = true;
        }


		[TearDown]
		public void TearDown()
		{			
			GameObject.DestroyImmediate(fileSaver.gameObject);
		}


        [Test]
        public void WriteManyFiles()
        {
            fileSaver.SetUp();

            // generate a large dictionary
			var dict = new Dictionary<string, object>();

            var largeArray = new int[10000];
			for (int i = 0; i < largeArray.Length; i++)
                largeArray[i] = i;

			dict["large_array"] = largeArray;

			// write lots and lots of JSON files
			int n = 100;
            string[] fpaths = new string[n];
			for (int i = 0; i < n; i++)
			{
				string fileName = string.Format("{0:000}", i);
				Debug.LogFormat("Queueing {0}", fileName);
            	string fpath = fileSaver.HandleJSONSerializableObject(dict, experiment, ppid, sessionNum, fileName, UXFDataType.OtherSessionData);
                fpaths[i] = fpath;
			}

            fileSaver.CleanUp();

			// cleanup files
            foreach (var fpath in fpaths)
            {
                System.IO.File.Delete(fpath);
            }
        }


        [Test]
        public void EarlyExit()
        {
            fileSaver.SetUp();
            fileSaver.CleanUp();
			
			Assert.Throws<System.InvalidOperationException>(
				() => {
                    fileSaver.ManageInWorker(() => Debug.Log("Code enqueued after FileSaver ended"));
				}
			);

            fileSaver.SetUp();
            fileSaver.ManageInWorker(() => Debug.Log("Code enqueued after FileSaver re-opened"));
            fileSaver.CleanUp();

        }

    }

}