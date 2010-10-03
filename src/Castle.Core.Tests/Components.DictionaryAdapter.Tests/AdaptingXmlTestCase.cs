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

namespace Castle.Components.DictionaryAdapter.Tests
{
#if !SILVERLIGHT
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.IO;
	using System.Linq;
	using System.Xml;
	using System.Xml.Schema;
	using System.Xml.Serialization;
	using NUnit.Framework;

	[TestFixture]
	public class AdaptinXmlTestCase
	{
		private DictionaryAdapterFactory factory;

		[SetUp]
		public void SetUp()
		{
			factory = new DictionaryAdapterFactory();
		}

		[Test]
		public void Factory_ForXml_CreatesTheAdapter()
		{
			XmlDocument document = null;
			var season = CreateXmlAdapter<ISeason>(null, ref document);
			Assert.IsNotNull(season);
		}

		[Test]
		public void Adapter_OnXml_Will_Preserve_References()
		{
			var line1 = "2922 South Highway 205";
			var city = "Rockwall";
			var state = "TX";
			var zipCode = "75032";

			var xml = string.Format(
				@"<Season xmlns='RISE'>
					 <Address xmlns='Common'>
						<Line1>{0}</Line1>
						<City>{1}</City>
						<State>{2}</State>
						<ZipCode>{3}</ZipCode>
					 </Address>
				  </Season>",
				line1, city, state, zipCode);

			XmlDocument document = null;
			var season = CreateXmlAdapter<ISeason>(xml, ref document);
			Assert.AreSame(season.Location, season.Location);
			Assert.AreEqual(line1, season.Location.Line1);

			var seasonNode = document["Season", "RISE"];
			var addressNode = seasonNode["Address", "Common"];
			var line1Node = addressNode["Line1", "Common"];
			line1Node.InnerText = "1234 Tulip";
			Assert.AreEqual("1234 Tulip", season.Location.Line1);
		}

		[Test]
		public void Adapter_OnXml_CanTargetWithXPath()
		{
			var line1 = "2922 South Highway 205";
			var city = "Rockwall";
			var state = "TX";
			var zipCode = "75032";

			var xml = string.Format(
				@"<Season xmlns='RISE'>
					 <Address xmlns='Common'>
						<Line1>{0}</Line1>
						<City>{1}</City>
						<State>{2}</State>
						<ZipCode>{3}</ZipCode>
					 </Address>
				  </Season>",
				line1, city, state, zipCode);

			XmlDocument document = null;
			var address = CreateXmlAdapter<IAddress>(xml, ref document);
			Assert.AreEqual(line1, address.Line1);
		}

