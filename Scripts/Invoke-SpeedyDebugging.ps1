param
(
	[Parameter()]
	[string] $ProjectPath,
	[Parameter()]
	[switch] $Rollback
)

$ErrorActionPreference = "STOP"

Clear-Host

$versionFull = "9.0.0.0"
$version = "9.0.0"
$files = Get-ChildItem $ProjectPath *.csproj -Recurse | Select-Object Fullname

# Package Reference
$speedyPR = "<PackageReference Include=`"Speedy`" Version=`"$version`" />"
$speedyPER = "<PackageReference Include=`"Speedy.EntityFramework`" Version=`"$version`" />"

$speedyNR = "<Reference Include=`"Speedy`"><HintPath>C:\Workspaces\GitHub\Speedy\Speedy.EntityFramework\bin\Debug\Speedy.dll</HintPath></Reference>"
$speedyNER = "<Reference Include=`"Speedy.EntityFramework`"><HintPath>C:\Workspaces\GitHub\Speedy\Speedy.EntityFramework\bin\Debug\Speedy.EntityFramework.dll</HintPath></Reference>"

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
	}
	else
	{
		$data = $data.Replace($speedyPR, $speedyNR)
		$data = $data.Replace($speedyPER, $speedyNER)
	}
	
	$data = Format-Xml -Data $data -IndentCount 4 -IndentCharacter ' '
	$file.FullName
	
	Set-Content $file.FullName -Value $data -Encoding UTF8
}