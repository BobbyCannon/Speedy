param (
    [Parameter(Mandatory = $false, Position = 0)]
    [string] $Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$watch = [System.Diagnostics.Stopwatch]::StartNew()
$scriptPath = Split-Path (Get-Variable MyInvocation).Value.MyCommand.Path 
$productName = "Speedy";
$destination = "C:\Binaries\$productName"
$destination2 = "C:\Workspaces\Nuget\Developer"

Push-Location $scriptPath

if (Test-Path $destination -PathType Container){
    Remove-Item $destination -Recurse -Force
}

New-Item $destination -ItemType Directory | Out-Null
New-Item $destination\Bin -ItemType Directory | Out-Null

if (!(Test-Path $destination2 -PathType Container)) {
    New-Item $destination2 -ItemType Directory | Out-Null
}

try {
	& "ResetAssemblyInfos.ps1"

    #.\IncrementVersion.ps1 -Build +

    & nuget.exe restore "$scriptPath\$productName.sln"

    $msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe"
    & $msbuild "$scriptPath\$productName.sln" /p:Configuration="$Configuration" /p:Platform="Any CPU" /t:Rebuild /p:VisualStudioVersion=15.0 /v:m /m

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build has failed! " $watch.Elapsed -ForegroundColor Red
        exit $LASTEXITCODE
    }

    Copy-Item "$productName\bin\$Configuration\$productName.dll" "$destination\bin\"
    Copy-Item "$productName\bin\$Configuration\$productName.pdb" "$destination\bin\"
    Copy-Item "$productName.EntityFramework\bin\$Configuration\$productName.EntityFramework.dll" "$destination\bin\"
    Copy-Item "$productName.EntityFramework\bin\$Configuration\$productName.EntityFramework.pdb" "$destination\bin\"

    $versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$destination\bin\$productName.dll")
    $build = ([Version] $versionInfo.ProductVersion).Build
    $version = $versionInfo.FileVersion.Replace(".$build.0", ".$build")
    $version = "$version-RC4"

    & "NuGet.exe" pack "$productName.nuspec" -Prop Configuration="$Configuration" -Version $version
    Move-Item "$productName.$version.nupkg" "$destination\$productName.$version.nupkg" -force
    Copy-Item "$destination\$productName.$version.nupkg" "$destination2" -force

    # Put the speedy version in the EF nuget package
    $content = Get-Content "$productName.EntityFramework.nuspec" -Raw
    $key = "id=`"$productName`" version=`""
    $index = $content.IndexOf($key)
    if ($index -ge 0) {
	    $index = $index + $key.length
	    $index2 = $content.IndexOf("`"", $index)
	    $content = $content.Replace($content.SubString($index, $index2 - $index), $version).Trim()
	    Set-Content "$productName.EntityFramework.nuspec" $content
    }

    & "nuget.exe" pack "$productName.EntityFramework.nuspec" -Prop Configuration="$Configuration" -Version $version
    Move-Item "$productName.EntityFramework.$version.nupkg" "$destination" -force
    Copy-Item "$destination\$productName.EntityFramework.$version.nupkg" "$destination2" -force

    Write-Host
    Set-Location $scriptPath
    Write-Host "Build: " $watch.Elapsed -ForegroundColor Yellow
} catch  {
    Write-Host $_.Exception.ToString() -ForegroundColor Red
    Write-Host "Build Failed:" $watch.Elapsed -ForegroundColor Red
    exit $LASTEXITCODE
} finally {
    Pop-Location
}