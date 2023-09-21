#Requires -RunAsAdministrator

# dotnet tool install --global dotnet-ef
# dotnet tool update --global dotnet-ef

$ErrorActionPreference = "Stop"
$watch = [System.Diagnostics.Stopwatch]::StartNew()
$scriptPath = Split-Path(Get-Variable MyInvocation).Value.MyCommand.Path
$scriptPath = (Get-Item $scriptPath).Parent.FullName

# $scriptPath = "C:\Workspaces\GitHub\Speedy"

Write-Host $scriptPath

# dotnet ef migrations -h
# dotnet ef migrations add -h

#
# Client Data Sqlite
#

$projectPath = "$scriptPath\Samples\Client\Speedy.Client.Data.Sqlite"
$startupPath = "$scriptPath\Tests\Speedy.IntegrationTests"

if (!(Test-Path $projectPath))
{
	throw "$projectPath does not exists"	
}

if (!(Test-Path $startupPath))
{
	throw "$startupPath does not exists"	
}

# ii $projectPath

Remove-Item "$projectPath\Migrations" -ErrorAction Ignore
dotnet ef migrations add InitialMigration --framework "net7.0-windows10.0.19041.0" --configuration "debug" -c "ContosoClientDatabase" -p $projectPath -s $startupPath -o "$projectPath\Migrations"

#
# Client Data Sqlite Old
#

$projectPath = "$scriptPath\Samples\Client\Speedy.Client.Data.Sqlite.Old"
$startupPath = "$scriptPath\Tests\Speedy.IntegrationTests"

if (!(Test-Path $projectPath))
{
	throw "$projectPath does not exists"	
}

if (!(Test-Path $startupPath))
{
	throw "$startupPath does not exists"	
}

Remove-Item "$projectPath\Migrations" -ErrorAction Ignore
dotnet ef migrations add InitialMigration --framework "net48" --configuration "debug" -c "ContosoClientDatabase" -p $projectPath -s $startupPath -o "$projectPath\Migrations"


#
# Website Data Sql
#

$projectPath = "$scriptPath\Samples\Website\Speedy.Website.Data.Sql"
$startupPath = "$scriptPath\Samples\Website\Speedy.Website"

if (!(Test-Path $projectPath))
{
	throw "$projectPath does not exists"	
}

if (!(Test-Path $startupPath))
{
	throw "$startupPath does not exists"	
}

Remove-Item "$projectPath\Migrations" -ErrorAction Ignore
dotnet ef migrations add InitialMigration --framework "net7.0" --configuration "debug" -c "ContosoSqlDatabase" -p $projectPath -s $startupPath -o "$projectPath\Migrations"

#
# Website Data Sql Old
#

$projectPath = "$scriptPath\Samples\Website\Speedy.Website.Data.Sql.Old"
$startupPath = "$scriptPath\Samples\Website\Speedy.Website"

if (!(Test-Path $projectPath))
{
	throw "$projectPath does not exists"	
}

if (!(Test-Path $startupPath))
{
	throw "$startupPath does not exists"	
}

Remove-Item "$projectPath\Migrations" -ErrorAction Ignore
dotnet ef migrations add InitialMigration --framework "netcoreapp3.1" --configuration "debug" -c "ContosoSqlDatabase" -p $projectPath -s $startupPath -o "$projectPath\Migrations"

#
# Website Data Sqlite
#

$projectPath = "$scriptPath\Samples\Website\Speedy.Website.Data.Sqlite"
$startupPath = "$scriptPath\Samples\Website\Speedy.Website"

if (!(Test-Path $projectPath))
{
	throw "$projectPath does not exists"	
}

if (!(Test-Path $startupPath))
{
	throw "$startupPath does not exists"	
}

Remove-Item "$projectPath\Migrations" -ErrorAction Ignore
dotnet ef migrations add InitialMigration --framework "net7.0" --configuration "debug" -c "ContosoSqliteDatabase" -p $projectPath -s $startupPath -o "$projectPath\Migrations"

#
# Website Data Sqlite Old
#

$projectPath = "$scriptPath\Samples\Website\Speedy.Website.Data.Sqlite.Old"
$startupPath = "$scriptPath\Samples\Website\Speedy.Website"

if (!(Test-Path $projectPath))
{
	throw "$projectPath does not exists"	
}

if (!(Test-Path $startupPath))
{
	throw "$startupPath does not exists"	
}

Remove-Item "$projectPath\Migrations" -ErrorAction Ignore
dotnet ef migrations add InitialMigration --framework "netcoreapp3.1" --configuration "debug" -c "ContosoSqliteDatabase" -p $projectPath -s $startupPath -o "$projectPath\Migrations"
