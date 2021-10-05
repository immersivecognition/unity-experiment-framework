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

	public class TestTrials
	{
        GameObject gameObject;
        Session session;
        FileSaver fileSaver;
        SessionLogger sessionLogger;

        string experimentName = "unit_test";
        string ppid = "test_trials";
        int sessionNum = 1;

        void Start()
        {
            gameObject = new GameObject();
            fileSaver = gameObject.AddComponent<FileSaver>();
            sessionLogger = gameObject.AddComponent<SessionLogger>();
			if (Session.instance != null) GameObject.DestroyImmediate(Session.instance.gameObject);
            session = gameObject.AddComponent<Session>();

            session.dataHandlers = new DataHandler[]{ fileSaver };

            sessionLogger.AttachReferences(
                session
            );

            sessionLogger.Initialise();
            fileSaver.StoragePath = "example_output";
            fileSaver.verboseDebug = true;

            session.Begin(experimentName, ppid);
            session.customHeaders.Add("observation");
            session.customHeaders.Add("null_observation");

			// generate trials
			session.CreateBlock(2);
            session.CreateBlock(3);
        }

        void Finish()
        {
            session.End();
            GameObject.DestroyImmediate(gameObject);
        }


        [Test]
        public void RunTrialsAdHocResultsAdd()
        {   
            Start();
            int i = 0;
            foreach (var trial in session.Trials)
            {
                trial.Begin();
                trial.result["observation"] = ++i;
                trial.result["null_observation"] = null;
                trial.result["not_customheader_observation"] = "something";

                Assert.AreSame(trial, session.CurrentTrial);
                Assert.AreEqual(trial.number, session.currentTrialNum);

                trial.End();
            }

            i = 0;
            foreach (var trial in session.Trials)
            {
                Assert.AreEqual(trial.result["observation"], ++i);
            }
            string sessionPath = fileSaver.GetSessionPath(experimentName, ppid, sessionNum);
            Finish();

            // read the file to check headers
            string firstLine = File.ReadAllLines(Path.Combine(sessionPath, "trial_results.csv"))[0];
            Assert.AreEqual("experiment,ppid,session_num,trial_num,block_num,trial_num_in_block,start_time,end_time,observation,null_observation,not_customheader_observation", firstLine);           
        }

        [Test]
        public void RunTrialsAdHocResultsAddEarlyExit()
        {   
            Start();
            session.GetBlock(1).GetRelativeTrial(1).Begin();
            session.GetBlock(1).GetRelativeTrial(1).result["not_customheader_observation"] = "something";
            session.GetBlock(1).GetRelativeTrial(1).End();
            
            session.GetBlock(1).trials[1].Begin();
            Finish(); // check we dont throw an error
        }

        [Test]
        public void BeginTrialBeforeBeginSession()
        {
            gameObject = new GameObject();
			if (Session.instance != null) GameObject.DestroyImmediate(Session.instance.gameObject);
            session = gameObject.AddComponent<Session>();

			// generate trials
			session.CreateBlock(1);

            Assert.Throws<InvalidOperationException>(() => session.FirstTrial.Begin());
        }


        [Test]
        public void WriteCommas()
        {
            Start();
            session.GetBlock(1).GetRelativeTrial(1).Begin();
            session.GetBlock(1).GetRelativeTrial(1).result["observation"] = "hello, hello"; // comma
            session.GetBlock(1).GetRelativeTrial(1).End();
            
            session.GetBlock(1).GetRelativeTrial(2).Begin();
            session.GetBlock(1).GetRelativeTrial(2).result["observation"] = Vector3.one; // Vector3.ToString() output contains commas
            session.GetBlock(1).GetRelativeTrial(2).End();

            int numHeaders = session.Headers.Count;

            string sessionPath = fileSaver.GetSessionPath(experimentName, ppid, sessionNum);
            Finish();

            // read the file back in, check number of columns equals number of headers
            string[] lines = File.ReadAllLines(Path.Combine(sessionPath, "trial_results.csv"));

            // num headers is equal
            Assert.AreEqual(numHeaders, lines[0].Split(',').GetLength(0));

            // trial 1 columns should equal header length
            Debug.Log( lines[1]);
            Assert.AreEqual(numHeaders, lines[1].Split(',').GetLength(0));

            // trial 2 columns should equal header length
            Assert.AreEqual(numHeaders, lines[2].Split(',').GetLength(0));
        }

	}

}