		[Test]
		public void Adapter_OnXml_CanReadProperties()
		{
			var name = "Soccer Adult Winter II 2010";
			var minAge = 30;
			var division = Division.Coed;
			var startsOn = new DateTime(2010, 2, 21);
			var endsOn = new DateTime(2010, 4, 18);
			var line1 = "2922 South Highway 205";
			var city = "Rockwall";
			var state = "TX";
			var zipCode = "75032";
			var team1Name = "Fairly Oddparents";
			var team1Balance = 450.00M;
			var team1Player1FirstName = "Mike";
			var team1Player1LastName = "Etheridge";
			var team1Player2FirstName = "Susan";
			var team1Player2LastName = "Houston";
			var team2Name = "Team Punishment";
			var team2Balance = 655.50M;
			var team2Player1FirstName = "Stephen";
			var team2Player1LastName = "Gray";
			var licenseNo = "1234";
			var tags = new[] { "Soccer", "Skills", "Fun" };

			var xml = string.Format(
				@"<Season xmlns='RISE' xmlns:rise='RISE'>
					 <Name>{0}</Name>
					 <MinimumAge>{1}</MinimumAge>
					 <Division>{2}</Division>
					 <StartsOn>{3}</StartsOn>
					 <EndsOn>{4}</EndsOn>
					 <Address xmlns='Common'>
						<Line1>{5}</Line1>
						<City>{6}</City>
						<State>{7}</State>
						<ZipCode>{8}</ZipCode>
					 </Address>
					 <League>
						<Team name='{9}'>
						   <AmountDue>{10}</AmountDue>
						   <Roster>
							  <Participant FirstName='{11}' lastName='{12}'>
							  </Participant>
							  <Participant FirstName='{13}' lastName='{14}'>
							  </Participant>
						   </Roster>
						</Team>
						<Team name='{15}'>
						   <AmountDue>{16}</AmountDue>
						   <Roster>
							  <Participant FirstName='{17}' lastName='{18}'>
							  </Participant>
						   </Roster>
						</Team>
					 </League>
					 <Tag>{19}</Tag>
					 <Tag>{20}</Tag>
					 <Tag>{21}</Tag>
					 <ExtraStuff>
						<LicenseNo>{22}</LicenseNo>
					 </ExtraStuff>
				  </Season>",
				name, minAge, division,
				XmlConvert.ToString(startsOn, XmlDateTimeSerializationMode.Local),
				XmlConvert.ToString(endsOn, XmlDateTimeSerializationMode.Local),
				line1, city, state, zipCode,
				team1Name, XmlConvert.ToString(team1Balance),
				team1Player1FirstName, team1Player1LastName,
				team1Player2FirstName, team1Player2LastName,
				team2Name, XmlConvert.ToString(team2Balance),
				team2Player1FirstName, team2Player1LastName,
				tags[0], tags[1], tags[2],
				licenseNo);

			XmlDocument document = null;
			var season = CreateXmlAdapter<ISeason>(xml, ref document);
			Assert.AreEqual(name, season.Name);
			Assert.AreEqual(minAge, season.MinimumAge);
			Assert.AreEqual(division, season.Division);
			Assert.AreEqual(startsOn.Date, season.StartsOn.Date);
			Assert.AreEqual(endsOn.Date, season.EndsOn.Date);
			Assert.AreEqual(line1, season.Location.Line1);
			Assert.AreEqual(city, season.Location.City);
			Assert.AreEqual(state, season.Location.State);
			Assert.AreEqual(zipCode, season.Location.ZipCode);
			Assert.AreEqual(2, season.Teams.Count);

			var team = season.Teams[0];
			var n = team.Name;

			Assert.AreEqual(team1Name, season.Teams[0].Name);
			Assert.AreEqual(team1Balance, season.Teams[0].Balance);
			Assert.IsNull(season.Teams[0].GamesPlayed);
			Assert.AreEqual(2, season.Teams[0].Players.Count);
			Assert.AreEqual(team1Player1FirstName, season.Teams[0].Players[0].FirstName);
			Assert.AreEqual(team1Player1LastName, season.Teams[0].Players[0].LastName);
			Assert.AreEqual(team1Player2FirstName, season.Teams[0].Players[1].FirstName);
			Assert.AreEqual(team1Player2LastName, season.Teams[0].Players[1].LastName);
			Assert.AreEqual(team2Name, season.Teams[1].Name);
			Assert.AreEqual(team2Balance, season.Teams[1].Balance);
			Assert.AreEqual(1, season.Teams[1].Players.Count);
			Assert.AreEqual(team2Player1FirstName, season.Teams[1].Players[0].FirstName);
			Assert.AreEqual(team2Player1LastName, season.Teams[1].Players[0].LastName);
			Assert.AreEqual(2, season.TeamsArray.Length);
			Assert.AreEqual(team1Name, season.TeamsArray[0].Name);
			Assert.AreEqual(team1Balance, season.TeamsArray[0].Balance);
			Assert.AreEqual(team2Name, season.TeamsArray[1].Name);
			Assert.AreEqual(team2Balance, season.TeamsArray[1].Balance);
			Assert.AreEqual(team1Balance + team2Balance, season.Balance);
			Assert.AreEqual(3, season.Tags.Length);
			Assert.Contains(tags[0], season.Tags);
			Assert.Contains(tags[1], season.Tags);
			Assert.Contains(tags[2], season.Tags);
			Assert.IsNotNull(season.ExtraStuff);
			Assert.AreEqual(licenseNo, season.ExtraStuff["LicenseNo", "RISE"].InnerText);
		}

