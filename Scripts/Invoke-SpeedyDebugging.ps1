param
(
	[Parameter()]
	[string] $ProjectPath,
	[Parameter()]
	[switch] $Rollback,
	[Parameter()]
	[string] $Version,
	[Parameter()]
	[string] $VersionSuffix = "",
	[Parameter()]
	[string] $Framework
)

if ($Framework -eq $null) {
	$Framework = "netstandard2.0"
}

Clear-Host

$ErrorActionPreference = "STOP"
$scriptPath = $PSScriptRoot.Replace("\Scripts", "")

# $scriptPath = "C:\Workspaces\EpicCoders\Speedy"
# $scriptPath = "C:\Workspaces\GitHub\Speedy"
# $Version = "8.4.8.0"
# $VersionSuffix = "RC1"
# $Framework = "netstandard2.0"


$file = ([System.IO.FileInfo] "$scriptPath\Speedy\Speedy.csproj")
$fileXml = [xml](Get-Content $file.FullName -Raw)

if (($Version -ne $null) -and ($Version.Length -gt 0)) {
	Write-Host "Processing $Version..."
	$versionFull = $Version
	$version = $versionFull.Substring(0, $versionFull.LastIndexOf("."))
} else {
	$versionFull = $fileXml.Project.PropertyGroup.AssemblyVersion.ToString()
	Write-Host "Processing $versionFull..."
	$version = $versionFull.Substring(0, $versionFull.LastIndexOf("."))
}

if ($VersionSuffix.Length -gt 0)
{
	$version = "$version-$VersionSuffix"
}

Write-Host "Version $version"
Write-Host "Getting projects..."

$files = Get-ChildItem $ProjectPath *.csproj -Recurse | Select-Object Fullname
$path = $Framework.ToString()

$projects = "Speedy",
	"Speedy.Automation",
	"Speedy.Application",
	"Speedy.Application.Maui",
	"Speedy.Application.Wpf",
	"Speedy.Application.Xamarin",
	"Speedy.EntityFramework",
	"Speedy.ServiceHosting"
	
$projects

$packageReferences = @()
$packageReferences2 = @()
$oldReferences = @()
$directReferences = @()
$directReferencesMarked = @()

foreach ($project in $projects)
{
	$packageReferences += "<PackageReference Include=`"$project`" Version=`"$version`" />"
	$packageReferences2 += "<PackageReference Include=`"$project`"><Version>$version</Version></PackageReference>"
	$oldReferences += "<Reference Include=`"$project, Version=$versionFull, Culture=neutral, PublicKeyToken=8db7b042d9663bf8, processorArchitecture=MSIL`"><HintPath>..\packages\$project.$version\lib\$Framework\$project.dll</HintPath></Reference>"
	$directReferences += "<Reference Include=`"$project`"><HintPath>$scriptPath\$project\bin\Debug\$Framework\$project.dll</HintPath></Reference>"
	# This allows us to roll back to the old package config direct references to local nuget files
	$directReferencesMarked += "<Reference Include=`"$project`" PackageConfig=`"true`"><HintPath>$scriptPath\$project\bin\Debug\$Framework\$project.dll</HintPath></Reference>"
}

$packageReferences
$packageReferences2
$oldReferences
$directReferences
$directReferencesMarked

foreach ($file in $files)
{
	$directory = [System.IO.Path]::GetDirectoryName($file.FullName)
	$data = Get-Content $file.FullName -Raw | Format-Xml -Minify
	
	if (!$data.ToString().Contains("Speedy"))
	{
		# this project doesn't referenc speedy so ignore it
		continue
	}
	
	# todo: need to support a different Framework when setting Maui, WPF, etc, add framework detection
	
	for ($i = 0; $i -le $packageReferences.Length; $i++)
	{
		if ($Rollback.IsPresent)
		{
			# first try and roll back direct old references to package config version
			$data = $data.Replace($directReferencesMarked[$i], $oldReferences[$i])
			
			# then we'll try the new package reference
			$data = $data.Replace($directReferences[$i], $packageReferences[$i])
		}
		else
		{
			# First try and use the old package config
			$data = $data.Replace($oldReferences[$i], $directReferencesMarked[$i])
			
			# Everything else used normal direct references
			$data = $data.Replace($packageReferences[$i], $directReferences[$i])
			$data = $data.Replace($packageReferences2[$i], $directReferences[$i])
		}	
	}
	
	$data = Format-Xml -Data $data -IndentCount 4 -IndentCharacter ' '
	$file.FullName
	
	#Set-Content $file.FullName -Value $data -Encoding UTF8
	$Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False
	[System.IO.File]::WriteAllLines($file.FullName, $data, $Utf8NoBomEncoding)
}