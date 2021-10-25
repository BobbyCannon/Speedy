#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Csv;

#endregion

namespace Speedy.UnitTests.Protocols.Csv
{
	/// <summary>
	/// CSV-SPEC: https://csv-spec.org/
	/// </summary>
	[TestClass]
	public class CsvParserTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void BlankLines()
		{
			var csv = "A,B\r\na,b\r\na1,b1\r\n\r\n\r\nc,d\r\n";
			var l = CsvParser.ReadContent<CsvValue>(csv, (o, c) =>
			{
				o.Value1 = c[0];
				o.Value2 = c[1];
				return true;
			});
			Assert.AreEqual(4, l.Count);
			TestHelper.AreEqual(new CsvValue("A", "B"), l[0]);
			TestHelper.AreEqual(new CsvValue("a", "b"), l[1]);
			TestHelper.AreEqual(new CsvValue("a1", "b1"), l[2]);
			TestHelper.AreEqual(new CsvValue("c", "d"), l[3]);
		}

		[TestMethod]
		public void CsvSpecTest1()
		{
			var csv = "aaa,bbb,ccc\r\nxxx,yyy,zzz\r\n";
			var l = CsvParser.ReadContent<CsvValue>(csv, (o, c) =>
			{
				o.Value1 = c[0];
				o.Value2 = c[1];
				o.Value3 = c[2];
				return true;
			});
			Assert.AreEqual(2, l.Count);
			TestHelper.AreEqual(new CsvValue("aaa", "bbb", "ccc"), l[0]);
			TestHelper.AreEqual(new CsvValue("xxx", "yyy", "zzz"), l[1]);
		}

		[TestMethod]
		public void CsvSpecTest10()
		{
			//         0            10              20         30
			//         0 1234 56 7890 12 3456 7 8 9 0123 456789012 3
			var csv = "\"aaa\",\"bbb\",\"ccc\"\r\n\"xxx\",yyy,zzz\r\n";
			var l = CsvParser.ReadContent<CsvValue>(csv, (o, c) =>
			{
				o.Value1 = c[0];
				o.Value2 = c[1];
				o.Value3 = c[2];
				return true;
			});
			Assert.AreEqual(2, l.Count);
			TestHelper.AreEqual(new CsvValue("aaa", "bbb", "ccc"), l[0]);
			TestHelper.AreEqual(new CsvValue("xxx", "yyy", "zzz"), l[1]);
		}

		[TestMethod]
		public void CsvSpecTest2()
		{
			var csv = "aaa,bbb,ccc\r\nxxx,yyy,zzz";
			var l = CsvParser.ReadContent<CsvValue>(csv, (o, c) =>
			{
				o.Value1 = c[0];
				o.Value2 = c[1];
				o.Value3 = c[2];
				return true;
			});
			Assert.AreEqual(2, l.Count);
			TestHelper.AreEqual(new CsvValue("aaa", "bbb", "ccc"), l[0]);
			TestHelper.AreEqual(new CsvValue("xxx", "yyy", "zzz"), l[1]);
		}

		[TestMethod]
		public void CsvSpecTest3()
		{
			var csv = "field_1,field_2,field_3\r\naaa,bbb,ccc\r\nxxx,yyy,zzz";
			var options = new CsvParserOptions { HasHeader = true };
			var l = CsvParser.ReadContent<CsvValue>(csv, options, (o, c) =>
			{
				o.Value1 = c[0];
				o.Value2 = c[1];
				o.Value3 = c[2];
				return true;
			});
			Assert.AreEqual(2, l.Count);
			TestHelper.AreEqual(new CsvValue("aaa", "bbb", "ccc"), l[0]);
			TestHelper.AreEqual(new CsvValue("xxx", "yyy", "zzz"), l[1]);
		}

		[TestMethod]
		public void CsvSpecTest4()
		{
			var csv = "aaa,bbb,ccc\r\n111,222,333,444\r\nxxx,yyy,zzz";
			var l = CsvParser.ReadContent<CsvValue>(csv, (o, c) =>
			{
				o.Value1 = c[0];
				o.Value2 = c[1];
				o.Value3 = c[2];
				return true;
			});
			Assert.AreEqual(3, l.Count);
			TestHelper.AreEqual(new CsvValue("aaa", "bbb", "ccc"), l[0]);
			TestHelper.AreEqual(new CsvValue("111", "222", "333"), l[1]);
			TestHelper.AreEqual(new CsvValue("xxx", "yyy", "zzz"), l[2]);
		}

