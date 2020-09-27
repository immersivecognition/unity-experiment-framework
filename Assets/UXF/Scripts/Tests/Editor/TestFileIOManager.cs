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

    public class TestFileIOManager
    {

        string experiment = "fileiomanager_test";
        string ppid = "test_ppid";
        int sessionNum = 1;
		FileIOManager fileIOManager;

        List<WriteFileInfo> writtenFiles;

        [SetUp]
        public void SetUp()
        {
			var gameObject = new GameObject();
			fileIOManager = gameObject.AddComponent<FileIOManager>();
            fileIOManager.storageLocation = "example_output";
			fileIOManager.verboseDebug = true;
        }


		[TearDown]
		public void TearDown()
		{			
			GameObject.DestroyImmediate(fileIOManager.gameObject);
		}


        [Test]
        public void WriteManyFiles()
        {
            fileIOManager.SetUp();

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
            	string fpath = fileIOManager.HandleJSONSerializableObject(dict, experiment, ppid, sessionNum, fileName, dataType: DataType.Other);
                fpaths[i] = fpath;
			}

            fileIOManager.CleanUp();

			// cleanup files
            foreach (var fpath in fpaths)
            {
                System.IO.File.Delete(fpath);
            }
        }


        [Test]
        public void EarlyExit()
        {
            fileIOManager.SetUp();
            fileIOManager.CleanUp();
			
			Assert.Throws<System.InvalidOperationException>(
				() => {
                    fileIOManager.ManageInWorker(() => Debug.Log("Code enqueued after FileIOManager ended"));
				}
			);

            fileIOManager.SetUp();
            fileIOManager.ManageInWorker(() => Debug.Log("Code enqueued after FileIOManager re-opened"));
            fileIOManager.CleanUp();

        }

    }

}