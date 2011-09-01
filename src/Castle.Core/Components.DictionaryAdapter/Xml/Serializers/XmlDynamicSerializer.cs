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
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.f
// See the License for the specific language governing permissions and
// limitations under the License.

#if !SILVERLIGHT
namespace Castle.Components.DictionaryAdapter.Xml
{
	public class XmlDynamicSerializer : XmlTypeSerializer
	{
		public static readonly XmlDynamicSerializer
			Instance = new XmlDynamicSerializer();

		protected XmlDynamicSerializer() { }

		public override object GetValue(XmlTypedNode node, IDictionaryAdapter parent, IXmlAccessor accessor)
		{
			return node.Type == typeof(object)
				? new object()
				: XmlTypeSerializer.For(node.Type).GetValue(node, parent, accessor);
		}

		public override void SetValue(XmlTypedNode node, IXmlAccessor accessor, object value)
		{
			if (node.Type != typeof(object))
				XmlTypeSerializer.For(node.Type).SetValue(node, accessor, value);
		}
	}
}
#endif
