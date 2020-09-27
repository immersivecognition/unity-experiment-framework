using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UXF.Tests
{

	public class TestMultithreading
	{

        GameObject gameObject;
        Session session;
        FileIOManager fileIOManager;
        SessionLogger sessionLogger;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject();
            fileIOManager = gameObject.AddComponent<FileIOManager>();
            sessionLogger = gameObject.AddComponent<SessionLogger>();
            session = gameObject.AddComponent<Session>();

            sessionLogger.AttachReferences(
                session
            );

            fileIOManager.storageLocation = "example_output";

            session.dataHandlers = new DataHandler[]{ fileIOManager };

            sessionLogger.Initialise();

            fileIOManager.verboseDebug = true;
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void AddSettingsDuringWriting()
        {
            string experimentName = "unit_test";
            string ppid = "test_trials";
            Settings settings = Settings.empty;

            // initialise settings
            for (int i = 0; i < 10000; i++)
            {
                string key = i.ToString();
                settings.SetValue(key, i);
            }

            session.Begin(experimentName, ppid, settings: settings);
            
            // add lots more during potential writing
            for (int i = 0; i < 10000; i++)
            {
                string key = "_" + i.ToString();
                settings.SetValue(key, i);
            }

            session.End();
        }

	}

}