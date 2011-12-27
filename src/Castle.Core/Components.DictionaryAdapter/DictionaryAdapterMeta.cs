﻿// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.Components.DictionaryAdapter
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;

	[DebuggerDisplay("Type: {Type.FullName,nq}")]
	public class DictionaryAdapterMeta
	{
		private IDictionary extendedProperties;

		public DictionaryAdapterMeta(Type type, object[] behaviors, IDictionaryMetaInitializer[] metaInitializers, 
			                         IDictionaryInitializer[] initializers, IDictionary<String, PropertyDescriptor> properties, 
			                         IDictionaryAdapterFactory factory)
		{
			Type = type;
			Factory = factory;
			Behaviors = behaviors;
			MetaInitializers = metaInitializers;
			Initializers = initializers;
			Properties = properties;

			InitializeMeta();
		}

		public Type Type { get; private set; }

		public object[] Behaviors { get; private set; }

		public IDictionaryAdapterFactory Factory { get; private set; }

		public IDictionary<string, PropertyDescriptor> Properties { get; private set; }

		public IDictionaryMetaInitializer[] MetaInitializers { get; private set; }

		public IDictionaryInitializer[] Initializers { get; private set; }

		public IDictionary ExtendedProperties
		{
			get
			{
				if (extendedProperties == null)
				{
					extendedProperties = new Dictionary<object, object>();
				}
				return extendedProperties;
			}
		}

		private void InitializeMeta()
		{
			foreach (var metaInitializer in MetaInitializers)
			{
				metaInitializer.Initialize(Factory, this);
			}
		}
	}
}
