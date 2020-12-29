#region References

using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Collections;
using Speedy.Data.Client;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Collections
{
	[TestClass]
	public class BaseObservableCollectionTests
	{
		#region Methods

		[TestMethod]
		public void ClearEmptyCollection()
		{
			var changes = 0;
			var collection = new BaseObservableCollection<string>();
			collection.CollectionChanged += (_, _) => changes++;
			collection.Clear();
			Assert.AreEqual(0, changes);
		}

		[TestMethod]
		public void ClearShouldCallOnChangedEvent()
		{
			var collection = new BaseObservableCollection<ClientAccount>();
			var actual = new List<NotifyCollectionChangedEventArgs>();
			collection.Add(new ClientAccount());
			collection.Add(new ClientAccount());

			collection.CollectionChanged += (sender, args) => actual.Add(args);
			collection.Clear();
			Assert.AreEqual(2, actual.Count);

			var actualEvent = actual[0];
			Assert.AreEqual(NotifyCollectionChangedAction.Remove, actualEvent.Action);
			Assert.AreEqual(1, actualEvent.OldItems.Count);
			Assert.AreEqual(null, actualEvent.NewItems);

			actualEvent = actual[1];
			Assert.AreEqual(NotifyCollectionChangedAction.Remove, actualEvent.Action);
			Assert.AreEqual(1, actualEvent.OldItems.Count);
			Assert.AreEqual(null, actualEvent.NewItems);
		}

		[TestMethod]
		public void DistinctCollection()
		{
			var collection = new BaseObservableCollection<string> { DistinctCheck = Equals };
			collection.AddRange(new[] { "test", "test", "test", "test" });
			Assert.AreEqual(1, collection.Count);
			Assert.AreEqual("test", collection[0]);
		}

		#endregion
	}
}