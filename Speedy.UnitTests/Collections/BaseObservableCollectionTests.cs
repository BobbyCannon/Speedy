#region References

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Collections;
using Speedy.Data.Client;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Collections;

[TestClass]
public class BaseObservableCollectionTests : SpeedyUnitTest
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
		collection.AddRange("test", "test", "test", "test");
		Assert.AreEqual(1, collection.Count);
		Assert.AreEqual("test", collection[0]);
	}

	[TestMethod]
	public void OnPropertyChanged()
	{
		var collection = new TestCollection();
		var actual = new List<string>();

		void onCollectionOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			actual.Add(args.PropertyName);
		}

		try
		{
			// will cause default property changes
			collection.PropertyChanged += onCollectionOnPropertyChanged;
			collection.Add(1);
			AreEqual(new[] { "Count", "Item[]" }, actual.ToArray(), actual.DumpJson);

			// should cause a property change
			actual.Clear();
			collection.Name = "aoeu";
			AreEqual(new[] { "Name" }, actual.ToArray(), actual.DumpJson);
		}
		finally
		{
			collection.PropertyChanged -= onCollectionOnPropertyChanged;
		}
	}

	#endregion

	#region Classes

	private class TestCollection : BaseObservableCollection<int>
	{
		#region Properties

		public string Name { get; set; }

		#endregion
	}

	#endregion
}