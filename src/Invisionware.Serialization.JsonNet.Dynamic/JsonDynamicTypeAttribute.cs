using System;
using System.Collections.Generic;
using System.Text;

namespace Invisionware.Serialization.JsonNet.Dynamic
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class JsonDynamicBaseTypeAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the name of the property that is used to indicate the Key used in the dynamic serialization.
		/// </summary>
		/// <value>The name of the property.</value>
		public string PropertyName { get; set; }
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class JsonDynamicTypeAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the type of the base that this class/interface is associated with.
		/// </summary>
		/// <value>The type of the base.</value>
		public Type BaseType { get; set; }
		/// <summary>
		/// Gets or sets the property value.
		/// </summary>
		/// <value>The property value.</value>
		public object PropertyValue { get; set; }
	}
}
