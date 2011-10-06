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

#if !SL3
namespace Castle.Components.DictionaryAdapter.Xml
{
	using System;
	using System.Xml.XPath;

	public class XmlXPathBehaviorAccessor : XmlAccessor, IXmlIncludedType, IXmlIncludedTypeMap,
		IConfigurable<XPathAttribute>,
		IConfigurable<XPathVariableAttribute>,
		IConfigurable<XPathFunctionAttribute>
	{
		private XmlAccessor itemAccessor;
		private XmlIncludedTypeSet includedTypes;
	    private CompiledXPath path;

		internal static readonly XmlAccessorFactory<XmlXPathBehaviorAccessor>
			Factory = (name, type, context) => new XmlXPathBehaviorAccessor(type, context);

	    protected XmlXPathBehaviorAccessor(Type type, IXmlContext context)
	        : base(type, context)
		{
			includedTypes = new XmlIncludedTypeSet();

			foreach (var includedType in context.GetIncludedTypes(type))
				includedTypes.Add(includedType);
		}

	    public CompiledXPath Path
	    {
	        get { return path; }
	    }

		public bool SelectsNodes
		{
			get { return path.Path.ReturnType == XPathResultType.NodeSet; }
		}

		XmlName IXmlIncludedType.XsiType
		{
			get { return XmlName.Empty; }
		}

		IXmlIncludedType IXmlIncludedTypeMap.Default
		{
			get { return this; }
		}

		public void Configure(XPathAttribute attribute)
		{
			if (path != null)
				throw Error.AttributeConflict(path.Path.Expression);

			path = attribute.Path;
		}

		public void Configure(XPathVariableAttribute attribute)
		{
			CloneContext().AddVariable(attribute);
		}

		public void Configure(XPathFunctionAttribute attribute)
		{
			CloneContext().AddFunction(attribute);
		}

		public override void Prepare()
		{
			Context.Enlist(path);
		}

		public override object GetPropertyValue(IXmlNode node, IDictionaryAdapter da, bool ifExists)
		{
			return SelectsNodes
				? base.GetPropertyValue(node, da, ifExists)
				: Evaluate(node);
		}

		public override void SetPropertyValue(IXmlNode node, IDictionaryAdapter da, ref object value)
		{
			if (SelectsNodes)
				base.SetPropertyValue(node, da, ref value);
			else
				throw Error.NotSupported();
		}

		private object Evaluate(IXmlNode node)
		{
			var value = node.Evaluate(path);
			return Convert.ChangeType(value, ClrType);
		}

		public override IXmlCollectionAccessor GetCollectionAccessor(Type itemType)
		{
			return itemAccessor ?? (itemAccessor = new ItemAccessor(this));
		}

		public override IXmlCursor SelectPropertyNode(IXmlNode node, bool create)
		{
			var flags = CursorFlags.AllNodes.MutableIf(create);
			return node.Select(path, this, Context, flags);
		}

		public override IXmlCursor SelectCollectionNode(IXmlNode node, bool create)
		{
			return node.SelectSelf(ClrType);
		}

		public override IXmlCursor SelectCollectionItems(IXmlNode node, bool create)
		{
			return node.Select(path, this, Context, CursorFlags.AllNodes.MutableIf(create) | CursorFlags.Multiple);
		}

		public bool TryGet(XmlName xsiType, out IXmlIncludedType includedType)
		{
			if (xsiType == XmlName.Empty || xsiType == this.XsiType)
				return Try.Success(out includedType, this);

			if (!includedTypes.TryGet(xsiType, out includedType))
				return false;

			if (!ClrType.IsAssignableFrom(includedType.ClrType))
				return Try.Failure(out includedType);

			return true;
		}

		public bool TryGet(Type clrType, out IXmlIncludedType includedType)
		{
			return clrType == this.ClrType
				? Try.Success(out includedType, this)
				: includedTypes.TryGet(clrType, out includedType);
		}

		private class ItemAccessor : XmlXPathBehaviorAccessor
		{
			public ItemAccessor(XmlXPathBehaviorAccessor parent)
				: base(parent.ClrType.GetCollectionItemType(), parent.Context)
			{
				path          = parent.path;
				includedTypes = parent.includedTypes;

				ConfigureNillable(true);
			}

			public override IXmlCollectionAccessor GetCollectionAccessor(Type itemType)
			{
				return GetDefaultCollectionAccessor(itemType);
			}
		}
	}
}
#endif