		[Test]
		public void Adapter_OnXml_CanWriteProperties()
		{
			var name = "Soccer Adult Winter II 2010";
			var minAge = 30;
			var division = Division.Coed;
			var startsOn = new DateTime(2010, 2, 21);
			var endsOn = new DateTime(2010, 4, 18);
			var line1 = "2922 South Highway 205";
			var city = "Rockwall";
			var state = "TX";
			var zipCode = "75032";
			var team1Name = "Fairly Oddparents";
			var team1Balance = 450.00M;
			var team3Name = "Barcelona";
			var team3Balance = 175.15M;

			var xml = string.Format(
				@"<Season xmlns='RISE' xmlns:rise='RISE'>
					 <Name>Soccer Adult Spring II 2010</Name>
					 <MinimumAge>16</MinimumAge>
					 <Division>Male</Division>
					 <StartsOn>{0}</StartsOn>
					 <EndsOn>{1}</EndsOn>
					 <League>
						<Team name='Hit And Run'>
						   <AmountDue>100.50</AmountDue>
						</Team>
						<Team name='Nemisis'>
						   <AmountDue>250.00</AmountDue>
						</Team>
					 </League>
					 <ExtraStuff>
						<LicenseNo>9999</LicenseNo>
					 </ExtraStuff>
				  </Season>",
				XmlConvert.ToString(new DateTime(2010, 7, 19), XmlDateTimeSerializationMode.Local),
				XmlConvert.ToString(new DateTime(2010, 9, 20), XmlDateTimeSerializationMode.Local));

			XmlDocument document = null;
			var season = CreateXmlAdapter<ISeason>(xml, ref document);
			season.Name = name;
			season.MinimumAge = minAge;
			season.Division = division;
			season.StartsOn = startsOn;
			season.EndsOn = endsOn;
			Assert.IsNotNull(season.Location);
			Assert.IsNull(document.DocumentElement["Address", "Common"]);
			season.Location.Line1 = line1;
			season.Location.City = city;
			season.Location.State = state;
			season.Location.ZipCode = zipCode;
			season.Teams[0].Name = team1Name;
			season.Teams[0].Balance = team1Balance;
			var team3 = season.Teams.AddNew();
			team3.Name = team3Name;
			team3.Balance = team3Balance;

			Assert.AreEqual(name, season.Name);
			Assert.AreEqual(minAge, season.MinimumAge);
			Assert.AreEqual(division, season.Division);
			Assert.AreEqual(startsOn.Date, season.StartsOn.Date);
			Assert.AreEqual(endsOn.Date, season.EndsOn.Date);
			Assert.AreEqual(line1, season.Location.Line1);
			Assert.AreEqual(city, season.Location.City);
			Assert.AreEqual(state, season.Location.State);
			Assert.AreEqual(zipCode, season.Location.ZipCode);
			Assert.AreEqual(3, season.Teams.Count);
			Assert.AreEqual(team1Name, season.Teams[0].Name);
			Assert.AreEqual(team1Balance, season.Teams[0].Balance);
			Assert.AreEqual(team3Name, season.Teams[2].Name);
			Assert.AreEqual(team3Balance, season.Teams[2].Balance);

			season.Teams.RemoveAt(1);
			Assert.AreEqual(2, season.Teams.Count);
			Assert.AreEqual(team3Name, season.Teams[1].Name);
			Assert.AreEqual(team3Balance, season.Teams[1].Balance);
		}

