﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.Components.DictionaryAdapter.Xml
{
	using System;
using System.Collections;

	public static class DictionaryAdapterExtensions
	{
		public static DictionaryAdapterMeta GetDictionaryAdapterMeta(this DictionaryAdapterMeta source, Type type)
		{
			var descriptor = new DictionaryDescriptor();
			descriptor.AddInitializers    (source.Initializers);
			descriptor.AddMetaInitializers(source.MetaInitializers);

			return source.Factory.GetAdapterMeta(type, descriptor);
		}

		public static object CreateChildAdapter(this IDictionaryAdapter parent, Type type, XmlAdapter adapter)
		{
			return CreateChildAdapter(parent, type, adapter, null);
		}

		public static object CreateChildAdapter(this IDictionaryAdapter parent, Type type, XmlAdapter adapter, IDictionary dictionary)
		{
			if (null == dictionary)
				dictionary = new Hashtable();

		    var descriptor = new DictionaryDescriptor();
		    parent.This.Descriptor.CopyBehaviors(descriptor);
		    descriptor.AddBehavior(adapter);

		    return parent.This.Factory.GetAdapter(type, dictionary, descriptor);
		}
	}
}
