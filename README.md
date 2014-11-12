# Speedy

Small embedded key value pair persistent repository for .NET.

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
Done: 00:00:23.2290542

Let's create a repository with 100000 items @ 1000 at a time.
Done: 00:00:04.2213932

Let's create a repository with 100000 items @ 2500 at a time.
Done: 00:00:01.7740462

Let's create a repository with 100000 items @ 10000 at a time.
Done: 00:00:00.5225688

Let's create a repository with 100000 items @ 50000 at a time.
Done: 00:00:00.1917974
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
Total: 00:00:02.9124129

Let's read randomly into the DB-100000 repository using all keys.
Total: 00:00:00.0293204
```