		[Test]
		public void Adapter_OnXml_CanCopySubTree()
		{
			var xml = string.Format(
				@"<Season xmlns='RISE' xmlns:rise='RISE'>
					 <Name>Soccer Adult Spring II 2010</Name>
					 <MinimumAge>16</MinimumAge>
					 <Division>Male</Division>
					 <StartsOn>{0}</StartsOn>
					 <EndsOn>{1}</EndsOn>
					 <Address xmlns='Common'>
						<Line1>2922 South Highway 205</Line1>
						<City>Rockwall</City>
						<State>TX</State>
						<ZipCode>75032</ZipCode>
					 </Address>
					 <League>
						<Team name='Hit And Run'>
						   <AmountDue>100.50</AmountDue>
						   <Roster>
							  <Participant FirstName='Mickey' lastName='Mouse'>
							  </Participant>
							  <Participant FirstName='Donald' lastName='Ducks'>
							  </Participant>
						   </Roster>
						</Team>
						<Team name='Nemisis'>
						   <AmountDue>250.00</AmountDue>
						</Team>
					 </League>
					 <Tag>Soccer</Tag>
					 <Tag>Cheetahs</Tag>
					 <Tag>Hot Shots</Tag>
					 <ExtraStuff>
						<LicenseNo>9999</LicenseNo>
					 </ExtraStuff>
				  </Season>",
				XmlConvert.ToString(new DateTime(2010, 7, 19), XmlDateTimeSerializationMode.Local),
				XmlConvert.ToString(new DateTime(2010, 9, 20), XmlDateTimeSerializationMode.Local));

			XmlDocument document1 = null;
			XmlDocument document2 = null;
			var season1 = CreateXmlAdapter<ISeason>(xml, ref document1);
			var season2 = CreateXmlAdapter<ISeason>(null, ref document2);
			season2.Location = season1.Location;
			season2.Tags = season1.Tags;
			season2.Teams = season1.Teams;
			var player = season2.Teams[1].Players.AddNew();
			player.FirstName = "Dave";
			player.LastName = "O'Hara";
			season1.Teams[0].Players[1] = player;

			Assert.AreNotSame(season1.Location, season2.Location);
			Assert.AreEqual(season1.Location.Line1, season2.Location.Line1);
			Assert.AreEqual(season1.Location.City, season2.Location.City);
			Assert.AreEqual(season1.Location.State, season2.Location.State);
			Assert.AreEqual(season1.Location.ZipCode, season2.Location.ZipCode);
			Assert.AreEqual(season1.Tags.Length, season2.Tags.Length);
			Assert.AreEqual(season2.Tags[0], season1.Tags[0]);
			Assert.AreEqual(season2.Tags[1], season1.Tags[1]);
			Assert.AreEqual(season2.Tags[2], season1.Tags[2]);
			Assert.AreEqual(season2.Teams.Count, season1.Teams.Count);
			Assert.AreEqual(season2.Teams[0].Name, season1.Teams[0].Name);
			Assert.AreEqual(season2.Teams[0].Balance, season1.Teams[0].Balance);
			Assert.AreEqual(season2.Teams[0].Players.Count, season2.Teams[0].Players.Count);
			Assert.AreEqual(season2.Teams[0].Players[0].FirstName, season1.Teams[0].Players[0].FirstName);
			Assert.AreEqual(season2.Teams[0].Players[0].LastName, season1.Teams[0].Players[0].LastName);
			Assert.AreEqual(season2.Teams[1].Name, season1.Teams[1].Name);
			Assert.AreEqual(season2.Teams[1].Balance, season1.Teams[1].Balance);
			Assert.AreEqual(player.FirstName, season1.Teams[0].Players[1].FirstName);
			Assert.AreEqual(player.LastName, season1.Teams[0].Players[1].LastName);

			season2.Location = null;
			season2.Tags = null;
			Assert.AreEqual(0, season2.Tags.Length);
		}

		[Test]
		public void Adapter_OnXml_CanCreate_Other_Adapter()
		{
			var xml = @"<Season xmlns='RISE' xmlns:rise='RISE'>
					 <Name>Soccer Adult Spring II 2010</Name>
					 <MinimumAge>16</MinimumAge>
					 <Division>Male</Division>
					 <League>
						<Team name='Hit And Run'>
						   <AmountDue>100.50</AmountDue>
						</Team>
						<Team name='Nemisis'>
						   <AmountDue>250.00</AmountDue>
						</Team>
					 </League>
				  </Season>";

			XmlDocument document = null;
			var season = CreateXmlAdapter<ISeason>(xml, ref document);
			var address = season.Create<IAddress>();
			Assert.IsNull(address.Line1);
		}

		[Test]
		public void Will_Use_Interface_Name_Without_I_When_Writing()
		{
			XmlDocument document = null;
			var team = CreateXmlAdapter<ITeam>(null, ref document);
			team.Name = "Turfmonsters";
			Assert.AreEqual("Team", document.DocumentElement.LocalName);
			Assert.AreEqual("", document.DocumentElement.NamespaceURI);
		}

