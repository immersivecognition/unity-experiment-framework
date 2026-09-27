using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;
using System.IO;
using System.Globalization;
using System.Threading;

namespace UXF.Tests
{
    public class TestFileSaver
    {
        string experiment = "fileSaver_test";
        string ppid = "test_ppid";
        int sessionNum = 1;
        FileSaver fileSaver;
        string testStoragePath;

        [SetUp]
        public void SetUp()
        {
            testStoragePath = Path.Combine(
                Path.GetTempPath(),
                "UXFTests",
                "FileSaver-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(testStoragePath);

            var gameObject = new GameObject();
            fileSaver = gameObject.AddComponent<FileSaver>();
            fileSaver.StoragePath = testStoragePath;
            fileSaver.verboseDebug = true;
        }


        [TearDown]
        public void TearDown()
        {
            try
            {
                if (fileSaver != null)
                    fileSaver.CleanUp();
            }
            finally
            {
                if (fileSaver != null)
                    GameObject.DestroyImmediate(fileSaver.gameObject);

                if (Directory.Exists(testStoragePath))
                    Directory.Delete(testStoragePath, true);
            }
        }


        [Test]
        public void WriteManyFiles()
        {
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
                string fpath = fileSaver.HandleText(largeString, experiment, ppid, sessionNum, fileName,
                    UXFDataType.OtherSessionData);
                fpaths[i] = fpath;
            }

            Debug.Log("###########################################");
            Debug.Log("############## CLEANING UP ################");
            Debug.Log("###########################################");
            fileSaver.CleanUp();

            Assert.Throws<System.InvalidOperationException>(() =>
            {
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
            fileSaver.SetUp();
            fileSaver.CleanUp();

            Assert.Throws<System.InvalidOperationException>(
                () => { fileSaver.ManageInWorker(() => Debug.Log("Code enqueued after FileSaver ended")); }
            );

            fileSaver.SetUp();
            fileSaver.ManageInWorker(() => Debug.Log("Code enqueued after FileSaver re-opened"));
            fileSaver.CleanUp();
        }

        [Test]
        public void AbsolutePath()
        {
            fileSaver.StoragePath = Path.Combine(testStoragePath, "absolute");
            fileSaver.SetUp();

            string outString =
                fileSaver.HandleText("abc", experiment, ppid, sessionNum, "test", UXFDataType.OtherSessionData);

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
            string outFile = Path.Combine(Application.persistentDataPath,
                @"fileSaver_test/test_ppid/S001/other/test.txt");

            string readData = File.ReadAllText(outFile);
            Assert.AreEqual(dataOutput, readData);

            if (File.Exists(outFile)) File.Delete(outFile);
        }

        [Test]
        public void FileSaverRelPath()
        {
            string root = Path.Combine(Path.GetTempPath(), "UXF Tests", "base");
            string nested = Path.Combine(root, "nested", "123");

            Assert.AreEqual(
                FileSaver.GetRelativePath(root, Path.Combine(root, "123")),
                "123"
            );

            Assert.AreEqual(
                FileSaver.GetRelativePath(root + Path.DirectorySeparatorChar, nested),
                "nested/123"
            );

            Assert.AreEqual(
                FileSaver.GetRelativePath(root, root),
                "."
            );
        }

        [Test]
        public void FileSaverRejectsTraversalInSessionIdentifiers()
        {
            Assert.Throws<System.ArgumentException>(() =>
                fileSaver.GetSessionPath("../outside", "participant", 1));
            Assert.Throws<System.ArgumentException>(() =>
                fileSaver.GetSessionPath("experiment", "..\\outside", 1));
            Assert.Throws<System.ArgumentException>(() =>
                fileSaver.GetSessionPath("C:/outside", "participant", 1));
        }

        [Test]
        public void FileSaverDoesNotChangeCallerCulture()
        {
            CultureInfo original = Thread.CurrentThread.CurrentCulture;
            try
            {
                CultureInfo callerCulture = new CultureInfo("de-DE");
                Thread.CurrentThread.CurrentCulture = callerCulture;
                fileSaver.forceENUSLocale = true;
                fileSaver.SetUp();

                Assert.AreSame(callerCulture, Thread.CurrentThread.CurrentCulture);
                fileSaver.CleanUp();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }

        [Test]
        public void FileSaverCleanupIsSafeBeforeSetupAndWhenRepeated()
        {
            fileSaver.CleanUp();
            fileSaver.SetUp();
            fileSaver.CleanUp();
            fileSaver.CleanUp();
        }
    }
}
