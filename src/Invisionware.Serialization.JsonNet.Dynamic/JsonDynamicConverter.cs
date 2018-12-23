using System;
using System.Collections.Generic;
using System.Linq;
using Invisionware.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Invisionware.Serialization.JsonNet.Dynamic
{
	public class JsonDynamicConverter : JsonConverter
	{
		/// <summary>
		/// The type cache
		/// </summary>
		private IDictionary<Type, IList<JsonDynamicTypeAttribute>> _typeCache;

		/// <summary>
		/// Builds the cache of classes.
		/// </summary>
		private void BuildCacheOfClasses()
		{
			if (_typeCache != null) return;

			var typesWithMyAttribute =
				// Note the AsParallel here, this will parallelize everything after.
				from a in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
				from t in a.GetTypes()
				let attributes = t.GetAttributeOfType<JsonDynamicTypeAttribute>()
				where attributes != null  && attributes.AnySafe() // && !t.IsPrimitive //&& !t.FullName.StartsWith("System.") && !t.FullName.StartsWith("Microsoft.") && !t.IsCOMObject
				select new { Type = t, Attributes = attributes };
			
			_typeCache = typesWithMyAttribute.ToDictionary(x => x.Type, y => y.Attributes);

			if (_typeCache != null && _typeCache.Any())
			{
				foreach (var typeCacheKey in _typeCache.Keys)
				{
					Log.Debug("JsonDynamicConverter: Cached Type {FullName}", typeCacheKey.FullName);
				}
			}
		}

		/// <summary>
		/// Determines whether this instance can convert the specified object type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns><c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.</returns>
		public override bool CanConvert(Type objectType)
		{
			var attr = objectType.GetAttributeOfType<JsonDynamicBaseTypeAttribute>() != null || objectType.GetAttributeOfType<JsonDynamicTypeAttribute>() != null;

			return attr;
			//return true;
		}

		#region Overrides of JsonConverter

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON.
		/// </summary>
		/// <value><c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON; otherwise, <c>false</c>.</value>
		public override bool CanRead { get; } = true;

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON.
		/// </summary>
		/// <value><c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON; otherwise, <c>false</c>.</value>
		public override bool CanWrite { get; } = false;

		#endregion

		/// <summary>
		/// Creates the specified object type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="jObject">The json object with data that needs to be converted to a string type.</param>
		/// <param name="serializer">The json.net serializer.</param>
		/// <returns>System.Object.</returns>
		private object Create(Type objectType, JObject jObject, JsonSerializer serializer)
		{
			object obj = null;

			var attr = objectType.GetAttributeOfType<JsonDynamicBaseTypeAttribute>().FirstOrDefault();
			if (attr != null)
			{
				Log.Debug("JsonDynamicConverter: Create SubType '{0}' [Property={1}]", objectType.FullName, attr.PropertyName);

				// Does the object have the property we need to key off of?
				if (jObject.TryGetValue(attr.PropertyName, StringComparison.CurrentCultureIgnoreCase, out var jValue))
				{
					var str = (string)jValue;
					Log.Debug("JsonDynamicConverter: Create SubType '{0}' [PropertyValue={1}]", objectType.FullName, str);

					var typeDetails = _typeCache.Where(x => x.Value.AnySafe()).FirstOrDefault(x =>
						string.Compare(Convert.ToString(x.Value.FirstOrDefault().PropertyValue), str,
							StringComparison.CurrentCultureIgnoreCase) == 0);

					if (typeDetails.Value == null)
					{
						// Hmm, this really should not be possible
						Log.Warning("JsonDynamicConverter: No subtype for '{0}' found in the cache ", str);

						return null;
					}

					Log.Debug("JsonDynamicConverter: Create Resolved SubType '{0}'", typeDetails.Key.FullName);

					obj = serializer.ContractResolver?.ResolveContract(typeDetails.Key)?.DefaultCreator() ?? Activator.CreateInstance(typeDetails.Key);
				}
			}

			try
			{
				if (obj == null)
				{
					Log.Debug("JsonDynamicConverter: Create Type '{0}'", objectType.FullName);

					obj = serializer.ContractResolver?.ResolveContract(objectType) ?? (objectType.IsClass ? Activator.CreateInstance(objectType) : null);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "JsonDynamicConverter: Error while trying to Create Type '{0}'", objectType.FullName);

				obj = null;
			}

			return obj;
		}


		/// <summary>
		/// Reads the JSON representation of the object.
		/// </summary>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="existingValue">The existing value of object being read.</param>
		/// <param name="serializer">The calling serializer.</param>
		/// <returns>The object value.</returns>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{			
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}

			BuildCacheOfClasses();

			// Load JObject from stream
			var jObject = JObject.Load(reader);

			// Create target object based on JObject
			var target = Create(objectType, jObject, serializer);
			if (target == null)
			{
				return null;
			}

			// Populate the object properties
			using (var jObjectReader = CopyReaderForObject(reader, jObject))
			{
				serializer.Populate(jObjectReader, target);
			}

			return target;
		}

		/// <summary>
		/// Serializes to the specified type
		/// </summary>
		/// <param name="writer">Newtonsoft.Json.JsonWriter</param>
		/// <param name="value">Object to serialize.</param>
		/// <param name="serializer">Newtonsoft.Json.JsonSerializer to use.</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}

		/// <summary>
		/// Creates a new reader for the specified jObject by copying the settings
		/// from an existing reader.
		/// </summary>
		/// <param name="reader">The reader whose settings should be copied.</param>
		/// <param name="jObject">The jObject to create a new reader for.</param>
		/// <returns>The new disposable reader.</returns>
		public static JsonReader CopyReaderForObject(JsonReader reader, JObject jObject)
		{
			var jObjectReader = jObject.CreateReader();

			jObjectReader.Culture = reader.Culture;
			jObjectReader.DateFormatString = reader.DateFormatString;
			jObjectReader.DateParseHandling = reader.DateParseHandling;
			jObjectReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
			jObjectReader.FloatParseHandling = reader.FloatParseHandling;
			jObjectReader.MaxDepth = reader.MaxDepth;
			jObjectReader.SupportMultipleContent = reader.SupportMultipleContent;

			return jObjectReader;
		}
	}
}
