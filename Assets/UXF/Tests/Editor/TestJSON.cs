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
			{"list", new List<object>(){1, 2, 3, 4}},
			{"array", new object[]{1, 2, 3, 4}},
			{"object", new Dictionary<string, object>(){{"a", 1}, {"b", "abc"}}}
		};

		const string jsonString = @"{
    ""string"": ""aaa"",
    ""bool"": true,
    ""int"": 3,
    ""float"": 3.1400001049041748,
    ""list"": [
        1,
        2,
        3,
        4
    ],
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
			Assert.AreEqual(jsonString, outString);
		}

		[Test]
		public void VectorSerialize()
		{
            var vectorDict = new Dictionary<string, object>
            {
                { "vector3", new Vector3(1, 2, 3) },
                { "vector2", new Vector2(1, 2) }
            };
			
            string vectorString = @"{
    ""vector3"": {
        ""x"": 1,
        ""y"": 2,
        ""z"": 3
    },
    ""vector2"": {
        ""x"": 1,
        ""y"": 2
    }
}";
            
            string outString = MiniJSON.Json.Serialize(vectorDict);
			Assert.AreEqual(vectorString, outString);
		}

	}

}