#region References

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Linq;
using Speedy.Samples.Entities;
using DynamicExpression = Speedy.Linq.DynamicExpression;

#endregion

namespace Speedy.IntegrationTests
{
	[TestClass]
	public class LinqTests
	{
		#region Methods

		[TestMethod]
		public void CreateClassTheadSafe()
		{
			const int numOfTasks = 15;

			var properties = new[] { new DynamicProperty("prop1", typeof(string)) };

			var tasks = new List<Task>(numOfTasks);

			for (var i = 0; i < numOfTasks; i++)
			{
				tasks.Add(Task.Factory.StartNew(() => DynamicExpression.CreateClass(properties)));
			}

			Task.WaitAll(tasks.ToArray());
		}

		[TestMethod]
		public void FilterExact()
		{
			var collection = new[]
			{
				new Address { City = "Easley" },
				new Address { City = "Pickens" },
				new Address { City = "Greenville" }
			};

			var expected = new[] { collection[0] };
			var actual = collection.Where("City == \"Easley\"").ToArray();

			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void FilterExactUsingParameters()
		{
			var collection = new[]
			{
				new Address { City = "Easley" },
				new Address { City = "Pickens" },
				new Address { City = "Greenville" }
			};

			var expected = new[] { collection[0] };
			var actual = collection.Where("City == @0", "Easley").ToArray();

			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void FilterGreaterThan()
		{
			var collection = new[]
			{
				new Address { City = "Easley" },
				new Address { City = "Pickens" },
				new Address { City = "Greenville" }
			};

			var expected = new[] { collection[1], collection[2] };
			var actual = collection.Where("City > @0", "Easley").ToArray();

			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void OrderBy()
		{
			var collection = new[]
			{
				new Address { City = "Easley" },
				new Address { City = "Pickens" },
				new Address { City = "Greenville" }
			};

			var expected = new[] { collection[0], collection[2], collection[1] };
			var actual = collection.OrderBy("City").ToArray();

			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void OrderByDescending()
		{
			var collection = new[]
			{
				new Address { City = "Easley" },
				new Address { City = "Pickens" },
				new Address { City = "Greenville" }
			};

			var expected = new[] { collection[1], collection[2], collection[0] };
			var actual = collection.OrderBy("City descending").ToArray();

			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void ParseDoubleLiteralReturnsDoubleExpression()
		{
			var expression = (ConstantExpression) DynamicExpression.Parse(typeof(double), "1.0");
			Assert.AreEqual(typeof(double), expression.Type);
			Assert.AreEqual(1.0, expression.Value);
		}

		[TestMethod]
		public void ParseFloatLiteralReturnsFloatExpression()
		{
			var expression = (ConstantExpression) DynamicExpression.Parse(typeof(float), "1.0f");
			Assert.AreEqual(typeof(float), expression.Type);
			Assert.AreEqual(1.0f, expression.Value);
		}

		[TestMethod]
		public void ParseLambdaDelegateTypeMethodCallReturnsEventHandlerLambdaExpression()
		{
			var expression = DynamicExpression.ParseLambda(
				typeof(EventHandler),
				new[]
				{
					Expression.Parameter(typeof(object), "sender"),
					Expression.Parameter(typeof(EventArgs), "e")
				},
				null,
				"sender.ToString()");

			Assert.AreEqual(typeof(void), expression.ReturnType);
			Assert.AreEqual(typeof(EventHandler), expression.Type);
		}

		[TestMethod]
		public void ParseLambdaVoidMethodCallReturnsActionDelegate()
		{
			var expression = DynamicExpression.ParseLambda(typeof(FileStream), null, "it.Close()");
			Assert.AreEqual(typeof(void), expression.ReturnType);
			Assert.AreEqual(typeof(Action<FileStream>), expression.Type);
		}

		[TestMethod]
		public void ParseParameterExpressionMethodCallReturnsIntExpression()
		{
			var expression = DynamicExpression.Parse(new[] { Expression.Parameter(typeof(int), "x") }, typeof(int), "x + 1");
			Assert.AreEqual(typeof(int), expression.Type);
		}

		[TestMethod]
		public void ParseTupleToStringMethodCallReturnsStringLambdaExpression()
		{
			var expression = DynamicExpression.ParseLambda(typeof(Tuple<int>), typeof(string), "it.ToString()");
			Assert.AreEqual(typeof(string), expression.ReturnType);
		}

		#endregion
	}
}