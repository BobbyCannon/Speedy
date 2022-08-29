#Requires -RunAsAdministrator

param (
    [Parameter(Mandatory = $false)]
    [string] $SitePath = "C:\inetpub\Speedy",
    [Parameter()]
    [string] $Configuration = "Release",
    [Parameter()]
    [string] $TargetFramework = "net6.0"
)

$ErrorActionPreference = "Stop"
$siteName = "Speedy"
$productName = "Speedy"
$watch = [System.Diagnostics.Stopwatch]::StartNew()
$scriptPath = Split-Path(Get-Variable MyInvocation).Value.MyCommand.Path
#$scriptPath = "C:\Workspaces\EpicCoders\$productName"

& nuget.exe restore "$scriptPath\Speedy.sln"

if ($LASTEXITCODE -ne 0)
{
	Write-Host "Nuget pull has failed! " $watch.Elapsed -ForegroundColor Red
	exit $LASTEXITCODE
}

# Visual Studio Online Support
$msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Professional\Msbuild\Current\Bin\MSBuild.exe"

& $msbuild "$scriptPath\Speedy.Website\Speedy.Website.csproj" /p:Configuration="$Configuration" /p:PublishProfile=localhost /p:DeployOnBuild=True /t:Rebuild /v:m /p:TargetFramework=$TargetFramework

if ($LASTEXITCODE -ne 0)
{
	Write-Error "Build has failed ($LASTEXITCODE)! $($watch.Elapsed)"
	exit $LASTEXITCODE
}

Write-Host "Deployed Completed" -ForegroundColor Yellow