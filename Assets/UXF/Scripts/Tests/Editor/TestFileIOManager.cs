using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.TestTools;
using System.Collections.Generic;
using NUnit.Framework;
using System.Collections;

namespace UXF.Tests
{

    public class TestFileIOManager
    {

        string path = "example_output/fileiomanager_test";
		FileIOManager fileIOManager;

        List<WriteFileInfo> writtenFiles;

        [SetUp]
        public void SetUp()
        {
			var gameObject = new GameObject();
			fileIOManager = gameObject.AddComponent<FileIOManager>();
			fileIOManager.debug = true;
        }


		[TearDown]
		public void TearDown()
		{			
			GameObject.DestroyImmediate(fileIOManager.gameObject);
		}


        [Test]
        public void WriteManyFiles()
        {
            fileIOManager.Begin();

            // generate a large dictionary
			var dict = new Dictionary<string, object>();

            var largeArray = new int[10000];
			for (int i = 0; i < largeArray.Length; i++)
                largeArray[i] = i;

			dict["large_array"] = largeArray;

            
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);


			// write lots of JSON files
			int n = 100;
			for (int i = 0; i < n; i++)
			{
				string fileName = string.Format("{0:000}.json", i);
                WriteFileInfo fileInfo = new WriteFileInfo(WriteFileType.Test, path, fileName);
				Debug.LogFormat("Queueing {0}", fileName);
            	fileIOManager.ManageInWorker(() => fileIOManager.WriteJson(dict, fileInfo));
			}

            fileIOManager.End();

			// cleanup files
            var files = System.IO.Directory.GetFiles(path, "*.json");
            foreach (var file in files)
            {
                System.IO.File.Delete(file);
            }
        }


        [Test]
        public void EarlyExit()
        {
            fileIOManager.Begin();
            fileIOManager.End();
			
			Assert.Throws<System.InvalidOperationException>(
				() => {
                    fileIOManager.ManageInWorker(() => Debug.Log("Code enqueued after FileIOManager ended"));
				}
			);

            fileIOManager.Begin();
            fileIOManager.ManageInWorker(() => Debug.Log("Code enqueued after FileIOManager re-opened"));
            fileIOManager.End();

        }

        [Test]
        public void WriteFileEventTest()
        {
            writtenFiles = new List<WriteFileInfo>();
            fileIOManager.onWriteFile.AddListener(new UnityAction<WriteFileInfo>(DoSomethingWithFile));
            fileIOManager.Begin();

            // generate a dictionary
            var dict = new Dictionary<string, object>();

            var intArray = new int[10];
            for (int i = 0; i < intArray.Length; i++)
                intArray[i] = i;
            dict["int_array"] = intArray;

            // write lots of JSON files
            int n = 100;
            WriteFileInfo[] fileInfos = new WriteFileInfo[n];
            for (int i = 0; i < n; i++)
            {
                string fileName = string.Format("{0:000}.json", i);
                WriteFileInfo fileInfo = new WriteFileInfo(WriteFileType.Test, path, fileName);
                fileInfos[i] = fileInfo;
                Debug.LogFormat("Queueing {0}", fileName);
                fileIOManager.ManageInWorker(() => fileIOManager.WriteJson(dict, fileInfo));
            }

            // end and join
            fileIOManager.End();

            // now check each file was passed to the event (i.e. added to the written files list)
            for (int i = 0; i < n; i++)
            {
                Assert.AreEqual(fileInfos[i], writtenFiles[i]);
            }

            writtenFiles.Clear();
        }

        void DoSomethingWithFile(WriteFileInfo writeFileInfo)
        {
            Debug.LogFormat("Received {0} file: {1}", writeFileInfo.fileType, writeFileInfo.paths[writeFileInfo.paths.Length-1]);
            System.Threading.Thread.Sleep(100); // sleep here to force producer loop to end early
            writtenFiles.Add(writeFileInfo);
        }

    }

}