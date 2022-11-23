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
$productName = "Speedy"

# $scriptPath = "C:\Workspaces\EpicCoders\$productName"
# $scriptPath = "C:\Workspaces\GitHub\$productName"

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

if (!(Test-Path $destination2 -PathType Container))
{
	New-Item $destination2 -ItemType Directory | Out-Null
}

try
{
	# Prepare the build for versioning!
	# $newVersion = .\IncrementVersion.ps1 -Build +
	$newVersion = .\IncrementVersion.ps1 -Major 11 -Minor 0 -Build $BuildNumber -Revision 0
	$nugetVersion = ([Version] $newVersion).ToString(3)
	
	if ($VersionSuffix.Length -gt 0)
	{
		$nugetVersion = "$nugetVersion-$VersionSuffix"
	}
	
	$projectFiles = "$scriptPath\Speedy\Speedy.csproj", 
		"$scriptPath\Speedy.Automation\Speedy.Automation.csproj",
		"$scriptPath\Speedy.Application\Speedy.Application.csproj",
		"$scriptPath\Speedy.Application.Maui\Speedy.Application.Maui.csproj",
		"$scriptPath\Speedy.Application.WPF\Speedy.Application.WPF.csproj",
		"$scriptPath\Speedy.Application.Xamarin\Speedy.Application.Xamarin.csproj",
		"$scriptPath\Speedy.EntityFramework\Speedy.EntityFramework.csproj",
		"$scriptPath\Speedy.ServiceHosting\Speedy.ServiceHosting.csproj"

	# Set the nuget version
	foreach ($filePath in $projectFiles)
	{
		$fileXml = [xml](Get-Content -Path $filePath)
		$fileXml.Project.PropertyGroup.Version = $nugetVersion
		Set-Content -Path $filePath -Value (Format-Xml $fileXml.OuterXml) -Encoding UTF8
	}

	& nuget.exe restore "$scriptPath\$productName.sln"

	$msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Professional\Msbuild\Current\Bin\MSBuild.exe"
	& $msbuild "$scriptPath\$productName.sln" /p:Configuration="$Configuration" /t:Rebuild /v:m /m

	if ($LASTEXITCODE -ne 0)
	{
		Write-Host "Build has failed! " $LASTEXITCODE $watch.Elapsed -ForegroundColor Red
		exit $LASTEXITCODE
	}

	Copy-Item "$productName\bin\$Configuration\$productName.$nugetVersion.nupkg" "$destination\"
	Copy-Item "$productName.Automation\bin\$Configuration\$productName.Automation.$nugetVersion.nupkg" "$destination\"
	Copy-Item "$productName.Application\bin\$Configuration\$productName.Application.$nugetVersion.nupkg" "$destination\"
	Copy-Item "$productName.Application.Maui\bin\$Configuration\$productName.Application.Maui.$nugetVersion.nupkg" "$destination\"
	Copy-Item "$productName.Application.WPF\bin\$Configuration\$productName.Application.WPF.$nugetVersion.nupkg" "$destination\"
	Copy-Item "$productName.Application.Xamarin\bin\$Configuration\$productName.Application.Xamarin.$nugetVersion.nupkg" "$destination\"
	Copy-Item "$productName.EntityFramework\bin\$Configuration\$productName.EntityFramework.$nugetVersion.nupkg" "$destination\"
	Copy-Item "$productName.ServiceHosting\bin\$Configuration\$productName.ServiceHosting.$nugetVersion.nupkg" "$destination\"
	
	Copy-Item "$productName\bin\$Configuration\$productName.$nugetVersion.nupkg" "$destination\$productName.$nugetVersion.nupkg.zip"
	Copy-Item "$productName.Automation\bin\$Configuration\$productName.Automation.$nugetVersion.nupkg" "$destination\$productName.Automation.$nugetVersion.nupkg.zip"
	Copy-Item "$productName.Application\bin\$Configuration\$productName.Application.$nugetVersion.nupkg" "$destination\$productName.Application.$nugetVersion.nupkg.zip"
	Copy-Item "$productName.Application.Maui\bin\$Configuration\$productName.Application.Maui.$nugetVersion.nupkg" "$destination\$productName.Application.Maui.$nugetVersion.nupkg.zip"
	Copy-Item "$productName.Application.WPF\bin\$Configuration\$productName.Application.WPF.$nugetVersion.nupkg" "$destination\$productName.Application.WPF.$nugetVersion.nupkg.zip"
	Copy-Item "$productName.Application.Xamarin\bin\$Configuration\$productName.Application.Xamarin.$nugetVersion.nupkg" "$destination\$productName.Application.Xamarin.$nugetVersion.nupkg.zip"
	Copy-Item "$productName.EntityFramework\bin\$Configuration\$productName.EntityFramework.$nugetVersion.nupkg" "$destination\$productName.EntityFramework.$nugetVersion.nupkg.zip"
	Copy-Item "$productName.ServiceHosting\bin\$Configuration\$productName.ServiceHosting.$nugetVersion.nupkg" "$destination\$productName.ServiceHosting.$nugetVersion.nupkg.zip"

	Copy-Item "$productName\bin\$Configuration\$productName.$nugetVersion.nupkg" "$destination2\"
	Copy-Item "$productName.Automation\bin\$Configuration\$productName.Automation.$nugetVersion.nupkg" "$destination2\"
	Copy-Item "$productName.Application\bin\$Configuration\$productName.Application.$nugetVersion.nupkg" "$destination2\"
	Copy-Item "$productName.Application.Maui\bin\$Configuration\$productName.Application.Maui.$nugetVersion.nupkg" "$destination2\"
	Copy-Item "$productName.Application.WPF\bin\$Configuration\$productName.Application.WPF.$nugetVersion.nupkg" "$destination2\"
	Copy-Item "$productName.Application.Xamarin\bin\$Configuration\$productName.Application.Xamarin.$nugetVersion.nupkg" "$destination2\"
	Copy-Item "$productName.EntityFramework\bin\$Configuration\$productName.EntityFramework.$nugetVersion.nupkg" "$destination2\"
	Copy-Item "$productName.ServiceHosting\bin\$Configuration\$productName.ServiceHosting.$nugetVersion.nupkg" "$destination2\"

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