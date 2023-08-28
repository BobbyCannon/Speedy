#region References

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation;
using Speedy.Automation.Desktop;
using Speedy.Automation.Tests;
using Speedy.Automation.Web;
using Speedy.Automation.Web.Browsers;
using Speedy.Automation.Web.Elements;
using Speedy.Extensions;
using Speedy.UnitTests;

#endregion

namespace Speedy.AutomationTests.Web
{
	[TestClass]
	public class BrowserTests
	{
		#region Constructors

		static BrowserTests()
		{
			DefaultTimeout = TimeSpan.FromMilliseconds(2000);
			TestSite = "https://speedy.local";
		}

		#endregion

		#region Properties

		public static bool CleanupBrowsers { get; set; }

		public static TimeSpan DefaultTimeout { get; set; }

		public static string TestSite { get; }

		#endregion

		#region Methods

		[TestMethod]
		public void AngularInputTrigger()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/Angular.html");

				var email = browser.First<TextInput>("email");
				email.SendKeys("user", true);

				var expected = "ng-valid-parse ng-untouched ng-scope ng-invalid ng-not-empty ng-dirty ng-invalid-email ng-valid-required".Split(' ');
				var actual = email.GetAttributeValue("class", true).Split(' ');
				TestHelper.AreEqual("user", email.Text);
				TestHelper.AreEqual(expected, actual);

