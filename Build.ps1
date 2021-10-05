param
(
	[Parameter()]
	[string] $Configuration = "Release",
	[Parameter()]
	[string] $BuildNumber = "+",
	[Parameter()]
	[string] $VersionSuffix = ""
)

$ErrorActionPreference = "Stop"
$watch = [System.Diagnostics.Stopwatch]::StartNew()
$scriptPath = $PSScriptRoot
#$scriptPath = "C:\Workspaces\GitHub\Speedy"
$productName = "Speedy"

if ($scriptPath.Length -le 0)
{
	$scriptPath = "C:\Workspaces\GitHub\$productName"
}

$destination = "$scriptPath\Binaries"
$destination2 = "C:\Workspaces\Nuget\Development"

Push-Location $scriptPath

if (Test-Path $destination -PathType Container)
{
	Remove-Item $destination -Recurse -Force
}

New-Item $destination -ItemType Directory | Out-Null
New-Item $destination\Bin -ItemType Directory | Out-Null

if (!(Test-Path $destination2 -PathType Container))
{
	New-Item $destination2 -ItemType Directory | Out-Null
}

try
{
	# Prepare the build for versioning!
	# $newVersion = .\IncrementVersion.ps1 -Build +
	$newVersion = .\IncrementVersion.ps1 -Build $BuildNumber
	$newVersion
	
	& nuget.exe restore "$scriptPath\$productName.sln"

	$msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
	
	if (!(Test-Path $msbuild -PathType Leaf))
	{
		$msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Preview\Msbuild\Current\bin\MSBuild.exe"
	}
	
	& $msbuild "$scriptPath\$productName.sln" /p:Configuration="$Configuration" /p:Platform="Any CPU" /t:Rebuild /v:m /m

	if ($LASTEXITCODE -ne 0)
	{
		Write-Host "Build has failed! " $watch.Elapsed -ForegroundColor Red
		exit $LASTEXITCODE
	}

	Copy-Item "$productName\bin\$Configuration\netstandard2.0\$productName.dll" "$destination\bin\"
	Copy-Item "$productName\bin\$Configuration\netstandard2.0\$productName.pdb" "$destination\bin\"
	
	$versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$destination\bin\$productName.dll")
	$nugetVersion = $versionInfo.ProductVersionRaw.ToString(3)

	if ($VersionSuffix.Length -gt 0)
	{
		$nugetVersion = "$nugetVersion-$VersionSuffix"
	}

	Copy-Item "$productName\bin\$Configuration\$productName.$nugetVersion.nupkg" "$destination\"
	Copy-Item "$productName.EntityFramework\bin\$Configuration\$productName.EntityFramework.$nugetVersion.nupkg" "$destination\"

	Copy-Item "$productName\bin\$Configuration\$productName.$nugetVersion.nupkg" "$destination2\"
	Copy-Item "$productName.EntityFramework\bin\$Configuration\$productName.EntityFramework.$nugetVersion.nupkg" "$destination2\"

	if ($versionInfo.FileVersion.ToString() -ne $newVersion)
	{
		Write-Error "The new version $($versionInfo.FileVersion.ToString()) does not match the expected version ($newVersion)"
	}

	Write-Host
	Set-Location $scriptPath
	Write-Host "Build: " $watch.Elapsed -ForegroundColor Yellow
}
catch
{
	Write-Host $_.Exception.ToString() -ForegroundColor Red
	Write-Host "Build Failed:" $watch.Elapsed -ForegroundColor Red
	exit $LASTEXITCODE
}
finally
{
	Pop-Location
}