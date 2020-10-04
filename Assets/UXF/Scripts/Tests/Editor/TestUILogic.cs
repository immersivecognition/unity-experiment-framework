using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UXF.UI;

namespace UXF.Tests
{

	public class TestUILogic
	{
        GameObject testObject;
        UIController uiController;

		[SetUp]
		public void SetUp()
		{
			testObject = new GameObject("Test object");
            
			uiController = testObject.AddComponent<UIController>();	
		}

		[TearDown]
		public void TearDown()
		{
			GameObject.DestroyImmediate(testObject);
		}

        [Test]
        public void TestSettingsPPIDLogic()
        {   
            string reason;

            // Built in UI
            uiController.startupMode = StartupMode.BuiltInUI;

            uiController.ppidMode = PPIDMode.AcquireFromUI;
            Assert.IsTrue(uiController.PPIDModeIsValid(out reason));

            uiController.ppidMode = PPIDMode.GenerateUnique;
            Assert.IsTrue(uiController.PPIDModeIsValid(out reason));

            uiController.settingsMode = SettingsMode.AcquireFromUI;
            Assert.IsTrue(uiController.SettingsModeIsValid(out reason));

            uiController.settingsMode = SettingsMode.DownloadFromURL;
            Assert.IsTrue(uiController.SettingsModeIsValid(out reason));

            uiController.settingsMode = SettingsMode.Empty;
            Assert.IsTrue(uiController.SettingsModeIsValid(out reason));


            // Automatic
            uiController.startupMode = StartupMode.Automatic;

            uiController.ppidMode = PPIDMode.AcquireFromUI;
            Assert.IsFalse(uiController.PPIDModeIsValid(out reason));

            uiController.ppidMode = PPIDMode.GenerateUnique;
            Assert.IsTrue(uiController.PPIDModeIsValid(out reason));

            uiController.settingsMode = SettingsMode.AcquireFromUI;
            Assert.IsFalse(uiController.SettingsModeIsValid(out reason));

            uiController.settingsMode = SettingsMode.DownloadFromURL;
            Assert.IsTrue(uiController.SettingsModeIsValid(out reason));

            uiController.settingsMode = SettingsMode.Empty;
            Assert.IsTrue(uiController.SettingsModeIsValid(out reason));


            // Manual
            uiController.startupMode = StartupMode.Manual;

            uiController.ppidMode = PPIDMode.AcquireFromUI;
            Assert.IsTrue(uiController.PPIDModeIsValid(out reason));

            uiController.ppidMode = PPIDMode.GenerateUnique;
            Assert.IsTrue(uiController.PPIDModeIsValid(out reason));

            uiController.settingsMode = SettingsMode.AcquireFromUI;
            Assert.IsTrue(uiController.SettingsModeIsValid(out reason));

            uiController.settingsMode = SettingsMode.DownloadFromURL;
            Assert.IsTrue(uiController.SettingsModeIsValid(out reason));

            uiController.settingsMode = SettingsMode.Empty;
            Assert.IsTrue(uiController.SettingsModeIsValid(out reason));


        }

	}

}