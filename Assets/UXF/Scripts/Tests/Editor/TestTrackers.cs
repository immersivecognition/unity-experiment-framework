using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UXF.Tests
{

	public class TestTrackers
	{

        GameObject gameObject;
        Session session;
        FileIOManager fileIOManager;
        SessionLogger sessionLogger;
		List<GameObject> tracked = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject();
            fileIOManager = gameObject.AddComponent<FileIOManager>();
            sessionLogger = gameObject.AddComponent<SessionLogger>();
            session = gameObject.AddComponent<Session>();

            session.AttachReferences(
                fileIOManager
            );

            sessionLogger.AttachReferences(
                fileIOManager,
                session
            );

            sessionLogger.Initialise();

            fileIOManager.debug = true;
            fileIOManager.Begin();

            string experimentName = "unit_test";
            string ppid = "test_trackers";
            session.Begin(experimentName, ppid, "example_output");


            for (int i = 0; i < 5; i++)
			{
                GameObject trackedObject = new GameObject();
                Tracker tracker = trackedObject.AddComponent<PositionRotationTracker>();
				tracker.objectName = string.Format("Tracker_{0}", i);

				session.trackedObjects.Add(tracker);
                tracked.Add(trackedObject);	
			}


			// generate trials
			session.CreateBlock(10);
        }

        [TearDown]
        public void TearDown()
        {
            session.End();
            fileIOManager.End();
            GameObject.DestroyImmediate(gameObject);

			foreach (GameObject trackedObject in tracked)
			{
				GameObject.DestroyImmediate(trackedObject);
			}
        }

        [Test]	
        public void TrackManyObjects()
        {
			Random.InitState(1); // reproducible

			foreach (var trial in session.Trials)
			{

				trial.Begin();

				// record 100 times in each trial
				for (int i = 0; i < 100; i++)
				{
                    foreach (GameObject trackedObject in tracked)
                    {
                        trackedObject.transform.position = new Vector3
                        (
                            Random.value, Random.value, Random.value
                        );

                        trackedObject.transform.eulerAngles = new Vector3
                        (
                            Random.value, Random.value, Random.value
                        );
                    
						trackedObject.GetComponent<PositionRotationTracker>().RecordRow();
					}
				}
			
				trial.End();

			}

        }

	}

}