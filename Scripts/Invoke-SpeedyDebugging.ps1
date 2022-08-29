param
(
	[Parameter()]
	[string] $ProjectPath,
	[Parameter()]
	[switch] $Rollback,
	[Parameter()]
	[string] $Version,
	[Parameter()]
	[switch] $Prerelease,
	[Parameter()]
	[string] $Framework
)

if ($Framework -eq $null) {
	$Framework = "net6.0"
}

Clear-Host

$ErrorActionPreference = "STOP"
$scriptPath = $PSScriptRoot.Replace("\Scripts", "")

#$scriptPath = "C:\Workspaces\EpicCoders\Speedy"
#$scriptPath = "C:\Workspaces\GitHub\Speedy"
#$Version = "8.4.8.0"
#$Framework = "netstandard2.0"

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

if ($Prerelease.IsPresent) {
	$version = "$version-pre"
}

Write-Host "Version $version"
Write-Host "Getting projects..."

$files = Get-ChildItem $ProjectPath *.csproj -Recurse | Select-Object Fullname
$path = $Framework.ToString()

$speedyPR = "<PackageReference Include=`"Speedy`" Version=`"$version`" />"
$speedyPR2 = "<PackageReference Include=`"Speedy`"><Version>$version</Version></PackageReference>"

$speedyPER = "<PackageReference Include=`"Speedy.EntityFramework`" Version=`"$version`" />"
$speedyPER2 = "<PackageReference Include=`"Speedy.EntityFramework`"><Version>$version</Version></PackageReference>"

$speedyPSR = "<PackageReference Include=`"Speedy.ServiceHosting`" Version=`"$version`" />"
$speedyPSR2 = "<PackageReference Include=`"Speedy.ServiceHosting`"><Version>$version</Version></PackageReference>"

$speedyNR = "<Reference Include=`"Speedy`"><HintPath>$scriptPath\Speedy.EntityFramework\bin\Debug\$Framework\Speedy.dll</HintPath></Reference>"
$speedyNER = "<Reference Include=`"Speedy.EntityFramework`"><HintPath>$scriptPath\Speedy.EntityFramework\bin\Debug\$Framework\Speedy.EntityFramework.dll</HintPath></Reference>"
$speedyNSR = "<Reference Include=`"Speedy.ServiceHosting`"><HintPath>$scriptPath\Speedy.ServiceHosting\bin\Debug\$Framework\Speedy.ServiceHosting.dll</HintPath></Reference>"

$speedyR2 = "<Reference Include=`"Speedy, Version=$versionFull, Culture=neutral, PublicKeyToken=8db7b042d9663bf8, processorArchitecture=MSIL`"><HintPath>..\packages\Speedy.$version\lib\netstandard2.0\Speedy.dll</HintPath></Reference>"
$speedyER2 = "<Reference Include=`"Speedy.EntityFramework, Version=$versionFull, Culture=neutral, PublicKeyToken=8db7b042d9663bf8, processorArchitecture=MSIL`"><HintPath>..\packages\Speedy.EntityFramework.$version\lib\netstandard2.0\Speedy.EntityFramework.dll</HintPath></Reference>"

foreach ($file in $files)
{
	$directory = [System.IO.Path]::GetDirectoryName($file.FullName)
	$data = Get-Content $file.FullName -Raw | Format-Xml -Minify
		
	if (!$data.ToString().Contains("Speedy"))
	{
		continue
	}
	
	if ($Rollback.IsPresent)
	{
		$data = $data.Replace($speedyNR, $speedyPR)
		$data = $data.Replace($speedyNER, $speedyPER)
		$data = $data.Replace($speedyNSR, $speedyPSR)
	}
	else
	{
		$data = $data.Replace($speedyR2, $speedyNR)
		$data = $data.Replace($speedyER2, $speedyNER)
		$data = $data.Replace($speedyPR, $speedyNR)
		$data = $data.Replace($speedyPR2, $speedyNR)
		$data = $data.Replace($speedyPER, $speedyNER)
		$data = $data.Replace($speedyPER2, $speedyNER)
		$data = $data.Replace($speedyPSR, $speedyNSR)
		$data = $data.Replace($speedyPSR2, $speedyNSR)
	}
	
	$data = Format-Xml -Data $data -IndentCount 4 -IndentCharacter ' '
	$file.FullName
	
	#Set-Content $file.FullName -Value $data -Encoding UTF8
	$Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False
	[System.IO.File]::WriteAllLines($file.FullName, $data, $Utf8NoBomEncoding)
}