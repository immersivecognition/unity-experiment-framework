using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UXF;

namespace UXFExamples
{
    public class Example_MultiSceneExperimentGenerator : MonoBehaviour
    {

		/// <summary>
		/// Awake is called when the script instance is being loaded.
		/// </summary>
		void Awake()
		{
			// here we make sure this object does not get destroyed when a new scene loads.
			DontDestroyOnLoad(gameObject);	
		}

		// we will call this when the session starts.
        public void Generate(Session session)
		{			
			// here we generate 2 blocks with a numer of trials defined by the settings file.
			Block block1 = session.CreateBlock(session.settings.GetInt("block1_numtrials"));
			Block block2 = session.CreateBlock(session.settings.GetInt("block2_numtrials"));

			// assign some settings, stating scene name that should be used
			block1.settings.SetValue("scene_name", "ShootingExample");
			block2.settings.SetValue("scene_name", "ReactionExample");

			// in block 1, lets create random difficulty on each trial, by changing the "target size"
			foreach (var trial in block1.trials)
			{
				trial.settings.SetValue("target_size", Random.Range(1.0f, 2.0f));
			}

			// in block 2, lets generate some delays for the reaction time task
			foreach (var trial in block2.trials)
			{
				// for the press stimulus and the fake stimulus
				trial.settings.SetValue("press_delay", Random.Range(0.5f, 5.0f));
				trial.settings.SetValue("fake_delay", Random.Range(0.5f, 5.0f));
			}
 
			// even though the settings are not used in all trials, we still need to set something if we want to write out the value of the setting
			block1.settings.SetValue("press_delay", null);
			block1.settings.SetValue("fake_delay", null);
			block2.settings.SetValue("target_size", null); 
		}

		// we will call this when the trial starts.
		public void SetupTrial(Trial trial)
		{
			// IMPORTANT
			// if this is the first trial in the block, we need to load a new scene.
			if (trial.numberInBlock == 1)
			{
				string scenePath = trial.settings.GetString("scene_name");

				// we'll load the scene asynchronously to avoid stutters
				AsyncOperation loadScene = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scenePath);

				// here we specify what we WILL do when the scene is loaded
				loadScene.completed += (op) => { SceneSpecificSetup(trial); };
			}
			else
			{
				// if it is NOT the first trial in the block, we just do some scene specific stuff immediately, no need to wait for scene load
				SceneSpecificSetup(trial);
			}
		}

		void SceneSpecificSetup(Trial trial)
		{
			// in order to perform scene-specific setup, we will find and our scene-specific scripts 
			// there are lots of ways to do this, but this works fine here
			if (trial.block.number == 1)
			{
				FindObjectOfType<Example_ShootingTask>().StartShootingTaskTrial(trial);
			}
			else if (trial.block.number == 2)
			{
				FindObjectOfType<Example_ReactionTask>().StartReactionTaskTrial(trial);
			}
		}


		public void CleanupTrial(Trial trial)
		{
			// same as above, but these are run on trial end not on begin

			// in order to perform scene-specific cleanup, we will find and our scene-specific scripts 
			// there are lots of ways to do this, but this works fine here
			if (trial.block.number == 1)
			{
				FindObjectOfType<Example_ShootingTask>().EndShootingTaskTrial(trial);
			}
			else if (trial.block.number == 2)
			{
				FindObjectOfType<Example_ReactionTask>().EndReactionTaskTrial(trial);
			}
		}



# if UNITY_EDITOR

		/// <summary>
		/// This function is called when the object becomes enabled and active.
		/// </summary>
		void OnValidate()
		{
			// scenes need to be added to build index in order to be able to be loaded - you can do this by opening the build settings menu
			// I do it here in script just incase a user changes it.
			AddScenesToBuildIndex();
		}

		// this just adds the scenes to the build index - you can normally do this by opening build settings, you don't need to write code
		// https://docs.unity3d.com/Manual/BuildSettings.html
		void AddScenesToBuildIndex()
		{
			List<string> newSceneNames = new List<string>()
			{
				"Assets/UXF/Examples/3_MultiScene_FootballAndRT/OtherScenes/ShootingExample.unity",
				"Assets/UXF/Examples/3_MultiScene_FootballAndRT/OtherScenes/ReactionExample.unity"
			};

			var scenes = new List<UnityEditor.EditorBuildSettingsScene>(UnityEditor.EditorBuildSettings.scenes);

			foreach (var newScene in newSceneNames)
			{
				var newSceneAdded = scenes
					.Where(s => s.path == newScene);

				if (newSceneAdded.Count() == 0)
				{
					scenes.Add(new UnityEditor.EditorBuildSettingsScene(newScene, true));
				}
				else if (newSceneAdded.Count() == 1)
				{
					newSceneAdded.Single().enabled = true;			
				}
				else
				{
					scenes = scenes
						.Where(s => s.path != newScene)
						.ToList();
					scenes.Add(new UnityEditor.EditorBuildSettingsScene(newScene, true));
				}
			}			

			UnityEditor.EditorBuildSettings.scenes = scenes.ToArray();

			Debug.Log("Added scenes to build settings for Multi Scene Example.");
		}
# endif

    }
}