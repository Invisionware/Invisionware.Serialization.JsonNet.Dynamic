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

			var str = JsonConvert.SerializeObject(testObject);

			str.Should().NotBeNullOrEmpty();
			//str.Should().BeEquivalentTo(_str);

			var result = JsonConvert.DeserializeObject<DynamicJsonObjectTestClass>(str);

			result.Should().NotBeNull();
			result.Items.Should().NotBeEmpty();
			result.Items.FirstOrDefault(x => x.Name == "NumberValue").Should().BeOfType<DynamicJsonObjectA>();
			result.Items.FirstOrDefault(x => x.Name == "StringValue").Should().BeOfType<DynamicJsonObjectB>();
		}

	}

	[JsonDynamicBaseType(PropertyName = "Name")]
	[JsonConverter(typeof(JsonDynamicConverter))]
	public interface IDynamicJsonBase
	{
		string Name { get; set; }
	}

	public class DynamicJsonObjectBase : IDynamicJsonBase
	{
		public string Name { get; set; }
	}

	[JsonDynamicType(BaseType = typeof(IDynamicJsonBase), PropertyValue = "NumberValue")]
	public class DynamicJsonObjectA : DynamicJsonObjectBase
	{
		public int NumberValue { get; set; }
	}

	[JsonDynamicType(BaseType = typeof(IDynamicJsonBase), PropertyValue = "StringValue")]
	public class DynamicJsonObjectB : IDynamicJsonBase
	{
		public string Name { get; set; }
		public string StringValue { get; set; }
	}

	public class DynamicJsonObjectTestClass
	{
		public IList<IDynamicJsonBase> Items { get; set; } = new List<IDynamicJsonBase>();
	}
}