				email.SendKeys("@domain.com");
				expected = "ng-valid-parse ng-untouched ng-scope ng-not-empty ng-dirty ng-valid-required ng-valid ng-valid-email".Split(' ');
				actual = email.GetAttributeValue("class", true).Split(' ');
				TestHelper.AreEqual("user@domain.com", email.Text);
				TestHelper.AreEqual(expected, actual);
			});
		}

		[TestMethod]
		public void AngularNewElementCount()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/Angular.html");

				var span = browser.First<Span>("items-length");
				var button = browser.First<Button>("addItem");
				button.Click();

				var count = 0;
				var result = span.Wait(x =>
				{
					count++;
					var text = ((Span) x).Text;
					return text == "1";
				});

				Assert.IsTrue(count > 1, "We should have tested text more than once.");
				Assert.IsTrue(result, "The count never incremented to 1.");
			});
		}

		[TestMethod]
		public void AngularNewElements()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/Angular.html");
				var elementCount = browser.Descendants().Count();

				var button = browser.First<Button>("addItem");
				button.Click();

				var newItem = browser.FirstOrDefault("items-0");
				Assert.IsNotNull(newItem, "Never found the new item.");

				Assert.AreEqual(elementCount + 1, browser.Descendants().Count());
				elementCount = browser.Descendants().Count();

				button.Click();

				newItem = browser.FirstOrDefault("items-1");
				Assert.IsNotNull(newItem, "Never found the new item.");

				Assert.AreEqual(elementCount + 1, browser.Descendants().Count());
			});
		}

		[TestMethod]
		public void AngularSetTextInputs()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/Angular.html#!/form");
				Assert.AreEqual("true", browser.First<Button>("saveButton").Disabled);
				browser.First<TextInput>("pageTitle").Text = "Hello World";
				Assert.AreEqual("Hello World", browser.First<TextInput>("pageTitle").Text);
				browser.First<TextArea>("pageText").Text = "The quick brown fox jumps over the lazy dog's back.";
				Assert.AreEqual("The quick brown fox jumps over the lazy dog's back.", browser.First<TextArea>("pageText").Text);
				Assert.AreEqual("false", browser.First<Button>("saveButton").Disabled);
			});
		}

		[TestMethod]
		public void AngularSwitchPageByNavigateTo()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/Angular.html#!/");
				Assert.AreEqual(TestSite + "/Angular.html#!/", browser.Uri);

				Assert.IsTrue(browser.Contains("addItem"));
				Assert.IsTrue(browser.Contains("anotherPageLink"));

				browser.NavigateTo(TestSite + "/Angular.html#!/anotherPage");
				Assert.AreEqual(TestSite + "/Angular.html#!/anotherPage", browser.Uri);

				Assert.IsFalse(browser.Contains("addItem"));
				Assert.IsTrue(browser.Contains("pageLink"));

				browser.NavigateTo(TestSite + "/Angular.html#!/");
				Assert.AreEqual(TestSite + "/Angular.html#!/", browser.Uri);

				Assert.IsTrue(browser.Contains("addItem"));
				Assert.IsTrue(browser.Contains("anotherPageLink"));
			});
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			if (CleanupBrowsers)
			{
				Browser.CloseBrowsers();
			}
		}

		[ClassInitialize]
		public static void ClassInitialize(TestContext context)
		{
			if (CleanupBrowsers)
			{
				Browser.CloseBrowsers();
			}
		}

		[TestMethod]
		public void ClickButton()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				Assert.AreEqual("Text Area's \"Quotes\" Data", browser.First<TextArea>("textarea").Text);
				browser.First("button").Click();
				Assert.AreEqual("button", browser.First<TextArea>("textarea").Text);

				browser.NavigateTo(TestSite + "/vue.html");
				Assert.AreEqual("", browser.First<Division>("error").Text);
				browser.First<Button>("click").Click();
				Assert.AreEqual("click", browser.First<Division>("error").Text);
			});
		}

		[TestMethod]
		public void ClickButtonByName()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				browser.First(x => x.Name == "buttonByName").Click();

				var textArea = browser.First<TextArea>("textarea");
				Assert.AreEqual("buttonByName", textArea.Text);
			});
		}

		[TestMethod]
		public void ClickCheckbox()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/inputs.html");

				foreach (var element in browser.Descendants<CheckBox>())
				{
					element.Click();
					Assert.IsTrue(element.Checked);
					element.Click();
					Assert.IsFalse(element.Checked);
				}
			});
		}

		[TestMethod]
		public void ClickInputButton()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				browser.First<Button>("inputButton").Click();

				var textArea = browser.First<TextArea>("textarea");
				Assert.AreEqual("inputButton", textArea.Text);
			});
		}

		[TestMethod]
		public void ClickLink()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				browser.First("link").Click();

				var textArea = browser.First<TextArea>("textarea");
				Assert.AreEqual("link", textArea.Text);
			});
		}

		[TestMethod]
		public void ClickRadioButton()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/inputs.html");

				Assert.IsFalse(browser.Descendants<RadioButton>().Any(x => x.Checked));

				foreach (var element in browser.Descendants<RadioButton>())
				{
					element.Click();
					Assert.IsTrue(element.Checked);
					Assert.IsTrue(browser.Descendants<RadioButton>().Where(x => x.Id == element.Id).All(x => x.Checked));
					Assert.IsTrue(browser.Descendants<RadioButton>().Where(x => x.Id != element.Id).All(x => !x.Checked));
				}
			});
		}

		[TestMethod]
		public void Close()
		{
			ForAllBrowsers(browser =>
			{
				Assert.IsFalse(browser.IsClosed, "The browser should not be closed.");
				browser.Close();
				Assert.IsTrue(browser.IsClosed, "The browser should be closed but was not.");
			});
		}

		[TestMethod]
		public void DeepDescendants()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/relationships.html");
				Assert.AreEqual(4043, browser.Descendants().Count());

				foreach (var e in browser.Descendants())
				{
					Console.WriteLine(e.Id);
				}
			});
		}

		[TestMethod]
		public void DetectAngularJavaScriptLibrary()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/Angular.html#!/");
				Assert.IsTrue(browser.JavascriptLibraries.Contains(JavaScriptLibrary.Angular));
			});
		}

		[TestMethod]
		public void DetectBootstrap2JavaScriptLibrary()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/Bootstrap2.html");
				Assert.IsTrue(browser.JavascriptLibraries.Contains(JavaScriptLibrary.Bootstrap2));
			});
		}

		[TestMethod]
		public void DetectBootstrap3JavaScriptLibrary()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/Bootstrap3.html");
				Assert.IsTrue(browser.JavascriptLibraries.Contains(JavaScriptLibrary.Bootstrap3));
			});
		}

		[TestMethod]
		public void DetectJQueryJavaScriptLibrary()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/JQuery.html");
				Assert.IsTrue(browser.JavascriptLibraries.Contains(JavaScriptLibrary.JQuery));
			});
		}

		[TestMethod]
		public void DetectMomentJavaScriptLibrary()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/Moment.html");
				Assert.IsTrue(browser.JavascriptLibraries.Contains(JavaScriptLibrary.Moment));
			});
		}

		[TestMethod]
		public void DetectNoJavaScriptLibrary()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/Inputs.html");
				Assert.AreEqual(0, browser.JavascriptLibraries.Count());
			});
		}

		[TestMethod]
		public void DetectVueJavaScriptLibrary()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/Vue.html");
				Assert.IsTrue(browser.JavascriptLibraries.Contains(JavaScriptLibrary.Vue));
			});
		}

		[TestMethod]
		public void ElementChildren()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/relationships.html");
				Assert.AreEqual(4043, browser.Descendants().Count());
				var children = browser.First("parent1div").Children;

				var expected = new[] { "child1div", "child2span", "child3br", "child4input" };
				Assert.AreEqual(4, children.Count);
				TestHelper.AreEqual(expected.ToList(), children.Select(x => x.Id).ToList());
			});
		}

		[TestMethod]
		public void ElementParent()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/relationships.html");
				var element = browser.First("child1div").Parent;
				Assert.AreEqual("parent1div", element.Id);
			});
		}

		[TestMethod]
		public void Enabled()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/inputs.html");
				Assert.AreEqual(true, browser.First<Button>("button").Enabled);
				Assert.AreEqual(false, browser.First<Button>("button2").Enabled);
			});
		}

		[TestMethod]
		public void EnumerateDivisions()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				var elements = browser.Descendants<Division>().ToList();
				Assert.AreEqual(2, elements.Count());

				foreach (var division in elements)
				{
					division.Highlight(true);
				}
			});
		}

		[TestMethod]
		public void EnumerateHeaders()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				var elements = browser.Descendants<Header>().ToList();
				Assert.AreEqual(6, elements.Count());

				foreach (var header in elements)
				{
					header.Text = "Header!";
				}
			});
		}

		[TestMethod]
		public void FilterElementByTextElements()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/inputs.html");
				var inputs = browser.Descendants<TextInput>().ToList();
				// Old IE treats input types of Date, Month, Week as "Text" which increases input count.
				//var expected = browser.BrowserType == BrowserType.InternetExplorer ? 11 : 8;
				Assert.AreEqual(8, inputs.Count);
			});
		}

		[TestMethod]
		public void FindElementByClass()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				var elements = browser.Descendants()
					.Cast<WebElement>()
					.Where(x => x.Class.Contains("red"));
				Assert.AreEqual(1, elements.Count());
			});
		}

		[TestMethod]
		public void FindElementByClassByValueAccessor()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				var elements = browser.Descendants()
					.Cast<WebElement>().Where(x => x["class"].Contains("red"));
				Assert.AreEqual(1, elements.Count());
			});
		}

		[TestMethod]
		public void FindElementByClassProperty()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				var elements = browser.Descendants<Link>().Where(x => x.Class == "bold");
				Assert.AreEqual(1, elements.Count());
			});
		}

		[TestMethod]
		public void FindElementByDataAttribute()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");

				var link = browser.Descendants(x => x["data-test"] == "testAnchor").FirstOrDefault();

				Assert.IsNotNull(link, "Failed to find the link by data attribute.");
				Assert.AreEqual(link.Id, "a1");
			});
		}

		[TestMethod]
		public void FindHeadersByText()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				var elements = browser.Descendants<Header>().Where(x => x.Text.Contains("Header"));
				Assert.AreEqual(6, elements.Count());
			});
		}

		[TestMethod]
		public void FindSpanElementByText()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				var elements = browser.Descendants<Span>(x => x.Text == "SPAN with ID of 1");
				Assert.AreEqual(1, elements.Count());
			});
		}

		[TestMethod]
		public void FindTextInputsByText()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				var elements = browser.Descendants<TextInput>().Where(x => x.Text == "Hello World");
				Assert.AreEqual(1, elements.Count());
			});
		}

		[TestMethod]
		public void Focus()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/inputs.html");

				var expected = browser.Descendants<TextInput>().Last();
				expected.Focus();
				Assert.IsNotNull(browser.ActiveElement, "There should be an active element.");
				Assert.AreEqual(expected.Id, browser.ActiveElement.Id);
			});
		}

		[TestMethod]
		public void FocusEvent()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");

				var expected = browser.First<TextInput>("text");
				expected.Focus();
				Assert.IsNotNull(browser.ActiveElement, "There should be an active element.");
				Assert.AreEqual(expected.Id, browser.ActiveElement.Id);
				Assert.AreEqual("focused", expected.Text);
			});
		}

		[TestMethod]
		public void ForAllBrowserExceptions()
		{
			Browser.CloseBrowsers();
			var expectedCount = TestHelper.BrowsersToTests.Count();

			try
			{
				ForAllBrowsers(x => throw new Exception(x.BrowserType.ToString()));
			}
			catch (AggregateException ex)
			{
				Assert.AreEqual(expectedCount, ex.InnerExceptions.Count);

				var index = 0;

				if (index < expectedCount && TestHelper.BrowsersToTests.HasFlag(BrowserType.Chrome))
				{
					Assert.AreEqual("Test failed using Chrome.", ex.InnerExceptions[index++].Message);
				}

				if (index < expectedCount && TestHelper.BrowsersToTests.HasFlag(BrowserType.Edge))
				{
					Assert.AreEqual("Test failed using Edge.", ex.InnerExceptions[index++].Message);
				}

				//Assert.AreEqual("Test failed using Firefox.", ex.InnerExceptions[2].Message);
			}
			catch (Exception ex)
			{
				Assert.Fail("Invalid exception: " + ex.GetType().FullName);
			}
			finally
			{
				Browser.CloseBrowsers();
			}
		}

		[TestMethod]
		public void ForAllBrowserShouldTimeoutFast()
		{
			try
			{
				ForAllBrowsers(x => Thread.Sleep(5000), timeout: 1000);
			}
			catch (Exception ex)
			{
				Assert.AreEqual("ForAllBrowsers has timed out.", ex.Message);
			}
		}

		[TestMethod]
		public void FormWithSubInputsWithNamesOfFormAttributeNames()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/forms.html");
				Assert.AreEqual(10, browser.Descendants().Count());
				Assert.AreEqual(10, browser.Descendants<WebElement>().Count());

				var form = browser.First<Form>("form");
				Assert.AreEqual("form", form.Id);
				Assert.AreEqual("form", form.Name);
				Assert.AreEqual("form", form.TagName);
				Assert.AreEqual("form", form.GetAttributeValue("id", true));

				var input = browser.First<TextInput>("id");
				Assert.AreEqual("id", input.Id);
				Assert.AreEqual("id", input.Name);
				Assert.AreEqual("input", input.TagName);
				Assert.AreEqual("id", input.GetAttributeValue("id", true));

				input = browser.First<TextInput>("name");
				Assert.AreEqual("name", input.Id);
				Assert.AreEqual("name", input.Name);
				Assert.AreEqual("input", input.TagName);
				Assert.AreEqual("name", input.GetAttributeValue("id", true));
			});
		}

		[TestMethod]
		public void FormWithSubInputsWithNamesOfFormAttributeNamesButMainFormHasNoId()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/forms2.html");
				Assert.AreEqual(10, browser.Descendants().Count());
				Assert.AreEqual(10, browser.Descendants<WebElement>().Count());

				var form = browser.First<Form>("form");
				Assert.AreEqual("form", form.Name);
				Assert.AreEqual("form", form.TagName);

				var input = browser.First<TextInput>("id");
				Assert.AreEqual("id", input.Id);
				Assert.AreEqual("id", input.Name);
				Assert.AreEqual("input", input.TagName);
				Assert.AreEqual("id", input.GetAttributeValue("id", true));

				input = browser.First<TextInput>("name");
				Assert.AreEqual("name", input.Id);
				Assert.AreEqual("name", input.Name);
				Assert.AreEqual("input", input.TagName);
				Assert.AreEqual("name", input.GetAttributeValue("id", true));
			});
		}

		[TestMethod]
		public void GetAndSetHtml()
		{
			ForAllBrowsers(browser =>
			{
				var guid = Guid.NewGuid().ToString();
				browser.NavigateTo(TestSite + "/main.html");

				var button = browser.FirstOrDefault<Button>("button");
				Assert.IsNotNull(button);
				var actual = browser.GetHtml();
				Assert.IsFalse(actual.Contains(guid));

				var expected = $"<html><head><link href=\"https://speedy.local/Content/speedy.css\" rel=\"stylesheet\"></head><body>{guid}</body></html>";
				browser.SetHtml(expected);
				actual = browser.GetHtml();
				actual.Dump();
				Assert.AreEqual(expected, actual);
			});
		}

		[TestMethod]
		public void GetAndSetHtmlForElement()
		{
			ForAllBrowsers(browser =>
			{
				var guid = Guid.NewGuid().ToString();
				var input = $"Special characters like \", ', \0, \", \n, \r, \r\n, <, >, should not break html... however &amp; must be already encoded! \n{guid}";
				browser.NavigateTo(TestSite + "/main.html");

				var body = browser.FirstOrDefault<Body>();
				Assert.IsNotNull(body);
				var original = body.GetHtml();
				body.SetHtml(input);
				var actual = body.GetHtml();

				Assert.AreNotEqual(original, actual);
				Assert.IsTrue(actual.Contains(guid));
			});
		}

		[TestMethod]
		public void GetAndSetHtmlOfCodeBlock()
		{
			ForAllBrowsers(browser =>
			{
				var guid = Guid.NewGuid().ToString();
				browser.NavigateTo(TestSite + "/main.html");

				var button = browser.FirstOrDefault<Button>("button");
				Assert.IsNotNull(button);
				var actual = browser.GetHtml();
				Assert.IsFalse(actual.Contains(guid));

				var expected = $"<html>\r\n<head>\r\n\t<link href=\"https://speedy.local/Content/speedy.css\" rel=\"stylesheet\">\r\n</head>\r\n<body>\r\n\t{guid}\r\n<pre><code>\r\naoeu\r\nblah\r\n\r\n\'testing\'\r\n</code></pre>\r\n</body>\r\n</html>";
				browser.SetHtml(expected);
				actual = browser.GetHtml();
				Assert.IsTrue(actual.Contains(guid));
			});
		}

		[TestMethod]
		public void GetAndSetHtmlOnAboutBlankPage()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo("about:blank");
				var actual = browser.GetHtml();
				Assert.IsTrue((actual == "<html id=\"speedy-1\"><head id=\"speedy-2\"></head><body id=\"speedy-3\"></body></html>") || (actual == ""));

				var guid = Guid.NewGuid().ToString();
				var expected = $"<html><head><link href=\"https://speedy.local/Content/speedy.css\" rel=\"stylesheet\"></head><body>{guid}</body></html>";
				browser.SetHtml(expected);
				Assert.AreEqual(expected, browser.GetHtml());

				guid = Guid.NewGuid().ToString();
				expected = $"<html><head></head><body>{guid}</body></html>";
				browser.SetHtml(expected);
				Assert.AreEqual(expected, browser.GetHtml());
			});
		}

		[TestMethod]
		public void GetAndSetStyle()
		{
			ForAllBrowsers(browser =>
				//ForEachBrowser(browser =>
			{
				browser.NavigateTo(TestSite + "/invalid.html");

				var span1 = browser.FirstOrDefault<WebElement>("span1");
				Assert.IsNotNull(span1);
				Assert.AreEqual("", span1.Style);
				Assert.AreEqual("", span1.GetAttributeValue("Style", true));
				span1.Style = "color: red";

				var actual = span1.GetStyleAttributeValue("color", true);
				Assert.AreEqual("red", actual);
				Assert.IsTrue(span1.GetAttributeValue("Style", true).Contains("color: red"));

				var input1 = browser.FirstOrDefault<WebElement>("input1");
				Assert.IsNotNull(input1);
				Assert.AreEqual("", input1.Style);
				Assert.AreEqual("", input1.GetAttributeValue("Style", true));
				input1.Style = "color: red";

				actual = input1.GetStyleAttributeValue("color", true);
				Assert.AreEqual("red", actual);
				Assert.IsTrue(input1.GetAttributeValue("Style", true).Contains("color: red"));
			});
		}

		[TestMethod]
		public void GetElementByNameIndex()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				var actual = browser.First(x => x.Name == "inputName").Name;
				Assert.AreEqual("inputName", actual);
			});
		}

		[TestMethod]
		public void GetNextSibling()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/Angular.html#!/");

				var items = browser.First<Division>("items");
				Assert.AreEqual(0, items.Children.Count);

				browser.First<Button>("addItem").Click().Click();
				var result = items.Wait(x => x.Refresh().Children.Count >= 2);
				Assert.IsTrue(result, "Should have had two items.");
				Assert.AreEqual(2, items.Children.Count);

				var item0 = items.First("items-0");
				var item1 = item0.GetNextSibling();

				Assert.IsNotNull(item1, "Failed to find sibling.");
				Assert.AreEqual("items-1", item1.Id);
			});
		}

		[TestMethod]
		public void GetPreviousSibling()
		{
			ForEachBrowser(browser =>
			{
				browser.NavigateTo(TestSite + "/Angular.html#!/");

				var items = browser.First<Division>("items");
				Assert.AreEqual(0, items.Children.Count);

				browser.First<Button>("addItem").Click().Click();
				var result = items.Wait(x => x.Refresh().Children.Count >= 2);
				Assert.IsTrue(result, "Should have had two items.");
				Assert.AreEqual(2, items.Children.Count);

				var item1 = items.First("items-1");
				var item0 = item1.GetPreviousSibling();

				Assert.AreEqual("items-0", item0.Id);
			});
		}

		[TestMethod]
		public void HighlightAllElements()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				var elements = browser.OfType<WebElement>();

				foreach (var element in elements)
				{
					var originalColor = element.GetStyleAttributeValue("background-color");
					element.Highlight(true);
					Assert.AreEqual("yellow", element.GetStyleAttributeValue("background-color"));
					element.Highlight(false);
					Assert.AreEqual(originalColor, element.GetStyleAttributeValue("background-color"));
				}
			});
		}

		[TestMethod]
		public void HighlightElement()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/inputs.html");

				var inputElements = browser.Descendants<WebElement>(t => t.TagName == "input").ToList();
				foreach (var element in inputElements)
				{
					var originalColor = element.GetStyleAttributeValue("background-color");
					element.Highlight(true);
					Assert.AreEqual("yellow", element.GetStyleAttributeValue("background-color"));
					element.Highlight(false);
					Assert.AreEqual(originalColor, element.GetStyleAttributeValue("background-color"));
				}
			});
		}

		[TestMethod]
		public void IframesShouldHaveAccess()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/iframe.html");
				browser.First<TextInput>("id").SendKeys("hello", true);

				var frame = browser.First("frame");
				var email = frame.First<TextInput>("email");
				email.SendKeys("world", true);

				Assert.AreEqual("world", email.Text);
				Assert.AreEqual("world", email.GetAttributeValue("value"));
			});
		}

		[TestMethod]
		public void InternetOfBing()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo("https://www.bing.com");
				// New bing needs delay
				Thread.Sleep(1000);
				browser.Descendants().ToList().Count.Dump();
				browser.FirstOrDefault("sb_form_q").LeftClick();
				Input.Keyboard.SendInput("Bobby Cannon Epic Coders");
				browser.FirstOrDefault("sb_form_go").Click();
				browser.WaitForComplete();
			});
		}

		[TestMethod]
		public void Location()
		{
			ForEachBrowser(browser =>
			{
				browser.NavigateTo("about:blank");
				browser.NavigateTo(TestSite + "/main.html");
				var button = browser.First("button");
				button.Location.Dump();
				button.LeftClick();

				var result = Utility.Wait(() => "button".Equals(browser.First<TextArea>("textarea").Text), 5000, 100);
				Assert.IsTrue(result, "The text should have been button but was not.");
			});
		}

		[TestMethod]
		public void MiddleClickButton()
		{
			ForEachBrowser(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				var button = browser.First("button");
				button.MiddleClick();
				browser.WaitForComplete(150);

				// Middle click may not click but does set focus.
				Assert.IsTrue(button.Focused);
				Input.Mouse.LeftButtonClick(button.Location);
			});
		}

		[TestMethod]
		public void NavigateThenRunJavascript()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");

				var actual = browser.ExecuteScript("Speedy.runScript('document.toString()')");
				Assert.IsTrue(actual.Contains("undefined"));

				actual = browser.ExecuteScript("document.title");
				Assert.AreEqual("Index", actual);
			});
		}

		[TestMethod]
		public void NavigateTo()
		{
			ForAllBrowsers(browser =>
			{
				var expected = TestSite + "/main.html";
				browser.NavigateTo(expected);
				Assert.AreEqual(expected, browser.Uri.ToLower());
			});
		}

		[TestMethod]
		public void NavigateToFromHttpToHttps()
		{
			ForAllBrowsers(browser =>
			{
				var expected = TestSite.Replace("https://", "http://") + "/main.html";
				Assert.IsFalse(expected.StartsWith("https://"));
				browser.NavigateTo(expected);
				Assert.IsTrue(browser.Uri.StartsWith("https://"));
				Assert.AreEqual($"{TestSite}/main.html", browser.Uri.ToLower());
			});
		}

		[TestMethod]
		public void NavigateToFromHttpToHttpsWhenAlreadyOnExpectedUri()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo($"{TestSite}/main.html");
				var expected = TestSite.Replace("https://", "http://") + "/main.html";
				Assert.IsFalse(expected.StartsWith("https://"));
				browser.NavigateTo(expected);
				Assert.IsTrue(browser.Uri.StartsWith("https://"), browser.Uri);
				Assert.AreEqual($"{TestSite}/main.html", browser.Uri.ToLower());
			});
		}

		[TestMethod]
		public void NavigateToSameUri()
		{
			ForAllBrowsers(browser =>
			{
				var expected = TestSite + "/main.html";
				browser.NavigateTo(expected);
				browser.NavigateTo(expected);
				browser.NavigateTo(expected);
				Assert.AreEqual(expected, browser.Uri.ToLower());
			});
		}

		[TestMethod]
		public void NavigateToSameUriWithEndingForwardSlash()
		{
			ForAllBrowsers(browser =>
			{
				var expected = TestSite + "/login/";
				browser.NavigateTo(expected);
				browser.NavigateTo(expected);
				browser.NavigateTo(expected);
				Assert.AreEqual($"{expected}", browser.Uri.ToLower());
			});
		}

		[TestMethod]
		public void NavigateToWithDifferentFinalUri()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/Angular.html");
				Assert.AreEqual(TestSite + "/Angular.html#!/", browser.Uri);
			});
		}

		[TestMethod]
		public void RawHtml()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/Jquery.html");
				var test = browser.RawHtml.Trim();
				Assert.IsTrue(test.StartsWith("<html"));
				Assert.IsTrue(test.EndsWith("</html>"));
			});
		}

		[TestMethod]
		public void RedirectByLink()
		{
			ForAllBrowsers(browser =>
			{
				var expected = TestSite + "/main.html";
				browser.NavigateTo(expected);
				Assert.AreEqual(expected, browser.Uri.ToLower());

				// Redirect by the link.
				browser.First<Link>("redirectLink").Click();
				browser.WaitForNavigation(TestSite + "/Inputs.html");

				expected = TestSite + "/inputs.html";
				Assert.AreEqual(expected, browser.Uri.ToLower());
			});
		}

		[TestMethod]
		public void RedirectByLinkWithoutProvidingExpectedUri()
		{
			ForAllBrowsers(browser =>
			{
				var expected = TestSite + "/main.html";
				browser.NavigateTo(expected);
				Assert.AreEqual(expected, browser.Uri.ToLower());

				// Redirect by the link.
				browser.First<Link>("redirectLink").Click();
				browser.WaitForNavigation();

				expected = TestSite + "/inputs.html";
				Assert.AreEqual(expected, browser.Uri.ToLower());
			});
		}

		[TestMethod]
		public void RedirectByScript()
		{
			ForAllBrowsers(browser =>
			{
				var expected = TestSite + "/main.html";
				browser.NavigateTo(expected);
				Assert.AreEqual(expected, browser.Uri.ToLower());
				Assert.IsNotNull(browser.First("link"), "Failed to find the link element.");

				// Redirect by a script.
				browser.ExecuteScript("window.location.href = 'inputs.html'");
				browser.WaitForNavigation(TestSite + "/inputs.html");

				expected = TestSite + "/inputs.html";
				Assert.AreEqual(expected, browser.Uri.ToLower());
			});
		}

		[TestMethod]
		public void RedirectByScriptWithoutProvidedExpectedUri()
		{
			ForAllBrowsers(browser =>
			{
				var expected = TestSite + "/main.html";
				browser.NavigateTo(expected);
				Assert.AreEqual(expected, browser.Uri.ToLower());
				Assert.IsNotNull(browser.First("link"), "Failed to find the link element.");

				// Redirect by a script.
				browser.ExecuteScript("setTimeout(function() {window.location.href = 'inputs.html'}, 1000)");
				browser.WaitForNavigation();

				expected = TestSite + "/inputs.html";
				Assert.AreEqual(expected, browser.Uri.ToLower());
			});
		}

		[TestMethod]
		public void Refresh()
		{
			ForAllBrowsers(browser =>
			{
				var expected = TestSite + "/angular.html";
				browser.NavigateTo(expected);
				Assert.AreEqual(33, browser.Descendants().Count());

				browser.First("addItem").Click();
				Assert.AreEqual(33, browser.Descendants().Count());

				var result = browser.Wait(x =>
				{
					x.Refresh();
					return x.Descendants().Count() == 34;
				});
				Assert.IsTrue(result, "The count never incremented.");
			});
		}

		[TestMethod]
		public void RightClickButton()
		{
			ForEachBrowser(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				var element = browser.First("logo1");
				element.RightClick();

				var location = element.Location;
				element.Focus();
				location.Dump();

				var clickX = location.X + (element.Width > 0 ? element.Width / 2 : 0);
				var clickY = location.Y + (element.Height > 0 ? element.Height / 2 : 0);

				Input.Mouse.MoveTo(clickX + 60, clickY + 22);
				Thread.Sleep(1000);

				// had to add "pane" as a valid option because FF uses a custom control now.
				var result = Utility.Wait(() => (DesktopElement.FromCursor()?.TypeName.Equals("menu item", StringComparison.OrdinalIgnoreCase) == true)
					|| (DesktopElement.FromCursor()?.TypeName.Equals("pane", StringComparison.OrdinalIgnoreCase) == true), 1000, 50);

				Assert.IsTrue(result, "Failed to find menu.");
				Input.Mouse.LeftButtonClick(element.Location);
			});
		}

		[TestMethod]
		public void ScrollIntoView()
		{
			ForEachBrowser(browser =>
			{
				var location = browser.Location;
				var size = browser.Size;

				try
				{
					browser.NavigateTo(TestSite + "/main.html");
					browser.MoveWindow(0, 0, 400, 300);
					browser.BringToFront();

					var button = browser.First<WebElement>("button");
					button.ScrollIntoView();
					Assert.IsTrue(button.Location.X < 120, $"x:{button.Location.X} should be less than 100.");
					Assert.IsTrue(button.Location.Y < 120, $"y:{button.Location.Y} should be less than 100.");
				}
				finally
				{
					browser.MoveWindow(location, size);
				}
			}, resizeBrowsers: false);
		}

		[TestMethod]
		public void SelectSelectedOption()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				var select = browser.First<Select>("select");
				Assert.AreEqual("2", select.Value);
				Assert.AreEqual("2", select.SelectedOption.Value);
				Assert.AreEqual("Two", select.SelectedOption.Text);
			});
		}

		[TestMethod]
		public void SendInputAllInputs()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/inputs.html");

				foreach (var input in browser.Descendants<TextInput>())
				{
					if (input.Id == "number")
					{
						input.SendInput("100");
						Assert.AreEqual("100", input.Text);
					}
					else
					{
						input.SendInput(input.Id);
						Assert.AreEqual(input.Id, input.Text);
					}
				}

				foreach (var input in browser.Descendants<TextArea>())
				{
					input.SendInput(input.Id);
					Assert.AreEqual(input.Id, input.Text);
				}
			});
		}

		[TestMethod]
		public void SendInputAppendInput()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/inputs.html");
				var input = browser.First<TextInput>("text");
				input.Value = "foo";
				input.SendInput("bar");
				Assert.AreEqual("foobar", input.Value);
			});
		}

		[TestMethod]
		public void SendInputPasswordInput()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/inputs.html");
				var input = browser.First<TextInput>("password");
				input.SendKeys("password", true);
				Assert.AreEqual("password", input.Value);
			});
		}

		[TestMethod]
		public void SendInputSelectInput()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				var select = browser.First<Select>("select");
				select.SendInput("O");
				Assert.AreEqual("One", select.Text);
				Assert.AreEqual("1", select.Value);
			});
		}

		[TestMethod]
		public void SendInputSetInput()
		{
			ForEachBrowser(browser =>
			{
				browser.NavigateTo(TestSite + "/inputs.html");
				var input = browser.First<TextInput>("text");
				input.Value = Guid.NewGuid().ToString();
				var guid = "1F867F5A-20EF-4060-8464-4E4E2C1A57F2";
				input.SendInput(guid, true);
				Assert.AreEqual("1F867F5A-20EF-4060-8464-4E4E2C1A57F2", input.Value);
			}, resizeBrowsers: false);
		}

		[TestMethod]
		public void SendInputUsingKeyboard()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/inputs.html");
				var input = browser.First("text");
				input.SendInput("bar");
				Assert.AreEqual("bar", ((TextInput) input).Value);
			});
		}

		[TestMethod]
		public void SendInputWithNewLine()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/inputs.html");
				var input = browser.First<TextArea>("textarea");
				input.SendInput("first\r\nsecond");
				Assert.AreEqual("first\nsecond", input.Text);
			});
		}

		[TestMethod]
		public void SetButtonText()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				browser.First<Button>("button").Text = "Hello";

				var actual = browser.First<Button>("button").GetAttributeValue("textContent", true);
				Assert.AreEqual("Hello", actual);
			});
		}

		[TestMethod]
		public void SetInputButtonText()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				browser.First<Button>("inputButton").Text = "Hello";

				var actual = browser.First<Button>("inputButton").GetAttributeValue("value", true);
				Assert.AreEqual("Hello", actual);
			});
		}

		[TestMethod]
		public void SetSelectText()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");
				var select = browser.First<Select>("select");
				select.Text = "One";
				Assert.AreEqual("One", select.Text);
				Assert.AreEqual("1", select.Value);

				var text = browser.First<TextInput>("text");
				Assert.AreEqual("1", text.Value);
			});
		}

		[TestMethod]
		public void SetTextAllInputs()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/inputs.html");

				foreach (var input in browser.Descendants<TextInput>())
				{
					if (input.Id == "number")
					{
						input.Text = "100";
						Assert.AreEqual("100", input.Text);
					}
					else
					{
						input.Text = input.Id;
						Assert.AreEqual(input.Id, input.Text);
					}
				}

				foreach (var input in browser.Descendants<TextArea>())
				{
					input.Text = input.Id;
					Assert.AreEqual(input.Id, input.Text);
				}
			});
		}

		[TestMethod]
		public void SetTextWithNewLine()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/inputs.html");
				var input = browser.First<TextArea>("textarea");
				input.Text = "first\r\nsecond";
				Assert.AreEqual("first\nsecond", input.Text);
			});
		}

		[TestMethod]
		public void SetThenGetHtmlOnAboutBlankPage()
		{
			ForAllBrowsers(browser =>
			{
				browser.SetHtml("aoeu");
				Assert.IsTrue(browser.GetHtml().Contains("aoeu"));

				var guid = Guid.NewGuid().ToString();
				guid.Dump();
				var body = $"{guid}";
				var input = $"<html><head><link href=\"https://speedy.local/Content/speedy.css\" rel=\"stylesheet\"></head><body>{body}</body></html>";
				browser.SetHtml(input);
				var actual = browser.GetHtml();
				actual.Dump();
				Assert.IsTrue(actual.Contains(guid));
			});
		}

		[TestMethod]
		public void SetThenGetHtmlOnAboutBlankPageMoreCharacters()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo("about:blank");
				browser.SetHtml("aoeu");
				Assert.IsTrue(browser.GetHtml().Contains("aoeu"));

				var guid = Guid.NewGuid().ToString();
				guid.Dump();
				var body = $"Special characters like \", ', \0, \", \n, \r, \r\n, <, >, should not break html... however &amp; must be already encoded! \n{guid}";
				var input = $"<html><head><link href=\"https://speedy.local/Content/speedy.css\" rel=\"stylesheet\"></head><body>{body}</body></html>";
				browser.SetHtml(input);
				var actual = browser.GetHtml();
				actual.Dump();
				Assert.IsTrue(actual.Contains(guid));
			});
		}

		[TestMethod]
		public void SetThenGetHtmlOnAboutBlankPageWithAllCharacters()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo("about:blank");

				if (browser is Firefox)
				{
					return;
				}

				var buffer = new byte[255];
				for (var i = 0; i < buffer.Length; i++)
				{
					buffer[i] = (byte) i;
				}

				var data = Encoding.UTF8.GetString(buffer).Replace("&", "&amp;");
				data.Escape().Dump();
				var guid = Guid.NewGuid().ToString();
				guid.Dump();
				var body = $"Special characters like \", ', \0, \", \n, \r, \r\n, <, >, should not break html... however &amp; must be already encoded! \n{data}\n{guid}";
				var input = $"<html><head><link href=\"https://speedy.local/Content/speedy.css\" rel=\"stylesheet\"></head><body>{body}</body></html>";
				browser.SetHtml(input);
				var actual = browser.GetHtml();
				actual.Dump();
				Assert.IsTrue(actual.Contains(guid));
			});
		}

		[TestMethod]
		public void SetThenGetHtmlOnAboutBlankPageWithRandomCharacters()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo("about:blank");

				if (browser is Firefox)
				{
					return;
				}

				var buffer = new byte[32];
				RandomNumberGenerator.Create().GetBytes(buffer);
				var data = Encoding.UTF8.GetString(buffer);
				data.Escape().Dump();
				var guid = Guid.NewGuid().ToString();
				guid.Dump();
				var body = $"Special characters like \", ', \0, \", \n, \r, \r\n, <, >, should not break html... however &amp; must be already encoded! \n{data}\n{guid}";
				var input = $"<html><head><link href=\"https://speedy.local/Content/speedy.css\" rel=\"stylesheet\"></head><body>{body}</body></html>";
				browser.SetHtml(input);
				var actual = browser.GetHtml();
				actual.Dump();
				Assert.IsTrue(actual.Contains(guid));
			});
		}

		[TestMethod]
		public void SetValueWithNewLine()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/inputs.html");
				var input = browser.First<TextArea>("textarea");
				input.Value = "first\r\nsecond";
				Assert.AreEqual("first\nsecond", input.Value);
			});
		}

		[TestMethod]
		public void TestContentForInputText()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/TextContent.html");

				var element = browser.First<TextInput>("inputText1");
				Assert.AreEqual(element.Text, "inputText1");
			});
		}

		[TestMethod]
		public void TextAreaValue()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/main.html");

				var element = browser.First<TextArea>("textarea");
				Assert.AreEqual(element.Text, "Text Area's \"Quotes\" Data");

				element.Text = "\"Text Area's \"Quote's\" Data\"";
				Assert.AreEqual(element.Text, "\"Text Area's \"Quote's\" Data\"");
			});
		}

		[TestMethod]
		public void TextContentForDiv()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/TextContent.html");

				var element = browser.First<Division>("div1");
				Assert.AreEqual(element.Text, "\n\t\t\tDiv - Span\n\t\t\tOther Text\n\t\t");
			});
		}

		[TestMethod]
		public void TextContentForDivWithChildren()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/TextContent.html");

				var element = browser.First<Division>("div2");
				Assert.AreEqual(element.Text, "\n\t\t\tHeader One\n\t\t\tHeader Two\n\t\t\tHeader Three\n\t\t\tHeader Four\n\t\t\tHeader Five\n\t\t");
			});
		}

		[TestMethod]
		public void TextContentForHeader1()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/TextContent.html");

				var element = browser.First<Header>("h1");
				Assert.AreEqual(element.Text, "Header One");
			});
		}

		[TestMethod]
		public void TextContentForHeader2()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/TextContent.html");

				var element = browser.First<Header>("h2");
				Assert.AreEqual(element.Text, "Header Two");
			});
		}

		[TestMethod]
		public void TextContentForHeader3()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/TextContent.html");

				var element = browser.First<Header>("h3");
				Assert.AreEqual(element.Text, "Header Three");
			});
		}

		[TestMethod]
		public void TextContentForHeader4()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/TextContent.html");

				var element = browser.First<Header>("h4");
				Assert.AreEqual(element.Text, "Header Four");
			});
		}

		[TestMethod]
		public void TextContentForHeader5()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/TextContent.html");

				var element = browser.First<Header>("h5");
				Assert.AreEqual(element.Text, "Header Five");
			});
		}

		[TestMethod]
		public void VueInputSendKeys()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/Vue.html");
				Assert.AreEqual(false, browser.First<Button>("submit").Enabled);

				browser.First<TextInput>("emailAddress").SendKeys("user");
				var expected = "user";
				var actual = browser.First<WebElement>("emailAddressLabel").GetHtml();
				Assert.AreEqual(expected, actual);

				expected = "keydown: 117(u)<br>keypress: 117(u)<br>keyup: 117(u)<br>keydown: 115(s)<br>keypress: 115(s)<br>keyup: 115(s)<br>keydown: 101(e)<br>keypress: 101(e)<br>keyup: 101(e)<br>keydown: 114(r)<br>keypress: 114(r)<br>keyup: 114(r)<br>";
				actual = browser.First<WebElement>("log").GetHtml();
				Assert.AreEqual(expected, actual);
				Assert.AreEqual(true, browser.First<Button>("submit").Enabled);
			});
		}

		[TestMethod]
		public void VueInputTrigger()
		{
			ForAllBrowsers(browser =>
			{
				browser.NavigateTo(TestSite + "/Vue.html");
				Assert.AreEqual(false, browser.First<Button>("submit").Enabled);

				browser.First<TextInput>("emailAddress").SendInput("user");
				var expected = "user";
				var actual = browser.First<WebElement>("emailAddressLabel").GetHtml();
				Assert.AreEqual(expected, actual);

				actual = browser.First<WebElement>("log").GetHtml();
				Assert.AreEqual(string.Empty, actual);
				Assert.AreEqual(true, browser.First<Button>("submit").Enabled);
			});
		}

		[TestMethod]
		public void WebElementRefresh()
		{
			ForAllBrowsers(browser =>
			{
				browser.Timeout = DefaultTimeout;
				browser.NavigateTo(TestSite + "/Angular.html#!/");

				var items = browser.First<Division>("items");
				Assert.AreEqual(0, items.Children.Count);

				browser.First<Button>("addItem").Click();

				var result = items.Wait(x => x.Refresh().Children.Count == 1);
				Assert.IsTrue(result, "Items was not added");
				Assert.AreEqual(1, items.Children.Count);
				Assert.AreEqual("items-0", items.Children[0].Id);
			});
		}

		private void ForAllBrowsers(Action<Browser> action, BrowserType browserTypes = TestHelper.BrowsersToTests, bool resizeBrowsers = true, bool useSecondaryMonitor = false, int? timeout = null)
		{
			CleanupBrowsers = false;
			browserTypes.ForAllBrowsers(action, useSecondaryMonitor, resizeBrowsers, BrowserResizeType.LeftSideBySide, timeout ?? (int) DefaultTimeout.TotalMilliseconds);
		}

		private void ForEachBrowser(Action<Browser> action, BrowserType browserTypes = TestHelper.BrowsersToTests, bool resizeBrowsers = true, bool useSecondaryMonitor = false)
		{
			CleanupBrowsers = false;
			browserTypes.ForEachBrowser(action, useSecondaryMonitor, resizeBrowsers, BrowserResizeType.LeftSideBySide);
		}

		#endregion
	}
}