using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UXF.UI;

namespace UXF.Tests
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    public class ReentrantDirectorySelector : LocalDirectorySelector
    {
        public int dialogCount;

        protected override string[] ShowFolderPanel(string title, string directory, bool multiselect)
        {
            dialogCount++;
            SelectFolder(); // Simulate the click being dispatched again by a modal message loop.
            return new string[0];
        }
    }

    public class TestLocalDirectorySelectorPlayMode
    {
        [UnityTest]
        public IEnumerator FolderClickOpensOneDialogAndAllowsAnotherAfterItCloses()
        {
            var gameObject = new GameObject("Directory selector reentry test");
            var form = gameObject.AddComponent<FormElement>();
            form.Initialise(() => string.Empty, _ => { });
            var selector = gameObject.AddComponent<ReentrantDirectorySelector>();
            selector.inputField = form;

            selector.SelectFolder();
            selector.SelectFolder();
            Assert.That(selector.dialogCount, Is.Zero, "The picker should open after the click finishes.");

            yield return null;
            yield return null;
            Assert.That(selector.dialogCount, Is.EqualTo(1), "A modal reentry must not open another picker.");

            selector.SelectFolder();
            yield return null;
            yield return null;
            Assert.That(selector.dialogCount, Is.EqualTo(2), "A later click should still open a picker.");

            Object.Destroy(gameObject);
        }
    }

#if UNITY_STANDALONE_WIN
    public class ReentrantWindowsFolderBrowser : SFB.StandaloneFileBrowserWindows
    {
        public int dialogCount;

        protected override string[] OpenFolderPanelCore(string title, string directory)
        {
            dialogCount++;
            Assert.That(OpenFolderPanel(title, directory, false), Is.Empty);
            return new[] { directory };
        }
    }

    public class TestWindowsFolderBrowser
    {
        [Test]
        public void NativeModalReentryDoesNotOpenAnotherFolderDialog()
        {
            var browser = new ReentrantWindowsFolderBrowser();
            Assert.That(browser.OpenFolderPanel("Select", "C:\\", false), Is.EqualTo(new[] { "C:\\" }));
            Assert.That(browser.dialogCount, Is.EqualTo(1));
            Assert.That(browser.OpenFolderPanel("Select", "C:\\", false), Is.EqualTo(new[] { "C:\\" }));
            Assert.That(browser.dialogCount, Is.EqualTo(2), "The guard must reset after a dialog closes.");
        }
    }
#endif
#endif
}
