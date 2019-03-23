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
			"\"array_double\": [1.44,2.44,3.44], " +
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
			var deserializedObject = new Dictionary<string, object>()
			{
				{ "key1", "value1" },
				{ "key2", 256L }
			};

			Assert.AreEqual(dict["array"], deserializedList);
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
			var deserializedIntList = new List<double>() { 1, 2, 3 };
			var deserializedDoubleList = new List<double>() { 1.44, 2.44, 3.44 };
			var deserializedFloatList = new List<double>() { 1.44f, 2.44f, 3.44f };
			var deserializedObject = new Dictionary<string, object>()
			{
				{ "key1", "value1" },
				{ "key2", 256L }
			};

			Assert.AreEqual(settings.GetObjectList("array"), deserializedList);
			Assert.AreEqual(settings.GetIntList("array"), deserializedIntList);
			Assert.AreEqual(settings.GetFloatList("array_double"), deserializedFloatList);
			Assert.AreEqual(settings.GetDoubleList("array_double"), deserializedDoubleList);


			Assert.AreEqual(settings.GetObject("object"), deserializedObject);
			Assert.AreEqual(settings.GetDict("object"), deserializedObject);
			Assert.AreEqual(settings.GetString("string"), "The quick brown fox \"jumps\" over the lazy dog ");
			Assert.AreEqual(settings.GetString("unicode"), "\u3041 Men\u00fa sesi\u00f3n");
			Assert.AreEqual(settings.GetInt("int"), 65536);
			Assert.AreEqual(settings.GetLong("int"), 65536L);
			Assert.AreEqual(settings.GetFloat("float"), 3.1415926f);
			Assert.AreEqual(settings.GetDouble("float"), 3.1415926d);
			Assert.AreEqual(settings.GetBool("bool"), true);
			Assert.IsNull(settings.GetObject("null"));

		}

		[Test]
		public void GetSetSettings()
		{
			Dictionary<string, object> dict = MiniJSON.Json.Deserialize(jsonString) as Dictionary<string, object>;
			Settings settings = new Settings(dict);

			settings.SetValue("key1", "test");
			settings.SetValue("key2", 100);

			Assert.AreEqual(settings.GetString("key1"), "test");
			Assert.AreEqual(settings.GetInt("key2"), 100);
			Assert.Throws<KeyNotFoundException>(() => settings.GetObject("key3"));
		}

		[Test]
		public void CascadeSettings()
		{
			Dictionary<string, object> dict = MiniJSON.Json.Deserialize(jsonString) as Dictionary<string, object>;
			Settings settings = new Settings(dict);

			Settings subSettings = Settings.empty;
			subSettings.SetParent(settings);

			subSettings.SetValue("key1", "test");
			subSettings.SetValue("key2", 100);

			Assert.AreEqual(subSettings.GetString("string"), "The quick brown fox \"jumps\" over the lazy dog ");
			Assert.AreEqual(subSettings.GetString("key1"), "test");
			Assert.AreEqual(subSettings.GetInt("key2"), 100);
			Assert.Throws<KeyNotFoundException>(() => settings.GetObject("key3"));
		}

	}

}