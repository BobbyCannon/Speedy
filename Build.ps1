param (
    [Parameter()]
    [string] $Configuration = "Release"
)

$watch = [System.Diagnostics.Stopwatch]::StartNew()
$scriptPath = Split-Path (Get-Variable MyInvocation).Value.MyCommand.Path 
Set-Location $scriptPath
$destination = "C:\Binaries\Speedy"
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

$msbuild = "C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe"
& $msbuild "$scriptPath\Speedy.sln" /p:Configuration="$Configuration" /p:Platform="Any CPU" /t:Rebuild /p:VisualStudioVersion=14.0 /v:m /m

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build has failed! " $watch.Elapsed -ForegroundColor Red
    exit $LASTEXITCODE
}

Set-Location $scriptPath

Copy-Item Speedy\bin\$Configuration\Speedy.dll $destination\bin\
Copy-Item Speedy\bin\$Configuration\Speedy.pdb $destination\bin\

$versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$destination\bin\Speedy.dll")
$version = $versionInfo.FileVersion.ToString()

& "NuGet.exe" pack Speedy.nuspec -Prop Configuration="$Configuration" -Version $version
Move-Item "Speedy.$version.nupkg" "$destination\Speedy.$version.nupkg" -force
Copy-Item "$destination\Speedy.$version.nupkg" "$nugetDestination" -force

Write-Host
Set-Location $scriptPath
Write-Host "Speedy Build: " $watch.Elapsed -ForegroundColor Yellow