		[Test]
		public void Will_Use_TypeName_From_XmlTypeAttribute_When_Writing()
		{
			XmlDocument document = null;
			var player = CreateXmlAdapter<IPlayer>(null, ref document);
			player.FirstName = "Joe";
			Assert.AreEqual("Player", document.DocumentElement.LocalName);
			Assert.AreEqual("", document.DocumentElement.NamespaceURI);
		}

		[Test]
		public void Will_Use_ElementName_And_Namesace_From_XmlRootAttribute_When_Writing()
		{
			XmlDocument document = null;
			var season = CreateXmlAdapter<ISeason>(null, ref document);
			season.Name = "Coed Summer";
			Assert.AreEqual("Season", document.DocumentElement.LocalName);
			Assert.AreEqual("RISE", document.DocumentElement.NamespaceURI);
		}

		private T CreateXmlAdapter<T>(string xml, ref XmlDocument document)
		{
			document = document ?? new XmlDocument();
			if (xml != null)
			{
				document.LoadXml(xml);
			}
			return factory.GetAdapter<T>(document);
		}

		public enum Division
		{
			Male,
			Female,
			Coed
		}

		[XmlNamespace("Common", "common"),
		 XPath("common:Address")]
		public interface IAddress
		{
			string Line1 { get; set; }
			string City { get; set; }
			string State { get; set; }
			string ZipCode { get; set; }
		}

		[XmlType("Player", Namespace = "People")]
		public interface IPlayer
		{
			string FirstName { get; set; }
			string LastName { get; set; }
		}

		[XmlType("Goalie", Namespace = "People")]
		public interface IGoalie : IPlayer
		{
			int GoalAllowed { get; set; }
		}

		public interface ITeam
		{
			[XmlAttribute]
			string Name { get; set; }
			int? GamesPlayed { get; set; }
			[XmlElement("AmountDue")]
			decimal Balance { get; set; }
			[XmlArray("Roster"), XmlArrayItem("Participant")]
			BindingList<IPlayer> Players { get; }
		}

		[XmlRoot("Season", Namespace = "RISE"),
		 XmlNamespace("RISE", "rise", Default = true)]
		public interface ISeason : IDictionaryAdapter
		{
			string Name { get; set; }
			int MinimumAge { get; set; }
			Division Division { get; set; }
			DateTime StartsOn { get; set; }
			DateTime EndsOn { get; set; }
			[XPath("sum(rise:League/rise:Team/rise:AmountDue)")]
			decimal Balance { get; }
			[XmlElement("Address", Namespace = "Common")]
			IAddress Location { get; set; }
			[Key("League"), XmlArrayItem("Team")]
			BindingList<ITeam> Teams { get; set; }
			[XPath("rise:League/rise:Team")]
			ITeam[] TeamsArray { get; }
			[XmlElement("Tag")]
			string[] Tags { get; set; }
			XmlElement ExtraStuff { get; set; }
		}

