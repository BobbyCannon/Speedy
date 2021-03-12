param
(
	[Parameter()]
	[string] $ProjectPath,
	[Parameter()]
	[switch] $Rollback
)

$ErrorActionPreference = "STOP"

Clear-Host

$versionFull = "8.0.3.17728"
$version = "8.0.3"
$files = Get-ChildItem $ProjectPath *.csproj -Recurse | Select-Object Fullname

$speedyR = "<Reference Include=`"Speedy, Version=$($versionFull), Culture=neutral, PublicKeyToken=8db7b042d9663bf8, processorArchitecture=MSIL`"><HintPath>..\packages\Speedy.$version\lib\netstandard2.0\Speedy.dll</HintPath></Reference>"
$speedyR2 = "<Reference Include=`"Speedy.EntityFramework, Version=$($versionFull), Culture=neutral, PublicKeyToken=8db7b042d9663bf8, processorArchitecture=MSIL`"><HintPath>..\packages\Speedy.EntityFramework.$version\lib\netstandard2.0\Speedy.EntityFramework.dll</HintPath></Reference>"
$speedyPR = "<PackageReference Include=`"Speedy`" Version=`"$version`" />"
$speedyPR2 = "<PackageReference Include=`"Speedy`"><Version>$version</Version></PackageReference>"
$speedyPER = "<PackageReference Include=`"Speedy.EntityFramework`" Version=`"$version`" />"
$speedyPER2 = "<PackageReference Include=`"Speedy.EntityFramework`"><Version>$version</Version></PackageReference>"
$speedyPCR = "<Reference Include=`"Speedy, Version=$($versionFull), Culture=neutral, PublicKeyToken=8db7b042d9663bf8, processorArchitecture=MSIL`"><HintPath>..\packages\Speedy.$version\lib\netstandard2.0\Speedy.dll</HintPath></Reference>"
$speedyPCER = "<Reference Include=`"Speedy.EntityFramework, Version=$($versionFull), Culture=neutral, PublicKeyToken=8db7b042d9663bf8, processorArchitecture=MSIL`"><HintPath>..\packages\Speedy.EntityFramework.$version\lib\netstandard2.0\Speedy.EntityFramework.dll</HintPath></Reference>"

$speedyNR = "<Reference Include=`"Speedy`"><HintPath>C:\Workspaces\GitHub\Speedy\Speedy.EntityFramework\bin\Debug\netstandard2.0\Speedy.dll</HintPath></Reference>"
$speedyNR2 = "<Reference Include=`"Speedy.EntityFramework`"><HintPath>C:\Workspaces\GitHub\Speedy\Speedy.EntityFramework\bin\Debug\netstandard2.0\Speedy.EntityFramework.dll</HintPath></Reference>"


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
			$data = $data.Replace($speedyNR, $speedyPCR)
			$data = $data.Replace($speedyNR2, $speedyPCER)
		}
		else
		{
			$data = $data.Replace($speedyNR, $speedyPR)
			$data = $data.Replace($speedyNR2, $speedyPER)
		}
	}
	else
	{
		$data = $data.Replace($speedyR, $speedyNR)
		$data = $data.Replace($speedyR2, $speedyNR2)
		$data = $data.Replace($speedyPR, $speedyNR)
		$data = $data.Replace($speedyPR2, $speedyNR)
		$data = $data.Replace($speedyPER, $speedyNR2)
		$data = $data.Replace($speedyPER2, $speedyNR2)
	}
	
	$data = Format-Xml -Data $data -IndentCount 4 -IndentCharacter ' '
	$file.FullName
	
	Set-Content $file.FullName -Value $data -Encoding UTF8
}