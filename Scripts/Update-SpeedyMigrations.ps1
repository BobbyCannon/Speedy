#Requires -RunAsAdministrator

# dotnet tool install --global dotnet-ef
# dotnet tool update --global dotnet-ef

$ErrorActionPreference = "Stop"
$watch = [System.Diagnostics.Stopwatch]::StartNew()
$scriptPath = Split-Path(Get-Variable MyInvocation).Value.MyCommand.Path
$scriptPath = (Get-Item $scriptPath).Parent.FullName

# $scriptPath = "C:\Workspaces\EpicCoders\Speedy"
# $scriptPath = "C:\Workspaces\GitHub\Speedy"

Write-Host $scriptPath

# dotnet ef migrations -h
# dotnet ef migrations add -h

#
# Client Data Sqlite
#

$projectPath = "$scriptPath\Speedy.Client.Data.Sqlite"
$startupPath = "$scriptPath\Speedy.IntegrationTests"

Remove-Item "$projectPath\Migrations" -ErrorAction Ignore
dotnet ef migrations add InitialMigration --framework "net6.0-windows" --configuration "debug" -c "ClientDatabase" -p $projectPath -s $startupPath -o "$projectPath\Migrations"


#
# Client Data Sqlite Old
#

$projectPath = "$scriptPath\Speedy.Client.Data.Sqlite.Old"
$startupPath = "$scriptPath\Speedy.IntegrationTests"

Remove-Item "$projectPath\Migrations" -ErrorAction Ignore
dotnet ef migrations add InitialMigration --framework "net48" --configuration "debug" -c "ClientDatabase" -p $projectPath -s $startupPath -o "$projectPath\Migrations"
#dotnet build $projectPath --framework "netcoreapp3.1" --configuration "debug"
#dotnet build $startupPath --framework "net48" --configuration "debug"


#
# Website Data Sql
#

$projectPath = "$scriptPath\Speedy.Website.Data.Sql"
$startupPath = "$scriptPath\Speedy.Website"

Remove-Item "$projectPath\Migrations" -ErrorAction Ignore
dotnet ef migrations add InitialMigration --framework "net6.0" --configuration "debug" -c "ServerSqlDatabase" -p $projectPath -s $startupPath -o "$projectPath\Migrations"

#
# Website Data Sql Old
#

$projectPath = "$scriptPath\Speedy.Website.Data.Sql.Old"
$startupPath = "$scriptPath\Speedy.Website"

Remove-Item "$projectPath\Migrations" -ErrorAction Ignore
dotnet ef migrations add InitialMigration --framework "net5.0" --configuration "debug" -c "ServerSqlDatabase" -p $projectPath -s $startupPath -o "$projectPath\Migrations"

#
# Website Data Sqlite
#

$projectPath = "$scriptPath\Speedy.Website.Data.Sqlite"
$startupPath = "$scriptPath\Speedy.Website"

Remove-Item "$projectPath\Migrations" -ErrorAction Ignore
dotnet ef migrations add InitialMigration --framework "net6.0" --configuration "debug" -c "ServerSqliteDatabase" -p $projectPath -s $startupPath -o "$projectPath\Migrations"

#
# Website Data Sqlite Old
#

$projectPath = "$scriptPath\Speedy.Website.Data.Sqlite.Old"
$startupPath = "$scriptPath\Speedy.Website"

Remove-Item "$projectPath\Migrations" -ErrorAction Ignore
dotnet ef migrations add InitialMigration --framework "netcoreapp3.1" --configuration "debug" -c "ServerSqliteDatabase" -p $projectPath -s $startupPath -o "$projectPath\Migrations"
#dotnet build $projectPath --framework "netstandard2.0" --configuration "debug"
#dotnet build $startupPath --framework "netcoreapp3.1" --configuration "debug"
