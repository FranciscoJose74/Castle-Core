// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if !SILVERLIGHT
namespace Castle.DynamicProxy
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Runtime.Serialization.Formatters.Binary;
	using Castle.DynamicProxy.Generators;

	/// <summary>
	/// Applied to the assemblies saved by <see cref="ModuleScope"/> in order to persist the cache data included in the persisted assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	[CLSCompliant(false)]
	public class CacheMappingsAttribute : Attribute
	{
		private static readonly ConstructorInfo constructor =
			typeof (CacheMappingsAttribute).GetConstructor(new Type[] {typeof (byte[])});

		public static void ApplyTo(AssemblyBuilder assemblyBuilder, Dictionary<CacheKey, string> mappings)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, mappings);
				byte[] bytes = stream.ToArray();
				CustomAttributeBuilder attributeBuilder = new CustomAttributeBuilder(constructor, new object[] {bytes});
				assemblyBuilder.SetCustomAttribute(attributeBuilder);
			}
		}

		private readonly byte[] _serializedCacheMappings;

		public CacheMappingsAttribute(byte[] serializedCacheMappings)
		{
			_serializedCacheMappings = serializedCacheMappings;
		}

		public byte[] SerializedCacheMappings
		{
			get { return _serializedCacheMappings; }
		}

		public Dictionary<CacheKey, string> GetDeserializedMappings()
		{
			using (MemoryStream stream = new MemoryStream(SerializedCacheMappings))
			{
				BinaryFormatter formatter = new BinaryFormatter();
				return (Dictionary<CacheKey, string>) formatter.Deserialize(stream);
			}
		}
	}
}
#endif
