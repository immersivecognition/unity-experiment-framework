using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.IO;

namespace UXF.Tests
{
	public class TestSessionBuilding {

		GameObject gameObject;
		Session session;

		[SetUp]
		public void SetUp()
		{
			gameObject = new GameObject();
			session = gameObject.AddComponent<Session>();
			gameObject.AddComponent<FileIOManager>();
		}

		[TearDown]
		public void TearDown()
		{
			GameObject.DestroyImmediate(gameObject);
		}

		[Test]
		public void InitialiseSession() // move to own test class
		{
			string experimentName = "experiment";
			string ppid = "test";
			session.Begin(experimentName, ppid, "example_output");

			Assert.That(session.hasInitialised);
			Assert.AreEqual(session.experimentName, experimentName);
			Assert.AreEqual(session.ppid, ppid);
			Assert.That(Directory.Exists(session.path));
		}

		[Test]
		public void CreateBlocks()
		{
			Block block1 = session.CreateBlock(5);
			Assert.AreEqual(block1.trials.Count, 5);
			
			Block block2 = session.CreateBlock();
			Assert.AreEqual(block2.trials.Count, 0);

			Trial trial = block2.CreateTrial();
			Assert.AreEqual(block2.trials.Count, 1);
			Assert.AreEqual(trial.number, 6);
			
			Assert.AreEqual(session.blocks.Count, 2);

		}
		
	}

}