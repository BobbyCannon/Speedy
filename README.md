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

I just did a few quick benchmarks.

##### Writes

```
Let's create a repository with 100000 items @ 100 at a time.
Done: 00:00:25.8153494

Let's create a repository with 100000 items @ 1000 at a time.
Done: 00:00:04.0267065

Let's create a repository with 100000 items @ 2500 at a time.
Done: 00:00:01.6755958

Let's create a repository with 100000 items @ 10000 at a time.
Done: 00:00:00.5001176

Let's create a repository with 100000 items @ 50000 at a time.
Done: 00:00:00.1786458
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

Write from mulitple threads.

```
var repository = new Repository(_directory, Guid.NewGuid().ToString());

var action = new Action<Repository, int, int>((repo, min, max) =>
{
	for (var i = min; i < max; i++)
	{
		repository.Write("Key" + i, "Value" + i);
		repository.Save();
	}
});

var index = 0;
var tasks = new[]
{
	Task.Run(() => action(repository, index++, 100 * index)),
	Task.Run(() => action(repository, index++, 100 * index)),
	Task.Run(() => action(repository, index++, 100 * index)),
	Task.Run(() => action(repository, index++, 100 * index)),
	Task.Run(() => action(repository, index++, 100 * index)),
	Task.Run(() => action(repository, index++, 100 * index)),
	Task.Run(() => action(repository, index++, 100 * index)),
	Task.Run(() => action(repository, index++, 100 * index))
};

Task.WaitAll(tasks);

var actual = repository.Read().ToList();
Assert.AreEqual(tasks.Length * 100, actual.Count);
```

Write and read from multiple threads.

```
var repository = new Repository(_directory, Guid.NewGuid().ToString());
var random = new Random();

var readAction = new Action<Repository>((repo) =>
{
	Thread.Sleep(random.Next(750, 2000));
	repository.Read().ToList();
});

var writeAction = new Action<Repository, int, int>((repo, min, max) =>
{
	for (var i = min; i < max; i++)
	{
		repository.Write("Key" + i, "Value" + i);
		repository.Save();
	}
});
			
var index = 0;
var tasks = new[]
{
	Task.Run(() => writeAction(repository, index++, 100 * index)),
	Task.Run(() => readAction(repository)),
	Task.Run(() => writeAction(repository, index++, 100 * index)),
	Task.Run(() => readAction(repository)),
	Task.Run(() => writeAction(repository, index++, 100 * index)),
	Task.Run(() => readAction(repository)),
	Task.Run(() => writeAction(repository, index++, 100 * index)),
	Task.Run(() => readAction(repository))
};

Task.WaitAll(tasks);

var actual = repository.Read().ToList();
Assert.AreEqual(4 * 100, actual.Count);
```