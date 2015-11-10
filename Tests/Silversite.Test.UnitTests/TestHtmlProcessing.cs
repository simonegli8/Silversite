using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Silversite.Html {
	/// <summary>
	/// Summary description for Html
	/// </summary>
	[TestClass]
	public class TestHtmlProcessing {
		public TestHtmlProcessing() {
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext {
			get {
				return testContextInstance;
			}
			set {
				testContextInstance = value;
			}
		}

	

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion


		[TestMethod]
		public void ServerTags() {
			//arrange
			const string TestHtml1 = @"<%@ Page Title='Test' Language='C#' AutoEventWireup='true' CodeBehind='admins.aspx.cs' Inherits='Silversite.silversite.admin.admins' %>";
			const string TestHtml2 = @"<% Page; Title='Test'; Inherits='Silversite.silversite.admin.admins'; %>";
			const string TestHtml3 = @"<%$ Page(); %>";
			const string TestHtml4 = @"<%= Page();  %>";
			const string TestHtml5 = @"<%# Page(); %>";
			const string TestHtml6 = @"<%: Page(); %>";

			var tag1 = new Html.ServerTag();
			tag1.Text = TestHtml1;
			var text1 = tag1.Text;
			var tag2 = new Html.ServerTag();
			tag2.Text = TestHtml2;
			var text2 = tag2.Text;
			var tag3 = new Html.ServerTag();
			tag3.Text = TestHtml3;
			var text3 = tag3.Text;
			var tag4 = new Html.ServerTag();
			tag4.Text = TestHtml4;
			var text4 = tag4.Text;
			var tag5 = new Html.ServerTag();
			tag5.Text = TestHtml5;
			var text5 = tag5.Text;
			var tag6 = new Html.ServerTag();
			tag6.Text = TestHtml6;
			var text6 = tag6.Text; 

			Assert.AreEqual(5, tag1.Attributes.Count);
			Assert.AreEqual(TestHtml1, text1);
			Assert.AreEqual(TestHtml2, text2);
			Assert.AreEqual(TestHtml3, text3);
			Assert.AreEqual(TestHtml4, text4);
			Assert.AreEqual(TestHtml5, text5);
			Assert.AreEqual(TestHtml6, text6);
		}

		[TestMethod]
		public void Comments() {
			//arrange
			const string TestHtml = @"<%@ Page Title='Test' Language='C#' AutoEventWireup='true' <%-- comment --%> CodeBehind='admins.aspx.cs' Inherits='Silversite.silversite.admin.admins' %> <%-- server comment --%>";

			var tag = new Html.ServerTag();
			tag.Text = TestHtml;
			var text = tag.Text;

			Assert.AreEqual(5, tag.Attributes.Count);
			Assert.AreEqual(TestHtml, text);
		}


		[TestMethod]
		public void Element() {
			//arrange
			const string TestHtml = @"<div runat='server'>Test<span style='color:red;'>Red Test</span></div>";

			var e = new Html.Element();
			e.Text = TestHtml;
			var text = e.Text;
			var span = (Html.Element)e.Children[1];
			var spantext = span.Children.Text;
			var testtext = span.Children[1].Text;

			Assert.AreEqual("Red Test", spantext);
			Assert.AreEqual("div", e.Name); 
			Assert.AreEqual(true, e.IsServerControl);
			Assert.AreEqual(TestHtml, text);
		}

		[TestMethod]
		public void Script() {
			//arrange
			const string TestHtml = "<div runat='server'>\nTest\n<span style='color:red;'>\nRed Test\n<script runat='server'>\n alert('hello');  \n<html im script />\n</script>\n</span>\n</div>";

			var e = new Html.Element();
			e.Text = TestHtml;
			var text = e.Text;
			var script = e.Find<Html.Script>();

			Assert.AreEqual("div", e.Name);
			Assert.AreEqual("\n alert('hello');  \n<html im script />\n", script.Value);
			Assert.AreEqual(true, e.IsServerControl);
			Assert.AreEqual(TestHtml, text);
		}

		[TestMethod]
		public void CData() {
			//arrange
			const string TestHtml = "<div runat='server'>\nTest\n<span style='color:red;'>\nRed Test\n<![CDATA[\n alert('hello');  \n<html im script />]]>\n</span>\n</div>";

			var e = new Html.Element();
			e.Text = TestHtml;
			var text = e.Text;
			var cdata = e.Find<Html.Literal>(n => n.LiteralToken.Class == TokenClass.CData);

			Assert.AreEqual("\n alert('hello');  \n<html im script />", cdata.Value);
			Assert.AreEqual(true, e.IsServerControl);
			Assert.AreEqual(TestHtml, text);
		}

		[TestMethod]
		public void Document() {
			//arrange
			const string TestHtml = @"
				<%@ Page Title='Test' Language='C#' AutoEventWireup='true' CodeBehind='admins.aspx.cs' Inherits='Silversite.silversite.admin.admins' %>
				<html>
					<head>
						<title>Test</title>
					</head>
					<body>
						<form runat='server'>
							<div id='test'>
								<asp:Calendar id='calendar'>
								</asp:Calendar>
							</div>
						</form>
					</body>
				</html>
			";


			// act
			var doc = new Document();
			doc.Text = TestHtml;
			var resHtml = doc.Text;

			var err = doc.Errors;

			// assert
			Assert.AreEqual(doc.Children.Count, 2);

			var html = (Html.Element)doc.Children[1];
			var body = (Html.Element)html.Children[1];
			var form = (Html.Element)body.Children[0];

			Assert.AreEqual("html", html.Name);
			Assert.AreEqual(2, html.Children.Count);


			Assert.AreEqual(doc.Body, body);
			Assert.AreEqual(doc.Form, form);

			Assert.AreEqual(TestHtml, resHtml);

		}
	}
}