		[TestMethod]
		public void CsvSpecTest5()
		{
			var csv = "aaa,bbb,\r\nxxx,yyy,";
			var l = CsvParser.ReadContent<CsvValue>(csv, (o, c) =>
			{
				o.Value1 = c[0];
				o.Value2 = c[1];
				o.Value3 = c[2];
				return true;
			});
			Assert.AreEqual(2, l.Count);
			TestHelper.AreEqual(new CsvValue("aaa", "bbb", ""), l[0]);
			TestHelper.AreEqual(new CsvValue("xxx", "yyy", ""), l[1]);
		}

		[TestMethod]
		public void CsvSpecTest6()
		{
			var csv = "aaa ,  bbb , ccc\r\n xxx, yyy  ,zzz ";
			var l = CsvParser.ReadContent<CsvValue>(csv, (o, c) =>
			{
				o.Value1 = c[0];
				o.Value2 = c[1];
				o.Value3 = c[2];
				return true;
			});
			Assert.AreEqual(2, l.Count);
			TestHelper.AreEqual(new CsvValue("aaa ", "  bbb ", " ccc"), l[0]);
			TestHelper.AreEqual(new CsvValue(" xxx", " yyy  ", "zzz "), l[1]);
		}

		[TestMethod]
		public void CsvSpecTest7()
		{
			var csv = "aaa,\"b\r\nbb\",ccc\r\nxxx,\"y, yy\",zzz\r\n";
			var l = CsvParser.ReadContent<CsvValue>(csv, (o, c) =>
			{
				o.Value1 = c[0];
				o.Value2 = c[1];
				o.Value3 = c[2];
				return true;
			});
			Assert.AreEqual(2, l.Count);
			TestHelper.AreEqual(new CsvValue("aaa", "b\r\nbb", "ccc"), l[0]);
			TestHelper.AreEqual(new CsvValue("xxx", "y, yy", "zzz"), l[1]);
		}

		[TestMethod]
		public void CsvSpecTest8()
		{
			var csv = "aaa,\"b\"\"bb\",ccc\r\n";
			var l = CsvParser.ReadContent<CsvValue>(csv, (o, c) =>
			{
				o.Value1 = c[0];
				o.Value2 = c[1];
				o.Value3 = c[2];
				return true;
			});
			Assert.AreEqual(1, l.Count);
			TestHelper.AreEqual(new CsvValue("aaa", "b\"bb", "ccc"), l[0]);
		}

		[TestMethod]
		public void CsvSpecTest9()
		{
			//         0         10           20
			//         012345678901 2 3456789 012345 678901 2 
			var csv = "aaa,bbb,ccc\r\nxxx,  \"y, yy\" ,zzz\r\n";
			var l = CsvParser.ReadContent<CsvValue>(csv, (o, c) =>
			{
				o.Value1 = c[0];
				o.Value2 = c[1];
				o.Value3 = c[2];
				return true;
			});
			Assert.AreEqual(2, l.Count);
			TestHelper.AreEqual(new CsvValue("aaa", "bbb", "ccc"), l[0]);
			TestHelper.AreEqual(new CsvValue("xxx", "y, yy", "zzz"), l[1]);
		}

		[TestMethod]
		public void SingleColumn()
		{
			var scenarios = new[]
			{
				("\"a\"", "a"),
				(" \"b\"", "b"),
				("\"c\" ", "c"),
				(" \"d\" ", "d"),
				("  \"e\"", "e"),
				("\"f\"  ", "f"),
				("  \"g\"  ", "g")
			};

			foreach (var scenario in scenarios)
			{
				Console.WriteLine(scenario.Item1);
				var l = CsvParser.ReadContent<CsvValue>(scenario.Item1, (o, c) =>
				{
					o.Value1 = c[0];
					return true;
				});
				Assert.AreEqual(1, l.Count);
				Assert.AreEqual(scenario.Item2, l[0].Value1);
			}
		}

		#endregion

		#region Classes

		public class CsvValue
		{
			#region Constructors

			public CsvValue()
			{
			}

			public CsvValue(string value1, string value2 = null, string value3 = null)
			{
				Value1 = value1;
				Value2 = value2;
				Value3 = value3;
			}

			#endregion

			#region Properties

			public string Value1 { get; set; }

			public string Value2 { get; set; }

			public string Value3 { get; set; }

			#endregion

			#region Methods

			public override string ToString()
			{
				return $"{Value1},{Value2},{Value3}";
			}

			#endregion
		}

		#endregion
	}
}