		[Test]
		public void Can_Read_From_Standard_Xml_Serialization()
		{
			var manager = new Manager { Name = "Craig", Level = 1 };
			var employee = new Employee
			{
				Name = "Dave",
				Supervisor = manager,
				Job = new Employment { Title = "Consultant", Salary = 100000M },
				Metadata = new Metadata { Tag = "Cool!" }
			};
			var group = new Group
			{
				Id = 2,
				Owner = manager,
				Employees = new Employee[] { employee, manager },
				Tags = new[] { "Primary", "Local" },
				Codes = Enumerable.Range(1, 5).ToList(),
				Comment = "Nothing important",
				ExtraInfo = new object[] { 43, "Extra", manager }
			};

			using (var stream = new FileStream("out.xml", FileMode.Create))
			{
				var serializer = new XmlSerializer(typeof(Group));
				serializer.Serialize(stream, group);
				stream.Flush();
			}

			var document = new XmlDocument();
			document.Load("out.xml");

			var groupRead = CreateXmlAdapter<IGroup>(null, ref document);
			Assert.AreEqual(2, groupRead.Id);
			Assert.IsInstanceOf<IManager>(groupRead.Owner);
			var managerRead = (IManager)groupRead.Owner;
			Assert.AreEqual(manager.Name, managerRead.Name);
			Assert.AreEqual(manager.Level, managerRead.Level);
			var employeesRead = groupRead.Employees;
			Assert.AreEqual(2, employeesRead.Length);
			Assert.AreEqual(employee.Name, employeesRead[0].Name);
			Assert.AreEqual(employee.Job.Title, employeesRead[0].Job.Title);
			Assert.AreEqual(employee.Job.Salary, employeesRead[0].Job.Salary);
			Assert.AreEqual(employee.Metadata.Tag, employeesRead[0].Metadata.Tag);
			Assert.AreEqual(manager.Name, employeesRead[1].Name);
			Assert.IsInstanceOf<IManager>(employeesRead[1]);
			var managerEmplRead = (IManager)employeesRead[1];
			Assert.AreEqual(manager.Level, managerEmplRead.Level);
			CollectionAssert.AreEqual(group.Tags, groupRead.Tags);
			var extraInfoRead = groupRead.ExtraInfo;
			Assert.AreEqual(3, extraInfoRead.Length);
			Assert.AreEqual(group.ExtraInfo[0], extraInfoRead[0]);
			Assert.AreEqual(group.ExtraInfo[1], extraInfoRead[1]);
			Assert.IsInstanceOf<IManager>(extraInfoRead[2]);
			var managerExtra = (IManager)extraInfoRead[2];
			Assert.AreEqual(manager.Name, managerExtra.Name);
			Assert.AreEqual(manager.Level, managerExtra.Level);

			groupRead.Comment = "Hello World";
			Assert.AreEqual("Hello World", groupRead.Comment);
			var commentRead = document["Comment", "Yum"];
			Assert.IsNull(commentRead);
		}

		[Test]
		public void Can_Write_To_Standard_Xml_Serialization()
		{
			XmlDocument document = null, mgr = null, emp = null;
			var manager = CreateXmlAdapter<IManager>(null, ref mgr);
			manager.Name = "Craig";
			manager.Level = 1;

			var employee = CreateXmlAdapter<IEmployee>(null, ref emp);
			employee.Name = "Dave";
			employee.Supervisor = manager;
			employee.Job = new Employment
			{
				Title = "Consultant",
				Salary = 100000M
			};
			employee.Metadata = new Metadata { Tag = "Cool!" };

			var group = CreateXmlAdapter<IGroup>(null, ref document);
			group.Id = 2;
			group.Owner = manager;
			group.Employees = new IEmployee[] { employee, manager };
			group.Tags = new[] { "Primary", "Local" };
			group.Codes = Enumerable.Range(1, 5).ToList();
			group.Comment = "Nothing important";
			group.ExtraInfo = new object[] { 43, "Extra", manager };

			document.Save("out.xml");

			using (var stream = new FileStream("out.xml", FileMode.Open))
			{
				var serializer = new XmlSerializer(typeof(Group));
				var groupRead = (Group)serializer.Deserialize(stream);

				Assert.AreEqual(2, groupRead.Id);
				Assert.IsInstanceOf<Manager>(groupRead.Owner);
				var managerRead = (Manager)groupRead.Owner;
				Assert.AreEqual(manager.Name, managerRead.Name);
				Assert.AreEqual(manager.Level, managerRead.Level);
				var employeesRead = groupRead.Employees;
				Assert.AreEqual(2, employeesRead.Length);
				Assert.AreEqual(employee.Name, employeesRead[0].Name);
				Assert.AreEqual(employee.Job.Title, employeesRead[0].Job.Title);
				Assert.AreEqual(employee.Job.Salary, employeesRead[0].Job.Salary);
				Assert.AreEqual(employee.Metadata.Tag, employeesRead[0].Metadata.Tag);
				Assert.AreEqual(manager.Name, employeesRead[1].Name);
				Assert.IsInstanceOf<Manager>(employeesRead[1]);
				var managerEmplRead = (Manager)employeesRead[1];
				Assert.AreEqual(manager.Level, managerEmplRead.Level);
				CollectionAssert.AreEqual(group.Tags, groupRead.Tags);
				Assert.IsNull(groupRead.Comment);
				Assert.AreEqual(3, groupRead.ExtraInfo.Length);
				Assert.AreEqual(group.ExtraInfo[0], groupRead.ExtraInfo[0]);
				Assert.AreEqual(group.ExtraInfo[1], groupRead.ExtraInfo[1]);
				Assert.IsInstanceOf<Manager>(groupRead.ExtraInfo[2]);
				var managerExtra = (Manager)groupRead.ExtraInfo[2];
				Assert.AreEqual(manager.Name, managerExtra.Name);
				Assert.AreEqual(manager.Level, managerExtra.Level);
			}
		}
	}

