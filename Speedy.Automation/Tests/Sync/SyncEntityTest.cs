﻿#region References

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Sync;

#endregion

namespace Speedy.Automation.Tests.Sync;

public abstract class SyncEntityTest<T, T2> : SpeedyTest 
	where T : SyncEntity<T2>, new()
{
    #region Methods

    protected void ValidateExclusions(Dictionary<string, (bool incoming, bool outgoing, bool syncUpdate, bool changeTracking)> expected, bool printOutput)
    {
        var entity = new T();
        ValidateExclusions(entity, expected, printOutput);
    }

    protected void ValidateExclusions(T entity, Dictionary<string, (bool incoming, bool outgoing, bool syncUpdate, bool changeTracking)> expected, bool printOutput)
    {
        var properties = entity.GetType().GetProperties().OrderBy(x => x.Name).ToList();
        var testWrong = properties.Count != expected.Count;

        if (testWrong)
        {
            printOutput = true;
        }

        if (printOutput)
        {
            expected.Clear();
            properties.ForEach(x => { expected.Add(x.Name, (false, false, false, false)); });
        }

        var builder = new StringBuilder();
        var actual = expected
            .ToDictionary(x => x.Key, x =>
            {
                var incoming = entity.IsPropertyExcludedForIncomingSync(x.Key);
                var outgoing = entity.IsPropertyExcludedForOutgoingSync(x.Key);
                var syncUpdate = entity.IsPropertyExcludedForSyncUpdate(x.Key);
                var changeTracking = entity.IsPropertyExcludedForChangeTracking(x.Key);
                return (incoming, outgoing, syncUpdate, changeTracking);
            });

        if (printOutput)
        {
            foreach (var item in actual)
            {
                var values = $"(incoming: {item.Value.incoming.ToString().ToLower()}, " +
                    $"outgoing: {item.Value.outgoing.ToString().ToLower()}, " +
                    $"syncUpdate: {item.Value.syncUpdate.ToString().ToLower()}, " +
                    $"changeTracking: {item.Value.changeTracking.ToString().ToLower()})";

                builder.AppendLine($"{{ \"{item.Key}\", {values} }},");
            }
        }

        foreach (var item in expected)
        {
            try
            {
                var results = actual[item.Key];
                Assert.AreEqual(item.Value.incoming, results.incoming, $"Incoming: {item.Key}");
                Assert.AreEqual(item.Value.outgoing, results.outgoing, $"Outgoing: {item.Key}");
                Assert.AreEqual(item.Value.syncUpdate, results.syncUpdate, $"Sync Update: {item.Key}");
                Assert.AreEqual(item.Value.changeTracking, results.changeTracking, $"Change Tracking: {item.Key}");
            }
            catch (System.Exception)
            {
                builder.ToString().Dump();
                throw;
            }
        }

        if (printOutput)
        {
            CopyToClipboard(builder.ToString());
            builder.ToString().Dump();

            if (testWrong)
            {
                Assert.Fail("Missing properties, please update test!");
            }

            Assert.Fail("Printing should only be for troubleshooting, please set [printOutput] to false.");
        }
    }

    #endregion
}