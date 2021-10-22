#Requires -RunAsAdministrator

param (
    [Parameter(Mandatory = $false)]
    [string] $SitePath = "C:\inetpub\Speedy",
    [Parameter()]
	[string] $Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$siteName = "Speedy"
$watch = [System.Diagnostics.Stopwatch]::StartNew()
$scriptPath = Split-Path(Get-Variable MyInvocation).Value.MyCommand.Path
#$scriptPath = "C:\Workspaces\GitHub\Speedy"

& nuget.exe restore "$scriptPath\Speedy.sln"

if ($LASTEXITCODE -ne 0)
{
	Write-Host "Nuget pull has failed! " $watch.Elapsed -ForegroundColor Red
	exit $LASTEXITCODE
}

# Visual Studio Online Support
$msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"

if (!(Test-Path $msbuild -PathType Leaf))
{
	$msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
}

if (!(Test-Path $msbuild -PathType Leaf))
{
	$msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Preview\Msbuild\Current\Bin\MSBuild.exe"
}

& $msbuild "$scriptPath\Speedy.Website\Speedy.Website.csproj" /p:Configuration="$Configuration" /p:PublishProfile=localhost /p:DeployOnBuild=True /p:VisualStudioVersion=16.0 /t:Rebuild /v:m

if ($LASTEXITCODE -ne 0)
{
	Write-Error "Build has failed ($LASTEXITCODE)! $($watch.Elapsed)"
	exit $LASTEXITCODE
}

Write-Host "Deployed Completed" -ForegroundColor Yellow