	#region DA Serialization Model

	[XmlType(TypeName = "Groupy", Namespace = "Yum"),
	 XmlRoot(ElementName = "GroupRoot", Namespace = "Arg")]
	public interface IGroup
	{
		int Id { get; set; }
		IEmployee Owner { get; set; }

		[XmlArrayItem("Dude", Type = typeof(IEmployee)),
		 XmlArrayItem(Type = typeof(IManager))]
		IEmployee[] Employees { get; set; }

		string[] Tags { get; set; }

		IList<int> Codes { get; set; }

		[XmlIgnore]
		string Comment { get; set; }

		[XmlArrayItem(typeof(int), ElementName = "MyNumber"),
		 XmlArrayItem(typeof(string), ElementName = "MyString"),
		 XmlArrayItem(typeof(IManager))]
		object[] ExtraInfo { get; set; }
	}

	//[XmlRoot(ElementName = "FooRoot")]
	[XmlType(TypeName = "Foo", Namespace = "Something"),
	 XmlInclude(typeof(IManager))]
	public interface IEmployee
	{
		string Name { get; set; }
		IEmployee Supervisor { get; set; }
		Employment Job { get; set; }
		Metadata Metadata { get; set; }
	}

	//[XmlRoot(ElementName = "BarRoot")]
	[XmlType(TypeName = "Bar", Namespace = "Nothing")]
	public interface IManager : IEmployee
	{
		int Level { get; set; }
	}

	#endregion

	#region Xml Serialization Model

	[XmlType(TypeName = "Groupy", Namespace = "Yum"),
	 XmlRoot(ElementName = "GroupRoot", Namespace = "Arg")]
	public class Group
	{
		public int Id;
		public Employee Owner;

		[XmlArrayItem("Dude", Type = typeof(Employee)),
		 XmlArrayItem(Type = typeof(Manager))]
		public Employee[] Employees;

		public string[] Tags;

		public List<int> Codes;

		[XmlIgnore]
		public string Comment;

		[XmlArray,
		 XmlArrayItem(typeof(int), ElementName = "MyNumber"),
		 XmlArrayItem(typeof(string), ElementName = "MyString"),
		 XmlArrayItem(typeof(Manager))]
		public object[] ExtraInfo;
	}

	//[XmlRoot(ElementName = "FooRoot")]
	[XmlType(TypeName="Foo", Namespace = "Something"),
	 XmlInclude(typeof(Manager))]
	public class Employee
	{
		public string Name;
		public Employee Supervisor;
		public Employment Job;
		public Metadata Metadata;
	}

	//[XmlRoot(ElementName = "BarRoot")]
	[XmlType(TypeName = "Bar", Namespace = "Nothing")]
	public class Manager : Employee
	{
		public int Level;
	}

	[XmlType(Namespace = "Potato"),
	 XmlRoot(Namespace = "Pickle")]
	public class Employment
	{
		public string Title { get; set; }

		public decimal Salary { get; set; }
	}

	public class Metadata : IXmlSerializable
	{
		public string Tag { get; set; }

		public void ReadXml(XmlReader reader)
		{
			reader.MoveToContent();
			var isEmptyElement = reader.IsEmptyElement;
			reader.ReadStartElement();
			if (isEmptyElement == false) 
			{
				Tag = reader.ReadElementString("Tag");
				reader.ReadEndElement();
			}
		}

		public void WriteXml(XmlWriter writer)
		{
			if (string.IsNullOrEmpty(Tag) == false)
			{
				writer.WriteElementString("Tag", Tag);
			}
		}

		public XmlSchema GetSchema()
		{
			return null;
		}
	}

	#endregion
#endif
}
