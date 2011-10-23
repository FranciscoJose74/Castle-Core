// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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
// 
namespace Castle.Components.Binder.Tests
{
	using System.Collections.Specialized;
	using NUnit.Framework;

	[TestFixture]
	public class TreeBuilderRegressionTestCase
	{
		[Test]
		public void IndexedContent()
		{
			var args = new NameValueCollection();

			args.Add("customer[0].name", "hammett");
			args.Add("customer[0].age", "26");
			args.Add("customer[0].all", "yada yada yada");
			args.Add("customer[10].name", "frasier");
			args.Add("customer[10].age", "50");
			args.Add("customer[10].all", "yada");

			var builder = new TreeBuilder();

			CompositeNode root = builder.BuildSourceNode(args);

			var node = (IndexedNode) root.GetChildNode("customer");

			Assert.IsNotNull(node);
			Assert.AreEqual(2, node.ChildrenCount);

			var cnode = (CompositeNode) node.GetChildNode("0");

			Assert.AreEqual("hammett", ((LeafNode) cnode.GetChildNode("name")).Value);
			Assert.AreEqual("26", ((LeafNode) cnode.GetChildNode("age")).Value);
			Assert.AreEqual("yada yada yada", ((LeafNode) cnode.GetChildNode("all")).Value);

			cnode = (CompositeNode) node.GetChildNode("10");

			Assert.AreEqual("frasier", ((LeafNode) cnode.GetChildNode("name")).Value);
			Assert.AreEqual("50", ((LeafNode) cnode.GetChildNode("age")).Value);
			Assert.AreEqual("yada", ((LeafNode) cnode.GetChildNode("all")).Value);
		}

		[Test]
		public void NoValidEntries()
		{
			var args = new NameValueCollection();

			args.Add("customername", "x");
			args.Add("customerage", "x");
			args.Add("customerall", "x");

			var builder = new TreeBuilder();

			CompositeNode root = builder.BuildSourceNode(args);
			Assert.IsNull(root.GetChildNode("customer"));
		}

		[Test]
		public void OneLevelNode()
		{
			var args = new NameValueCollection();

			args.Add("customer.name", "hammett");
			args.Add("customer.age", "26");
			args.Add("customer.all", "yada yada yada");

			var builder = new TreeBuilder();

			CompositeNode root = builder.BuildSourceNode(args);

			var node = (CompositeNode) root.GetChildNode("customer");

			Assert.IsNotNull(node);

			Assert.AreEqual("hammett", ((LeafNode) node.GetChildNode("name")).Value);
			Assert.AreEqual("26", ((LeafNode) node.GetChildNode("age")).Value);
			Assert.AreEqual("yada yada yada", ((LeafNode) node.GetChildNode("all")).Value);
		}

		[Test]
		public void TwoLevels()
		{
			var args = new NameValueCollection();

			args.Add("customer.name", "hammett");
			args.Add("customer.age", "26");
			args.Add("customer.location.code", "pt-br");
			args.Add("customer.location.country", "55");

			var builder = new TreeBuilder();

			CompositeNode root = builder.BuildSourceNode(args);
			Assert.IsNotNull(root);

			var node = (CompositeNode) root.GetChildNode("customer");
			Assert.IsNotNull(root);

			var locationNode = (CompositeNode) node.GetChildNode("location");
			Assert.IsNotNull(locationNode);

			Assert.AreEqual("pt-br", ((LeafNode) locationNode.GetChildNode("code")).Value);
			Assert.AreEqual("55", ((LeafNode) locationNode.GetChildNode("country")).Value);
		}
	}
}