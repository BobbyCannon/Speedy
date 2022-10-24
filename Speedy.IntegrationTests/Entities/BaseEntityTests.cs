#region References

using System.Collections.Generic;
using System.Text;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Extensions;
using Speedy.Sync;
using Speedy.UnitTests;

#endregion

namespace Speedy.IntegrationTests.Entities
{
	public class BaseEntityTests<T> : SpeedyUnitTest<T> where T : ISyncEntity, new()
	{
		#region Methods

		protected void ValidateExclusions(ISyncEntity entity, Dictionary<string, (bool incoming, bool outgoing, bool syncUpdate, bool changeTracking)> expected, bool printOutput)
		{
			var properties = entity.GetType().GetProperties();
			var builder = new StringBuilder();

			if (properties.Length != expected.Count)
			{
				printOutput = true;
			}

			if (printOutput)
			{
				expected.Clear();
				properties.ForEach(x => { expected.Add(x.Name, (false, false, false, false)); });
			}

			foreach (var item in expected)
			{
				var incoming = entity.IsPropertyExcludedForIncomingSync(item.Key);
				var outgoing = entity.IsPropertyExcludedForOutgoingSync(item.Key);
				var syncUpdate = entity.IsPropertyExcludedForSyncUpdate(item.Key);
				var changeTracking = entity.IsPropertyExcludedForChangeTracking(item.Key);

				if (printOutput)
				{
					var values = $"({incoming}, {outgoing}, {syncUpdate}, {changeTracking})".ToLower();
					builder.AppendLine($"{{ \"{item.Key}\", {values} }},");
				}
				else
				{
					Assert.AreEqual(item.Value.incoming, entity.IsPropertyExcludedForIncomingSync(item.Key), $"Incoming: {item.Key}");
					Assert.AreEqual(item.Value.outgoing, entity.IsPropertyExcludedForOutgoingSync(item.Key), $"Outgoing: {item.Key}");
					Assert.AreEqual(item.Value.syncUpdate, entity.IsPropertyExcludedForSyncUpdate(item.Key), $"Sync Update: {item.Key}");
					Assert.AreEqual(item.Value.changeTracking, entity.IsPropertyExcludedForChangeTracking(item.Key), $"Change Tracking: {item.Key}");
				}
			}

			if (printOutput)
			{
				Clipboard.SetText(builder.ToString());
				TestHelper.Dump(builder.ToString());

				if (properties.Length != expected.Count)
				{
					Assert.Fail("Missing properties, please update test!");
				}

				Assert.Fail("Printing should only be for troubleshooting, please set [printOutput] to false.");
			}
		}

		#endregion
	}
}