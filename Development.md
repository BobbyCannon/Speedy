# Migrations

Whenever changes are made to entities we just reset migrations. Meaning we simple delete the existing migration named "Initial" and recreate it. Look we have enough database to maintain, this is just a sample. Much easier to just blow it away and recreate it. Just be sure to delete any existing SQL test databases (Speedy, Speedy2).

Run these two migration to create the "initial" migration

# Client
Add-Migration Initial -Project Speedy.Client.Data -StartupProject Speedy.IntegrationTests
Remove-Migration -Project Speedy.Client.Data -StartupProject Speedy.IntegrationTests

# Website Sql
Add-Migration Initial -Project Speedy.Website.Data.Sql -StartupProject Speedy.Website
Remove-Migration -Project Speedy.Website.Data.Sql -StartupProject Speedy.Website

# Website Sqlite
Add-Migration Initial -Project Speedy.Website.Data.Sqlite -StartupProject Speedy.Benchmark
Remove-Migration -Project Speedy.Website.Data.Sqlite -StartupProject Speedy.Benchmark
