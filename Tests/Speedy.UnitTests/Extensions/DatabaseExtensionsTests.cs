#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Client.Data;
using Speedy.EntityFramework;
using Speedy.Extensions;
using Speedy.Website.Data;

#endregion

namespace Speedy.UnitTests.Extensions;

[TestClass]
public class DatabaseExtensionsTests
{
	#region Methods

	[TestMethod]
	public void EnsureAllPropertiesAreMappedForClientDatabase()
	{
		var database = new ContosoClientMemoryDatabase();
		var missingProperties = database.ValidateMappings();
		Validate(missingProperties);
	}

	[TestMethod]
	public void EnsureAllPropertiesAreMappedForEntityDatabase()
	{
		var database = new ContosoMemoryDatabase();
		var missingProperties = database.ValidateMappings();
		Validate(missingProperties);
	}

	private void Validate(IDictionary<string, ICollection<string>> missingProperties)
	{
		missingProperties.ForEach(x =>
		{
			x.Key.Dump();
			x.Value.ForEach(v => $"\tb.Property(x => x.{v}).IsRequired();".Dump());
		});

		if (missingProperties.Any())
		{
			throw new Exception("An entity mapping is missing members.");
		}
	}

	#endregion
}