using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UXF.Tests
{

	public class TestSettings {

		string jsonString =
			"{ \"array\": [1.44,2,3], " +
			"\"object\": {\"key1\":\"value1\", \"key2\":256}, " +
			"\"string\": \"The quick brown fox \\\"jumps\\\" over the lazy dog \", " +
			"\"unicode\": \"\\u3041 Men\u00fa sesi\u00f3n\", " +
			"\"int\": 65536, " +
			"\"float\": 3.1415926, " +
			"\"bool\": true, " +
			"\"null\": null }";

		[Test]
		public void DeserializeJson()
		{
			Dictionary<string, object> dict = MiniJSON.Json.Deserialize(jsonString) as Dictionary<string, object>;

			Assert.IsNotNull(dict);

			var deserializedList = new List<double>() { 1.44, 2, 3 };
			Assert.AreEqual(dict["array"], deserializedList);

			var deserializedObject = new Dictionary<string, object>()
			{
				{ "key1", "value1" },
				{ "key2", 256L }
			};
			Assert.AreEqual(dict["object"], deserializedObject);

			Assert.AreEqual(dict["string"], "The quick brown fox \"jumps\" over the lazy dog ");
			Assert.AreEqual(dict["unicode"], "\u3041 Men\u00fa sesi\u00f3n");
			Assert.AreEqual(dict["int"], 65536L);
			Assert.AreEqual(dict["float"], 3.1415926d);
			Assert.AreEqual(dict["bool"], true);
			Assert.IsNull(dict["null"]);
		}

		[Test]
		public void DictToSettings()
		{
			Dictionary<string, object> dict = MiniJSON.Json.Deserialize(jsonString) as Dictionary<string, object>;
			Settings settings = new Settings(dict);

			var deserializedList = new List<double>() { 1.44, 2, 3 };
			Assert.AreEqual(settings["array"], deserializedList);

			var deserializedObject = new Dictionary<string, object>()
			{
				{ "key1", "value1" },
				{ "key2", 256L }
			};
			Assert.AreEqual(settings["object"], deserializedObject);

			Assert.AreEqual(settings["string"], "The quick brown fox \"jumps\" over the lazy dog ");
			Assert.AreEqual(settings["unicode"], "\u3041 Men\u00fa sesi\u00f3n");
			Assert.AreEqual(settings["int"], 65536L);
			Assert.AreEqual(settings["float"], 3.1415926d);
			Assert.AreEqual(settings["bool"], true);
			Assert.IsNull(settings["null"]);

		}

		[Test]
		public void GetSetSettings()
		{
			Dictionary<string, object> dict = MiniJSON.Json.Deserialize(jsonString) as Dictionary<string, object>;
			Settings settings = new Settings(dict);

			settings["key1"] = "test";
			settings["key2"] = 100;

			Assert.AreEqual(settings["key1"], "test");
			Assert.AreEqual(settings["key2"], 100);
			Assert.IsNull(settings["key3"]);
		}

		[Test]
		public void CascadeSettings()
		{
			Dictionary<string, object> dict = MiniJSON.Json.Deserialize(jsonString) as Dictionary<string, object>;
			Settings settings = new Settings(dict);

			Settings subSettings = Settings.empty;
			subSettings.SetParent(settings);

			subSettings["key1"] = "test";
			subSettings["key2"] = 100;

			Assert.AreEqual(subSettings["string"], "The quick brown fox \"jumps\" over the lazy dog ");
			Assert.AreEqual(subSettings["key1"], "test");
			Assert.AreEqual(subSettings["key2"], 100);
			Assert.IsNull(subSettings["key3"]);

		}

		[Test]
		public void ConvertSettings()
		{
			Dictionary<string, object> dict = MiniJSON.Json.Deserialize(jsonString) as Dictionary<string, object>;
			Settings settings = new Settings(dict);

			Assert.AreEqual(Convert.ToSingle(settings["int"]), 65536f);
			Assert.AreEqual(Convert.ToInt32(settings["int"]), 65536);

		}

	}

}