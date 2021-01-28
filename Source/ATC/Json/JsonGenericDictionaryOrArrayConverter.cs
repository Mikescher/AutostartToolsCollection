using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace ATC.Json
{
	// https://stackoverflow.com/a/28633769/1761622
	public class JsonGenericDictionaryOrArrayConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return GetDictionaryKeyValueTypes(objectType).Count() == 1;
		}

		public override bool CanWrite => true;

		// ReSharper disable once UnusedMember.Local
		object ReadJsonGeneric<TKey, TValue>(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var tokenType = reader.TokenType;

			if (!(existingValue is IDictionary<TKey, TValue> dict))
			{
				var contract = serializer.ContractResolver.ResolveContract(objectType);
				dict = (IDictionary<TKey, TValue>)contract.DefaultCreator();
			}

			if (tokenType == JsonToken.StartArray)
			{
				var pairs = new JsonSerializer().Deserialize<KeyValuePair<TKey, TValue>[]>(reader);
				if (pairs == null)
					return existingValue;
				foreach (var pair in pairs)
					dict.Add(pair);
			}
			else if (tokenType == JsonToken.StartObject)
			{
				// Using "Populate()" avoids infinite recursion.
				// https://github.com/JamesNK/Newtonsoft.Json/blob/ee170dc5510bb3ffd35fc1b0d986f34e33c51ab9/Src/Newtonsoft.Json/Converters/CustomCreationConverter.cs
				serializer.Populate(reader, dict);
			}
			return dict;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var keyValueTypes = GetDictionaryKeyValueTypes(objectType).Single(); // Throws an exception if not exactly one.

			var method = GetType().GetMethod("ReadJsonGeneric", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			var genericMethod = method.MakeGenericMethod(keyValueTypes.Key, keyValueTypes.Value);
			return genericMethod.Invoke(this, new[] { reader, objectType, existingValue, serializer });
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}

		private IEnumerable<Type> GetInterfacesAndSelf(Type type)
		{
			if (type == null)
				throw new ArgumentNullException();
			if (type.IsInterface)
				return new[] { type }.Concat(type.GetInterfaces());
			else
				return type.GetInterfaces();
		}

		private IEnumerable<KeyValuePair<Type, Type>> GetDictionaryKeyValueTypes(Type type)
		{
			foreach (Type intType in GetInterfacesAndSelf(type))
			{
				if (intType.IsGenericType
				    && intType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
				{
					var args = intType.GetGenericArguments();
					if (args.Length == 2)
						yield return new KeyValuePair<Type, Type>(args[0], args[1]);
				}
			}
		}
	}
}
