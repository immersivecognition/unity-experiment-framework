using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UXF.Tests
{
	public class TestSessionBuilding
	{
		GameObject gameObject;
		Session session;

		[SetUp]
		public void SetUp()
		{
			gameObject = new GameObject();
            if (Session.instance != null) GameObject.DestroyImmediate(Session.instance.gameObject);
			session = gameObject.AddComponent<Session>();			
		}

		[TearDown]
		public void TearDown()
		{
			GameObject.DestroyImmediate(gameObject);
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

			// reset blocks
			session.blocks = new List<Block>();

		}

		[Test]
		public void ShuffleBlocks()
		{
			for (int i = 0; i < 5; i++)
			{
				Block block = session.CreateBlock(10);
				block.settings.SetValue("order", i);
			}

			var rng = new System.Random(0);

			session.blocks.Shuffle(rng);
			int[] expectedOrders = new int[]{ 0, 1, 2, 4, 3 };
            for (int i = 0; i < 5; i++)
			{
				var block = session.blocks[i];
				var expected = expectedOrders[i];
				Assert.AreEqual(block.settings.GetInt("order"), expected);
			}

            // reset blocks
            session.blocks = new List<Block>();

		}


        [Test]
        public void ShuffleTrials()
        {
            Block block = session.CreateBlock(10);

            for (int i = 0; i < block.trials.Count; i++)
			{
                block.trials[i].settings.SetValue("order", i);
			}

            var rng = new System.Random(10);
            int[] expectedOrders = new int[] { 2, 0, 3, 5, 1, 7, 4, 8, 6, 9 };

            block.trials.Shuffle(rng);
            for (int i = 0; i < block.trials.Count; i++)
			{
				var trial = block.trials[i];
                var expected = expectedOrders[i];
				Assert.AreEqual(trial.settings.GetInt("order"), expected);
			}

            // reset blocks
            session.blocks = new List<Block>();

        }

		[Test]

		public void SwapTrials()
		{
			Block block = session.CreateBlock(5);

            for (int i = 0; i < block.trials.Count; i++)
			{
                block.trials[i].settings.SetValue("order", i);
			}

			int[] expectedOrders = new int[] { 0, 3, 2, 1, 4 };

			block.trials.Swap(1, 3);

			for (int i = 0; i < block.trials.Count; i++)
			{
				var trial = block.trials[i];
                var expected = expectedOrders[i];
				Assert.AreEqual(trial.settings.GetInt("order"), expected);
			}

			// reset blocks
            session.blocks = new List<Block>();

		}

		[Test]
		public void InvalidTrialAccess()
		{
			session.CreateBlock();

			Assert.Throws<NoSuchTrialException>(
				delegate { Trial t = session.FirstTrial; }
			);

			Assert.Throws<NoSuchTrialException>(
				delegate { Trial t = session.LastTrial; }
			);

			// reset blocks
            session.blocks = new List<Block>();
		}

		[Test]
		public void InvalidBlockAccess()
		{
			Assert.Throws<NoSuchTrialException>(
				delegate { Trial t = session.FirstTrial; }
			);

			Assert.Throws<NoSuchTrialException>(
				delegate { Trial t = session.LastTrial; }
			);
		}

		[Test]
		public void FirstLast()
		{
			Block block1 = session.CreateBlock(10);
			Block block2 = session.CreateBlock(10);

			Assert.AreEqual(block1.GetRelativeTrial(1), session.FirstTrial);
			Assert.AreEqual(block2.GetRelativeTrial(10), session.LastTrial);

			// reset blocks
            session.blocks = new List<Block>();
		}


		[Test]
		public void GetLastTrialWhenLastBlockEmpty()
		{
			Block block1 = session.CreateBlock(10);
			Block block2 = session.CreateBlock();

			Assert.AreEqual(session.LastTrial, block1.GetRelativeTrial(10));

			// reset blocks
            session.blocks = new List<Block>();
		}
		
		[Test]
		public void GetLastTrialWhenNoTrials()
		{
			Block block1 = session.CreateBlock();
			Block block2 = session.CreateBlock();

			Assert.Throws<NoSuchTrialException>(() => { var a = session.LastTrial; } );

			// reset blocks
            session.blocks = new List<Block>();
		}

		[Test]
		public void GetLastTrialWhenNoBlocks()
		{
			Assert.Throws<NoSuchTrialException>(() => { var a = session.LastTrial; } );

			// reset blocks
            session.blocks = new List<Block>();
		}
		
	}

}