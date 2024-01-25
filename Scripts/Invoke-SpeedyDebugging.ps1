param
(
	[Parameter()]
	[string] $ProjectPath,
	[Parameter()]
	[switch] $Rollback,
	[Parameter()]
	[string] $Version,
	[Parameter()]
	[string] $VersionSuffix = ""
)

$ErrorActionPreference = "STOP"
$scriptPath = $PSScriptRoot.Replace("\Scripts", "")

# $Rollback = $true
# $scriptPath = "C:\Workspaces\EpicCoders\Speedy"
# $scriptPath = "C:\Workspaces\GitHub\Speedy"
# $Version = "12.0.0.0"
# $VersionSuffix = "RC2"
# $Framework = "netstandard2.0"

function Format-XmlContent
{
	[CmdletBinding()]
	Param (
		[Parameter(ValueFromPipeline=$true, Mandatory=$true)]
		[string] $xmlcontent,
		[Parameter(ValueFromPipeline=$false, Mandatory=$false)]
		[switch] $indented
	)
	
	$xmldoc = New-Object -TypeName System.Xml.XmlDocument
	$xmldoc.LoadXml($xmlcontent)
	$sw = New-Object System.IO.StringWriter
	$writer = New-Object System.Xml.XmlTextwriter($sw)
	$writer.Formatting = [System.XML.Formatting]::None
	$writer.IndentChar = "`t"
	$writer.Indentation = 1

	if ($indented.IsPresent) {
		$writer.Formatting = [System.XML.Formatting]::Indented
	}

	$xmldoc.WriteContentTo($writer)
	$sw.ToString()
}


if (($Version -ne $null) -and ($Version.Length -gt 0)) {
	Write-Host "Processing $Version..."
	$versionFull = $Version
	$version = $versionFull.Substring(0, $versionFull.LastIndexOf("."))
} else {
	$file = ([System.IO.FileInfo] "$scriptPath\Speedy\Speedy.csproj")
	$fileXml = [xml](Get-Content $file.FullName -Raw)

	$versionFull = $fileXml.Project.PropertyGroup.AssemblyVersion.ToString()
	Write-Host "Processing $versionFull..."
	$version = $versionFull.Substring(0, $versionFull.LastIndexOf("."))
}

if ($VersionSuffix.Length -gt 0)
{
	$version = "$version-$VersionSuffix"
}

$speedyProjects = @{
	"Speedy" = "netstandard2.0;net6.0;net7.0;net8.0";
	"Speedy.Application" = "net8.0-android;net8.0-ios;net8.0-maccatalyst;net8.0-windows10.0.19041.0";
	"Speedy.Application.Maui" = "net8.0-android;net8.0-ios;net8.0-maccatalyst;net8.0-windows10.0.19041.0";
	"Speedy.Application.Uwp" = "";
	"Speedy.Application.Web" = "net48;netcoreapp3.1;net6.0;net7.0;net8.0";
	"Speedy.Application.Wpf" = "net48;netcoreapp3.1;net6.0-windows10.0.19041.0;net7.0-windows10.0.19041.0;net8.0-windows10.0.19041.0";
	"Speedy.Application.Xamarin" = "netstandard2.0;MonoAndroid12.0;uap10.0.19041;Xamarin.iOS10;net6.0-windows10.0.19041.0;net7.0-windows10.0.19041.0;net8.0-windows10.0.19041.0";
	"Speedy.Automation" = "netstandard2.0;netstandard2.1;net48;net6.0;net7.0;net8.0";
	"Speedy.EntityFramework" = "netstandard2.0;net6.0;net7.0;net8.0";
	"Speedy.ServiceHosting" = "netstandard2.0;net6.0-windows10.0.19041.0;net7.0-windows10.0.19041.0;net8.0-windows10.0.19041.0";
}

# & 'C:\Workspaces\GitHub\Speedy\Scripts\Invoke-SpeedyDebugging.ps1' -ProjectPath 'C:\Workspaces\Flare Solution' -Version 12.0.1.0

Write-Host "Version $version"
Write-Host "Getting projects..."

$files = Get-ChildItem $ProjectPath *.csproj -Recurse | Select-Object Fullname

foreach ($file in $files)
{
	$directory = [System.IO.Path]::GetDirectoryName($file.FullName)
	$data = Get-Content $file.FullName -Raw | Format-XmlContent
	
	if (!$data.ToString().Contains("Speedy"))
	{
		# this project doesn't referenc speedy so ignore it
		continue
	}
	
	Write-Host $file.FullName -ForegroundColor Cyan
	
	# Locate all item groups
		
	# Detect Frameworks
		
	$data = Format-XmlContent -xmlcontent $data -indented
		
	#Set-Content $file.FullName -Value $data -Encoding UTF8
	$Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False
	[System.IO.File]::WriteAllLines($file.FullName, $data, $Utf8NoBomEncoding)
}