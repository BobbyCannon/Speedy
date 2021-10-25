param
(
	[Parameter()]
	[string] $ProjectPath,
	[Parameter()]
	[switch] $Rollback
)

$ErrorActionPreference = "STOP"
$scriptPath = $PSScriptRoot
#$scriptPath = "C:\Workspaces\EpicCoders\Speedy"

Clear-Host

$versionFull = "9.0.0.0"
$version = "9.0.0"
$files = Get-ChildItem $ProjectPath *.csproj -Recurse | Select-Object Fullname

$speedyPR = "<PackageReference Include=`"Speedy`" Version=`"$version`" />"
$speedyPR2 = "<PackageReference Include=`"Speedy`"><Version>$version</Version></PackageReference>"

$speedyPER = "<PackageReference Include=`"Speedy.EntityFramework`" Version=`"$version`" />"
$speedyPER2 = "<PackageReference Include=`"Speedy.EntityFramework`"><Version>$version</Version></PackageReference>"

$speedyPSR = "<PackageReference Include=`"Speedy.ServiceHosting`" Version=`"$version`" />"
$speedyPSR2 = "<PackageReference Include=`"Speedy.ServiceHosting`"><Version>$version</Version></PackageReference>"
$speedyNR = "<Reference Include=`"Speedy`"><HintPath>$scriptPath\Speedy.EntityFramework\bin\Debug\Speedy.dll</HintPath></Reference>"
$speedyNER = "<Reference Include=`"Speedy.EntityFramework`"><HintPath>$scriptPath\Speedy.EntityFramework\bin\Debug\Speedy.EntityFramework.dll</HintPath></Reference>"
$speedyNSR = "<Reference Include=`"Speedy.ServiceHosting`"><HintPath>$scriptPath\Speedy.ServiceHosting\bin\Debug\Speedy.ServiceHosting.dll</HintPath></Reference>"

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