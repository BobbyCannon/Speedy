param (
    [Parameter()]
    [string] $Configuration = "Release"
)

$watch = [System.Diagnostics.Stopwatch]::StartNew()
$scriptPath = Split-Path (Get-Variable MyInvocation).Value.MyCommand.Path 
Set-Location $scriptPath
$productName = "Speedy";
$destination = "C:\Binaries\$productName"
$nugetDestination = "C:\Workspaces\Nuget\Developer"

if (Test-Path $destination -PathType Container){
    Remove-Item $destination -Recurse -Force
}

New-Item $destination -ItemType Directory | Out-Null
New-Item $destination\Bin -ItemType Directory | Out-Null

if (!(Test-Path $nugetDestination -PathType Container)){
    New-Item $nugetDestination -ItemType Directory | Out-Null
}

$build = [Math]::Floor([DateTime]::UtcNow.Subtract([DateTime]::Parse("01/01/2000").Date).TotalDays)
$revision = [Math]::Floor([DateTime]::UtcNow.TimeOfDay.TotalSeconds / 2)

.\IncrementVersion.ps1 -Build $build -Revision $revision

& nuget.exe restore "$scriptPath\$productName.sln"
$msbuild = "C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe"
& $msbuild "$scriptPath\$productName.sln" /p:Configuration="$Configuration" /p:Platform="Any CPU" /t:Rebuild /p:VisualStudioVersion=14.0 /v:m /m

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build has failed! " $watch.Elapsed -ForegroundColor Red
    exit $LASTEXITCODE
}

Set-Location $scriptPath

Copy-Item "$productName\bin\$Configuration\$productName.dll" "$destination\bin\"
Copy-Item "$productName\bin\$Configuration\$productName.pdb" "$destination\bin\"
Copy-Item "$productName.EntityFramework\bin\$Configuration\$productName.EntityFramework.dll" "$destination\bin\"
Copy-Item "$productName.EntityFramework\bin\$Configuration\$productName.EntityFramework.pdb" "$destination\bin\"

$versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$destination\bin\$productName.dll")
$version = $versionInfo.FileVersion.ToString()

& "NuGet.exe" pack "$productName.nuspec" -Prop Configuration="$Configuration" -Version $version
Move-Item "$productName.$version.nupkg" "$destination\$productName.$version.nupkg" -force
Copy-Item "$destination\$productName.$version.nupkg" "$nugetDestination" -force

$content = Get-Content "$productName.EntityFramework.nuspec" -Raw
$key = "id=`"$productName`" version=`""
$index = $content.IndexOf($key)
if ($index -ge 0) {
	$index = $index + $key.length
	$index2 = $content.IndexOf("`"", $index)
	$content = $content.Replace($content.SubString($index, $index2 - $index), $version)
	Set-Content "$productName.EntityFramework.nuspec" $content
}

& "nuget.exe" pack "$productName.EntityFramework.nuspec" -Prop Configuration="$Configuration" -Version $version
Move-Item "$productName.EntityFramework.$version.nupkg" "$destination" -force
Copy-Item "$destination\$productName.EntityFramework.$version.nupkg" "$nugetDestination" -force

Write-Host
Set-Location $scriptPath
Write-Host "$productName Build: " $watch.Elapsed -ForegroundColor Yellow