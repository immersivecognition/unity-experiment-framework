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
            fileSaver.StoragePath = "example_output";
            fileSaver.SetUp();

            // generate a large dictionary
			var dict = new Dictionary<string, object>();

            var largeArray = new string[100];
            string largeString = new string('*', 50000);

			// write lots and lots of JSON files
			int n = 100;
            string[] fpaths = new string[n];
			for (int i = 0; i < n; i++)
			{
				string fileName = string.Format("{0}", i);
				Debug.LogFormat("Queueing {0}", fileName);
            	string fpath = fileSaver.HandleText(largeString, experiment, ppid, sessionNum, fileName, UXFDataType.OtherSessionData);
                fpaths[i] = fpath;
			}

            Debug.Log("###########################################");
            Debug.Log("############## CLEANING UP ################");
            Debug.Log("###########################################");
            fileSaver.CleanUp();

            Assert.Throws<System.InvalidOperationException>(() => {
                fileSaver.HandleText(largeString, experiment, ppid, sessionNum, "0", UXFDataType.OtherSessionData);
            });

			// cleanup files
            foreach (var fpath in fpaths)
            {
                System.IO.File.Delete(Path.Combine(fileSaver.StoragePath, fpath));
            }
        }


        [Test]
        public void EarlyExit()
        {
            fileSaver.StoragePath = "example_output";
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

        [Test]
        public void AbsolutePath()
        {
            fileSaver.StoragePath = "C:/example_output";
            fileSaver.SetUp();
            
            string outString = fileSaver.HandleText("abc", experiment, ppid, sessionNum, "test", UXFDataType.OtherSessionData);

            Assert.AreEqual(outString, @"fileSaver_test/test_ppid/S001/other/test.txt");

            fileSaver.CleanUp();
        }

        [Test]
        public void PersistentDataPath()
        {
            fileSaver.dataSaveLocation = DataSaveLocation.PersistentDataPath;
            fileSaver.SetUp();
            Assert.AreEqual(Application.persistentDataPath, fileSaver.StoragePath);

            string dataOutput = "abc";
            fileSaver.HandleText(dataOutput, experiment, ppid, sessionNum, "test", UXFDataType.OtherSessionData);
            fileSaver.CleanUp();
            string outFile = Path.Combine(Application.persistentDataPath, @"fileSaver_test/test_ppid/S001/other/test.txt");

            string readData = File.ReadAllText(outFile);
            Assert.AreEqual(dataOutput, readData);
            
            if (File.Exists(outFile)) File.Delete(outFile);
        }

        [Test]
        public void FileSaverRelPath()
        {

            Assert.AreEqual(
                FileSaver.GetRelativePath("C:\\base", "C:\\base\\123"),
                "123"
            );

            Assert.AreEqual(
                FileSaver.GetRelativePath("base", "base\\123"),
                "123"
            );

            Assert.AreEqual(
                FileSaver.GetRelativePath("base/", "base\\123"),
                "123"
            );

            Assert.AreEqual(
                FileSaver.GetRelativePath("C:/base/", "C:/base\\123"),
                "123"
            );
        }

        [Test]
        public void MoveToBackup()
        {
            fileSaver.StoragePath = "test_output";
            if (Directory.Exists(fileSaver.StoragePath))
            {
                Directory.Delete(fileSaver.StoragePath, true);
            }

            fileSaver.SetUp();

            string fileName = "testMoveToBackup";
            fileSaver.HandleText("", "a", "b", 1, fileName, UXFDataType.TrialResults);
            fileSaver.HandleText("", "a", "b", 1, fileName, UXFDataType.TrialResults);
            fileSaver.HandleText("", "a", "b", 1, fileName, UXFDataType.TrialResults);

            System.Threading.Thread.Sleep(500);

            string testFilesDirectory = fileSaver.GetSessionPath("a", "b", 1);

            string[] files = Directory.GetFiles(testFilesDirectory, $"{fileName}*.txt");
            Assert.AreEqual(files.Length, 3);

            fileSaver.CleanUp();
            Directory.Delete(fileSaver.StoragePath, true);
        }

    }

}
