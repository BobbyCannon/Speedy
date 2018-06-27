# Speedy

Speedy is a simple easy to use Entity Framework unit testing framework.

### Setup an interface to describe your database

``` csharp
public interface IContosoDatabase : IDatabase
{
	IRepository<Address, int> Addresses { get; }
	IRepository<Person, int> People { get; }
}
```

### Setup your Entity Framework database

``` csharp
public class ContosoDatabase : EntityFrameworkDatabase, IContosoDatabase
{
	public ContosoDatabase()
	{
		// Default constructor needed for Add-Migration
	}

	public ContosoDatabase(DbContextOptions contextOptions, DatabaseOptions options = null)
		: base(contextOptions, options)
	{
	}

	public IRepository<Address, int> Addresses => GetRepository<Address>();
	public IRepository<Person, int> People => GetRepository<Person>();
}
```

### Setup your Memory database

``` csharp
public class ContosoMemoryDatabase : Database, IContosoDatabase
{
	public ContosoMemoryDatabase(string directory = null, DatabaseOptions options = null)
		: base(directory, options)
	{
		Addresses = GetRepository<Address>();
		People = GetRepository<Person>();
	}

	public IRepository<Address, int> Addresses { get; }
	public IRepository<Person, int> People { get; }
}
```

### Now you can write test that will run against EntityFramework and / or the Memory database

Preferable you'll write all your _unit test_ using the memory database. You can then use the Entity Framework database for your _integration tests_.

``` csharp
[TestMethod]
public void AddAddressTest()
{
	foreach (var database in new IContosoDatabase[] { new ContosoDatabase(GetOptions()), new ContosoMemoryDatabase() })
	{
		using (database)
		{
			database.Addresses.Add(new Address
				{
					City = "Greenville",
					Line1 = "Main Street",
					Line2 = string.Empty,
					Postal = "29671",
					State = "SC"
				});
			
			database.SaveChanges();
		}
	}
}

```

## Versions

- v5 supports Entity Framework Core
- v4 supports Entity Framework 6