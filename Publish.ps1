param
(
	[Parameter()]
	[Switch] $Preview
)

$scriptPath = $PSScriptRoot
$files = Get-Item "$scriptPath\Binaries\*.nupkg"

foreach ($file in $files)
{
	Write-Host $file.FullName -ForegroundColor Cyan

	if ($Preview.IsPresent)
	{
		continue
	}
	
	& "nuget.exe" push $file.FullName -Source https://www.nuget.org/api/v2/package
}