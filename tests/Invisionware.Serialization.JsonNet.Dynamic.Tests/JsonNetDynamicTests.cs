using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Invisionware.Serialization.JsonNet.Dynamic.UnitTests
{
	[Category("JsonDynamic")]
	public class JsonNetDynamicTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void JsonDynamicSerializeTest()
		{
			var testObject = new DynamicJsonObjectTestClass();

			testObject.Items.Add(new DynamicJsonObjectA { Name = "NumberValue", NumberValue = 1});
			testObject.Items.Add(new DynamicJsonObjectB { Name = "StringValue", StringValue = DateTime.Now.ToString()});
			testObject.Items.Add(new DynamicJsonObjectC { Name = "DictionaryValue", Properties = new Dictionary<string, string> { { "key1", "value1" } } });
			testObject.OtherItems.Add(new DynamicJsonObjectA { Name = "NumberValue", NumberValue = 2 });
			testObject.OtherItems.Add(new DynamicJsonObjectB { Name = "StringValue", StringValue = DateTime.Now.AddDays(1).ToString() });

			var str = JsonConvert.SerializeObject(testObject);

			str.Should().NotBeNullOrEmpty();
			//str.Should().BeEquivalentTo(_str);

			var result = JsonConvert.DeserializeObject<DynamicJsonObjectTestClass>(str);

			result.Should().NotBeNull();
			result.Items.Should().NotBeEmpty();
			result.Items.FirstOrDefault(x => x.Name == "NumberValue").Should().BeOfType<DynamicJsonObjectA>();
			result.Items.FirstOrDefault(x => x.Name == "StringValue").Should().BeOfType<DynamicJsonObjectB>();
			result.Items.FirstOrDefault(x => x.Name == "DictionaryValue").Should().BeOfType<DynamicJsonObjectC>();
			result.OtherItems.FirstOrDefault(x => x.Name == "NumberValue").Should().BeOfType<DynamicJsonObjectA>();
		}

	}
}