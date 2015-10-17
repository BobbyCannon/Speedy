# Speedy

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

Let's create a repository with 100000 items @ 100 at a time.
Done: 00:00:21.4885609

Let's create a repository with 100000 items @ 1000 at a time.
Done: 00:00:04.0543204

Let's create a repository with 100000 items @ 2500 at a time.
Done: 00:00:01.7953596

Let's create a repository with 100000 items @ 10000 at a time.
Done: 00:00:00.5891991

Let's create a repository with 100000 items @ 50000 at a time.
Done: 00:00:00.2776635

```

#### Writes With Cache Limits
```

Let's create a repository with 100000 items @ 100 at a time with a cache of 1000 items.
Done: 00:00:03.4236623

Let's create a repository with 100000 items @ 1000 at a time with a cache of 10000 items.
Done: 00:00:01.3811429

Let's create a repository with 100000 items @ 2500 at a time with a cache of 10000 items.
Done: 00:00:00.6092164

Let's create a repository with 100000 items @ 10000 at a time with a cache of 25000 items.
Done: 00:00:01.2549856

Let's create a repository with 100000 items @ 50000 at a time with a cache of 100000 items.
Done: 00:00:01.1356382
```

You'll need to balance how often to save you repository based on
how much memory you want to use. More items written before saving
will help speed but will require more memory. If you have low memory
requirements then save often but it'll take longer to write the 
full repository.

##### Reads

Reads using 100 random keys in a repository of 100,000 items.

```
Let's read randomly into the DB-100000 repository @ 1 at a time.
Total: 00:00:02.7959685

Let's read randomly into the DB-100000 repository using all keys.
Total: 00:00:00.0281244
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