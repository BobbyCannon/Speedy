# Migrations

Whenever changes are made to entities we just reset migrations. Meaning we simple delete the existing migration named "Initial" and recreate it. Look we have enough database to maintain, this is just a sample. Much easier to just blow it away and recreate it. Just be sure to delete any existing SQL test databases (Speedy, Speedy2).

Run these two migration to create the "initial" migration

- Add-Migration Initial -Project Speedy.Samples.Sql -StartupProject Speedy.Website
- Add-Migration Initial -Project Speedy.Samples.Sqlite -StartupProject Speedy.Benchmark