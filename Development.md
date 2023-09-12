# todo

- Add IClonable to SpeedyList
- Update type.CreateInstance to allow providing custom values or add ability to link interfaces to implementations.
- Should we have a different "date time" other than modified on, if B synced content newer than A but A data is older but just synced it will override B which should "win"?
- bug: FilteredRepository is returning non-saved entities on entity relationship collections
- todo: Add columns changed / modified to the "OnModified" method.
- add ability to "SoftRemove" so the Remove can just set IsDeleted even if "PermantDelete is set"?
- bug: Will database.CollectionChange blow up if we add and remove at the same time? woah.
- Optimization
	- Relationships that are batched sync fails
	- We should try to wire up relationships before saving as a batch or try to fix it from the batch?


# Development Notes / Patterns

The patterns below are used when working in the Speedy solution.

### Data Models

Client[Name] : Storage models for client side entities.
[Name]Entity : Storage models for server side entities.

### Method / Process verbs

Assign[Name] : Represents an assignment model.
Update[Name] : Represents the actual changing of state.


# Migrations

Whenever changes are made to entities we just reset migrations.
Meaning we simple delete the existing migration named "Initial" and recreate it.
Look we have enough database to maintain, this is just a sample. 
Much easier to just blow it away and recreate it. 
Just be sure to delete any existing SQL test databases (Speedy, Speedy2).

See Scripts\Update-SpeedyMigrations.ps1

# Automation

- C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\UIAVerify

# Significant Changes

_handler.ServerCertificateCustomValidationCallback += OnServerCertificateCustomValidationCallback;

# Other Useful Awesome Open Source

- https://lvcharts.com/
-
