using UnityEngine;
using UnityEditor;
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
				string fileName = System.IO.Path.Combine(path, string.Format("{0:000}.json", i));
				Debug.LogFormat("Queueing {0}", fileName);
            	fileIOManager.ManageInWorker(() => fileIOManager.WriteJson(fileName, dict));
			}

            fileIOManager.End();

			// cleanup files
            var files = System.IO.Directory.GetFiles(path, "*.json");
            foreach (var file in files)
            {
                System.IO.File.Delete(file);
            }
        }
    }

}