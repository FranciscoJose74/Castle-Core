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

namespace Castle.Components.DictionaryAdapter.Xml
{
	using System;
	using System.Linq;
	using System.Xml;
	using System.Xml.Serialization;

	public class XmlAttributeBehaviorAccessor : XmlNodeAccessor,
		IConfigurable<XmlAttributeAttribute>
	{
		private string localName;
		private string namespaceUri;
		private XmlKnownTypeSet knownTypes;
		private bool configured;

		internal static readonly XmlAccessorFactory<XmlAttributeBehaviorAccessor>
			Factory = (property, knownTypes) => new XmlAttributeBehaviorAccessor(property, knownTypes);

		public XmlAttributeBehaviorAccessor(PropertyDescriptor property, IXmlKnownTypeMap knownTypes)
			: base(property.PropertyType, knownTypes)
		{
			this.localName = XmlConvert.EncodeLocalName(property.PropertyName);
		}

		public override string LocalName
		{
			get { return localName; }
		}

		public override string NamespaceUri
		{
			get { return namespaceUri; }
		}

		public override IXmlKnownTypeMap KnownTypes
		{
			get { return (IXmlKnownTypeMap) knownTypes ?? base.KnownTypes; }
		}

		public void Configure(XmlAttributeAttribute attribute)
		{
			if (attribute.Type == null)
			{
				if (configured)
					throw Error.AttributeConflict(null);
				if (!string.IsNullOrEmpty(attribute.AttributeName))
					localName = attribute.AttributeName;
				if (!string.IsNullOrEmpty(attribute.Namespace))
					namespaceUri = attribute.Namespace;
				configured = true;
			}
			else
			{
				if (knownTypes == null)
					knownTypes = new XmlKnownTypeSet(ClrType);
				knownTypes.Add(attribute);
			}
		}

		public override void Prepare()
		{
			if (knownTypes != null &&
				knownTypes.Any(t => t.ClrType == null))
				knownTypes.Parent = this;
		}

		public override IXmlCollectionAccessor GetCollectionAccessor(Type itemType)
		{
			throw Error.NotSupported();
		}

		public override IXmlCursor SelectPropertyNode(IXmlNode node, bool mutable)
		{
			return node.SelectChildren(KnownTypes, CursorFlags.Attributes.MutableIf(mutable));
		}

		public override IXmlCursor SelectCollectionNode(IXmlNode node, bool mutable)
		{
			throw Error.NotSupported();
		}

		public override IXmlCursor SelectCollectionItems(IXmlNode node, bool mutable)
		{
			throw Error.NotSupported();
		}
	}
}
