// Copyright 2004-2005 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.ActiveRecordScaffold.Tests
{
	using System;
	using System.IO;

	using NUnit.Framework;

	using Castle.ActiveRecord;
	using Castle.ActiveRecord.Framework;

	using Castle.MonoRail.Engine.Tests;

	using TestScaffolding.Model;


	[TestFixture]
	public class ListActionTestCase : AbstractCassiniTestCase
	{
		[Test]
		public void ListBlog()
		{
			TestDBUtils.Recreate();
			
			string url = "/blogs/listblog.rails";
			string expected = "My View contents for Home\\Index";

			Execute(url, expected);
		}

		protected override String ObtainPhysicalDir()
		{
			return Path.Combine( AppDomain.CurrentDomain.BaseDirectory, @"..\TestScaffolding" );
		}
	}
}
