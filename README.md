# Speedy

Small embedded key value pair persistent repository for .NET.

### Code Example

```
var repository = new Repository("C:\\Speedy", "Items");
repository.Write("Item1", "Item1");
repository.Write("Item2", "Item2");
repository.Write("Item3", "Item3");
repository.Save();
```

##### Only requirement

Keys cannot contain the "|" character

##### Rough Benchmarks

I just did a few quick benchmarks. Here are the writes.

```
Starting to benchmark Speedy... hold on to your hats!
Cleaning up the test data folder...

Let's create a repository with 100,000 items @ 100 at a time.
Done: 00:00:22.8881170

Let's create a repository with 100,000 items @ 1000 at a time.
Done: 00:00:04.1342338

Let's create a repository with 100,000 items @ 2500 at a time.
Done: 00:00:01.7382815

Let's create a repository with 100,000 items @ 1000 at a time.
Done: 00:00:04.1350233
```

Reads using 100 random keys in a repository of 100,000 items.

```
Let's read randomly into the DB-100000 repository @ 1 at a time.
Total: 00:00:02.9320008

Let's read randomly into the DB-100000 repository using all keys.
Total: 00:00:00.0307500
```