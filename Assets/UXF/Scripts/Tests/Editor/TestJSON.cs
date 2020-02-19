using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UXF.Tests
{
	public class TestJSON
	{

		Dictionary<string, object> dict = new Dictionary<string, object>()
		{
			{"string", "aaa"},
			{"bool", true},
			{"int", 3},
			{"float", 3.14f},
			{"array", new List<object>(){1, 2, 3, 4}},
			{"object", new Dictionary<string, object>(){{"a", 1}, {"b", "abc"}}}
		};

		const string expectedOutput = @"{
    ""string"": ""aaa"",
    ""bool"": true,
    ""int"": 3,
    ""float"": ""3.14"",
    ""array"": [
        1,
        2,
        3,
        4
    ],
    ""object"": {
        ""a"": 1,
        ""b"": ""abc""
    }
}";

		[Test]
		public void JSONIndentation()
		{
			string outString = MiniJSON.Json.Serialize(dict);
			Assert.AreEqual(expectedOutput, outString);
			Debug.Log(expectedOutput);
		}
	}

}