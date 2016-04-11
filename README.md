# Speedy

Speedy offers a quick embedded way to manage your data. Here are the two options.

* Embedded Database
	* Memory
	* Disk
	* SQL (Entity Framework)

* Key Value Repository


## Embedded Database

Simple and easy to use embedded relationship database.

#### Code Example

```
using (var database = provider.CreateContext())
{
	for (var i = count; i < count + chunkSize; i++)
	{
		var address = new Address { Line1 = "Line " + i, Line2 = "Line " + i, City = "City " + i, Postal = "Postal " + i, State = "State " + i };

		if (random.Next(1, 100) % 2 == 0)
		{
			address.People.Add(new Person
			{
				Name = "Person " + i
			});
		}

		database.Addresses.Add(address);
	}

	database.SaveChanges();
}
```

#### Write of 10,000 items

```
On Disk
10:277 : 150 chunks.
05:957 : 300 chunks.
03:886 : 600 chunks.
03:222 : 1200 chunks.
03:455 : 2400 chunks.

SQL
08:574 : 150 chunks.
11:985 : 300 chunks.
19:598 : 600 chunks.
36:905 : 1200 chunks.
20:363 : 2400 chunks.
```

## Key Value Repository

Small thread safe embedded key value pair persistent repository for .NET.

#### Code Example

```
var repository = new Repository("C:\\Speedy", "Items");
repository.Write("Item1", "Item1");
repository.Write("Item2", "Item2");
repository.Write("Item3", "Item3");
repository.Save();
```

##### Only Requirement

Keys cannot contain the "|" (pipe) character.

#### Quick Benchmarks

I just did a few quick benchmarks. The first set is without caching. The second set is using a cache timeout of 30 seconds and limits.

#### Writes

```
Starting to benchmark Speedy Repository writing 100000...
No Caching
22:318: 100 at a time.
04:382: 1000 at a time.
01:926: 2500 at a time.
00:776: 10000 at a time.
00:414: 50000 at a time.

Caching
02:780: 100 at a time with a cache of 1000 items.
00:529: 1000 at a time with a cache of 10000 items.
00:756: 2500 at a time with a cache of 10000 items.
00:365: 10000 at a time with a cache of 25000 items.
00:255: 50000 at a time with a cache of 100000 items.
```

You'll need to balance how often to save you repository based on how much memory you want to use. More items written before saving
will help speed but will require more memory. If you have low memory requirements then save often but it'll take longer to write the 
full repository.

##### Reads

Reads using 100 random keys in a repository of 100,000 items.

*Note:* The times will fluctuate as they keys are selected at random.

```
Let's read the randomly selected keys from the DB-100000 repository one at a time.
Total: 07:567

Let's read the randomly selected keys from the DB-100000 repository all at once.
Total: 00:132
```

##### Multiple Threads

Write from multiple threads.

```
using (var context = Repository.Create(TestHelper.Directory, Guid.NewGuid().ToString(), TimeSpan.FromSeconds(1), 5))
{
	var repository = context;
	var action = new Action<IRepository, int, int>((repo, min, max) =>
	{
		for (var i = min; i < max; i++)
		{
			repository.Write("Key" + i, "Value" + i);
			repository.Save();
		}

		repository.Flush();
	});

	var size = 10;

	var tasks = new[]
	{
		Task.Factory.StartNew(() => action(repository, 0, size)),
		Task.Factory.StartNew(() => action(repository, 1 * size, 1 * size + size)),
		Task.Factory.StartNew(() => action(repository, 2 * size, 2 * size + size)),
		Task.Factory.StartNew(() => action(repository, 3 * size, 3 * size + size)),
		Task.Factory.StartNew(() => action(repository, 4 * size, 4 * size + size)),
		Task.Factory.StartNew(() => action(repository, 5 * size, 5 * size + size)),
		Task.Factory.StartNew(() => action(repository, 6 * size, 6 * size + size)),
		Task.Factory.StartNew(() => action(repository, 7 * size, 7 * size + size))
	};

	Task.WaitAll(tasks);
	repository.Flush();

	var actual = repository.Read().ToList();
	Assert.AreEqual(tasks.Length * size, actual.Count);
}
```

Write and read from multiple threads.

```
using (var context = Repository.Create(TestHelper.Directory, Guid.NewGuid().ToString(), TimeSpan.FromSeconds(1), 10))
{
	var repository = context;
	var random = new Random();

	var readAction = new Action<IRepository>(repo =>
	{
		Thread.Sleep(random.Next(10, 50));
		repository.Read().ToList();
	});

	var writeAction = new Action<IRepository, int, int>((repo, min, max) =>
	{
		for (var i = min; i < max; i++)
		{
			repository.Write("Key" + i, "Value" + i);
			repository.Save();
		}

		repository.Flush();
	});

	var size = 20;

	var tasks = new[]
	{
		Task.Factory.StartNew(() => writeAction(repository, 0, size)),
		Task.Factory.StartNew(() => readAction(repository)),
		Task.Factory.StartNew(() => writeAction(repository, 1 * size, 1 * size + size)),
		Task.Factory.StartNew(() => readAction(repository)),
		Task.Factory.StartNew(() => writeAction(repository, 2 * size, 2 * size + size)),
		Task.Factory.StartNew(() => readAction(repository)),
		Task.Factory.StartNew(() => writeAction(repository, 3 * size, 3 * size + size)),
		Task.Factory.StartNew(() => readAction(repository))
	};

	Task.WaitAll(tasks);

	var actual = repository.Read().ToList();
	Assert.AreEqual(tasks.Length / 2 * size, actual.Count);
}
```

#### Versions

v4 Upgraded to .NET 4.5.2
v3 Database, Repository, .NET 4
v2 Repository only, .NET 4
v1 Original release, .NET 4