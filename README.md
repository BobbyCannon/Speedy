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