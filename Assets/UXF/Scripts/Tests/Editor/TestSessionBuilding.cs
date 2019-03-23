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
			session = gameObject.AddComponent<Session>();
			session.AttachReferences(
				newFileIOManager: gameObject.AddComponent<FileIOManager>()
			);
			
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

			int[] expectedOrders = new int[]{ 3, 0, 2, 1, 4 };
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
            int[] expectedOrders = new int[] { 9, 7, 2, 8, 0, 5, 1, 4, 6, 3 };

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
		
	}

}