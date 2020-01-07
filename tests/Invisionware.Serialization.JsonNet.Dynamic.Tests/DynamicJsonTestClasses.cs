using Newtonsoft.Json;
using System.Collections.Generic;

namespace Invisionware.Serialization.JsonNet.Dynamic.UnitTests
{
	// This is our base contract
	// Note the "PropertyName" set to "Name" -- this will tell the framework that the object property "Name" will be used to determin what object we need to create during deserialization
	[JsonDynamicBaseType(PropertyName = "Name")]
	[JsonConverter(typeof(JsonDynamicConverter))]
	public interface IDynamicJsonBase
	{
		string Name { get; set; }
	}

	// Lets define a base class that implements the base contract
	public class DynamicJsonObjectBase : IDynamicJsonBase
	{
		public string Name { get; set; }
	}

	// Now lets define a child class that is derived from our base class
	// Note the "PropertyValue" set to "NumberValue" -- this will be key as it will help the deserializer know which class to map 
	[JsonDynamicType(BaseType = typeof(IDynamicJsonBase), PropertyValue = "NumberValue")]
	public class DynamicJsonObjectA : DynamicJsonObjectBase
	{
		public int NumberValue { get; set; }
	}

	// Now lets define a completely different child class that is derived from our base class
	// Note the "PropertyValue" set to "StringValue" -- this will be key as it will help the deserializer know which class to map 
	[JsonDynamicType(BaseType = typeof(IDynamicJsonBase), PropertyValue = "StringValue")]
	public class DynamicJsonObjectB : IDynamicJsonBase
	{
		public string Name { get; set; }
		public string StringValue { get; set; }
	}

	// Now lets define another different child class that is derived from our base class and implements a dictionary (just because we can!)
	// Note the "PropertyValue" set to "DictionaryValue" -- this will be key as it will help the deserializer know which class to map 
	[JsonDynamicType(BaseType = typeof(IDynamicJsonBase), PropertyValue = "DictionaryValue")]
	public class DynamicJsonObjectC : IDynamicJsonBase
	{
		public string Name { get; set; }
		public Dictionary<string, string> Properties { get; set; }
	}


	// And to top it all off lets create a class that contains a list of anything that is based on the original contract
	public class DynamicJsonObjectTestClass
	{
		public IList<IDynamicJsonBase> Items { get; set; } = new List<IDynamicJsonBase>();
		public IList<IDynamicJsonBase> OtherItems { get; set; } = new List<IDynamicJsonBase>();
	}

}