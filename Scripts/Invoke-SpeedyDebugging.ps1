param
(
	[Parameter()]
	[string] $ProjectPath,
	[Parameter()]
	[switch] $Rollback
)

$ErrorActionPreference = "STOP"
$scriptPath = $PSScriptRoot.Replace("\Scripts", "")
#$scriptPath = "C:\Workspaces\GitHub\Speedy"

Clear-Host

$file = ([System.IO.FileInfo] "$scriptPath\Speedy\Speedy.csproj")
$fileXml = [xml](Get-Content $file.FullName -Raw)
$versionFull = $fileXml.Project.PropertyGroup.AssemblyVersion.ToString()
$version = $versionFull.Substring(0, $versionFull.LastIndexOf("."))

$files = Get-ChildItem $ProjectPath *.csproj -Recurse | Select-Object Fullname

$speedyPR = "<PackageReference Include=`"Speedy`" Version=`"$version`" />"
$speedyPR2 = "<PackageReference Include=`"Speedy`"><Version>$version</Version></PackageReference>"

$speedyPER = "<PackageReference Include=`"Speedy.EntityFramework`" Version=`"$version`" />"
$speedyPER2 = "<PackageReference Include=`"Speedy.EntityFramework`"><Version>$version</Version></PackageReference>"

$speedyPSR = "<PackageReference Include=`"Speedy.ServiceHosting`" Version=`"$version`" />"
$speedyPSR2 = "<PackageReference Include=`"Speedy.ServiceHosting`"><Version>$version</Version></PackageReference>"

$speedyR = "<Reference Include=`"Speedy, Version=$($versionFull), Culture=neutral, PublicKeyToken=8db7b042d9663bf8, processorArchitecture=MSIL`"><HintPath>..\packages\Speedy.$version\lib\netstandard2.0\Speedy.dll</HintPath></Reference>"
$speedyER = "<Reference Include=`"Speedy.EntityFramework, Version=$($versionFull), Culture=neutral, PublicKeyToken=8db7b042d9663bf8, processorArchitecture=MSIL`"><HintPath>..\packages\Speedy.EntityFramework.$version\lib\netstandard2.0\Speedy.EntityFramework.dll</HintPath></Reference>"
$speedySR = "<Reference Include=`"Speedy.ServiceHosting, Version=$($versionFull), Culture=neutral, PublicKeyToken=8db7b042d9663bf8, processorArchitecture=MSIL`"><HintPath>..\packages\Speedy.ServiceHosting.$version\lib\netstandard2.0\Speedy.ServiceHosting.dll</HintPath></Reference>"

$speedyNR = "<Reference Include=`"Speedy`"><HintPath>$scriptPath\Speedy.EntityFramework\bin\Debug\netstandard2.0\Speedy.dll</HintPath></Reference>"
$speedyNER = "<Reference Include=`"Speedy.EntityFramework`"><HintPath>$scriptPath\Speedy.EntityFramework\bin\Debug\netstandard2.0\Speedy.EntityFramework.dll</HintPath></Reference>"
$speedyNSR = "<Reference Include=`"Speedy.ServiceHosting`"><HintPath>$scriptPath\Speedy.ServiceHosting\bin\Debug\netstandard2.0\Speedy.ServiceHosting.dll</HintPath></Reference>"

foreach ($file in $files)
{
	$directory = [System.IO.Path]::GetDirectoryName($file.FullName)
	$packagePath = $directory + "\packages.config"
	$packageExists = [System.IO.File]::Exists($packagePath)
	$data = Get-Content $file.FullName -Raw | Format-Xml -Minify
		
	if (!$data.ToString().Contains("Speedy"))
	{
		continue
	}
	
	if ($Rollback.IsPresent)
	{
		if ($packageExists)
		{
			$data = $data.Replace($speedyNR, $speedyR)
			$data = $data.Replace($speedyNER, $speedyER)
			$data = $data.Replace($speedyNSR, $speedySR)
		}
		else
		{
			$data = $data.Replace($speedyNR, $speedyPR)
			$data = $data.Replace($speedyNER, $speedyPER)
			$data = $data.Replace($speedyNSR, $speedyPSR)
		}
	}
	else
	{
		$data = $data.Replace($speedyR, $speedyNR)
		$data = $data.Replace($speedyER, $speedyNER)
		$data = $data.Replace($speedySR, $speedyNSR)
		
		$data = $data.Replace($speedyPR, $speedyNR)
		$data = $data.Replace($speedyPR2, $speedyNR)
		$data = $data.Replace($speedyPER, $speedyNER)
		$data = $data.Replace($speedyPER2, $speedyNER)
		$data = $data.Replace($speedyPSR, $speedyNSR)
		$data = $data.Replace($speedyPSR2, $speedyNSR)
	}
	
	$data = Format-Xml -Data $data -IndentCount 4 -IndentCharacter ' '
	$file.FullName
	
	Set-Content $file.FullName -Value $data -Encoding UTF8
}