param (
    [Parameter()]
    [switch] $IncludeDocumentation,
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
New-Item $destination\bin -ItemType Directory | Out-Null

if (!(Test-Path $nugetDestination -PathType Container)){
    New-Item $nugetDestination -ItemType Directory | Out-Null
}

$build = [Math]::Floor([DateTime]::UtcNow.Subtract([DateTime]::Parse("01/01/2000").Date).TotalDays)
$revision = [Math]::Floor([DateTime]::UtcNow.TimeOfDay.TotalSeconds / 2)

.\IncrementVersion.ps1 Speedy $build $revision
.\IncrementVersion.ps1 Speedy.Benchmarks $build $revision
.\IncrementVersion.ps1 Speedy.Tests $build $revision

$msbuild = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
cmd /c $msbuild "$scriptPath\Speedy.sln" /p:Configuration="$Configuration" /p:Platform="Any CPU" /t:Rebuild /p:VisualStudioVersion=12.0 /v:m /m

Set-Location $scriptPath

Copy-Item Speedy\bin\$Configuration\Speedy.dll $destination\bin\

$versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$destination\bin\Speedy.dll")
$version = $versionInfo.FileVersion.ToString()

cmd /c ".nuget\NuGet.exe" pack Speedy.nuspec -Prop Configuration="$Configuration" -Version $version
Move-Item "Speedy.$version.nupkg" "$destination" -force
Copy-Item "$destination\Speedy.$version.nupkg" "$nugetDestination" -force

.\ResetAssemblyInfos.ps1

Write-Host
Set-Location $scriptPath
Write-Host "Speedy Build: " $watch.Elapsed