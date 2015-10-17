 [CmdletBinding(PositionalBinding=$false)] 

 param (
    [Parameter(Mandatory=$false)]
    [string] $Major,
    [Parameter(Mandatory=$false)]
    [string] $Minor,
    [Parameter(Mandatory=$false)]
    [string] $Build,
    [Parameter(Mandatory=$false)]
    [string] $Revision
)

$assemblyPattern = "[0-9]+(\.([0-9]+|\*)){1,3}"
$assemblyVersionPatternCs = "^\[Assembly: AssemblyVersion\(`"$assemblyPattern`"\)"
$assemblyVersionPatternVb = $assemblyVersionPatternCs.Replace("\[", "\<")

# See if the build should be generated.
if ($Build -eq "*") {
    $Build = [Math]::Floor([DateTime]::UtcNow.Subtract([DateTime]::Parse("01/01/2000").Date).TotalDays)
}

# See if the revision should be generated.
if ($Revision -eq "*") {
    $Revision = [Math]::Floor([DateTime]::UtcNow.TimeOfDay.TotalSeconds / 2)
}

function Get-VersionLine
{
    param (
        [Parameter(Mandatory=$true,Position=1,ValueFromPipeline=$true)]
        [System.IO.FileInfo] $file,
        [Parameter(Mandatory=$true,Position=2,ValueFromPipeline=$true)]
        [string] $pattern
    )

    $content = Get-Content $file
    $result = ($content | Select-String -pattern $pattern)

    if ($result.GetType().Name -eq "Object[]") {
        return $result[0].ToString()
    }

    return $result.ToString()
}

function Get-VersionArray 
{
    param (
        [Parameter(Mandatory=$true,Position=1,ValueFromPipeline=$true)]
        [string] $versionLine
    )

    $version = $versionLine | Select-String $assemblyPattern | % { $_.Matches.Value }
    $versionParts = $version.Split('.')

    # Ensure there is a part 3.
    if ($versionParts.Length -lt 3) { 
        $versionParts += 0 
    }
    
    # Ensure there is a part 4.
    if ($versionParts.Length -lt 4) { 
        $versionParts += 0 
    }

    return $versionParts
}

function Convert-VersionArray
{
    param (
        [Parameter(Mandatory=$true,ValueFromPipeline=$true)]
        [object[]] $VersionArray
    )
        
    return "{0}.{1}.{2}.{3}" -f $VersionArray[0], $VersionArray[1], $VersionArray[2], $VersionArray[3]
}

function Set-BuildNumbers
{
    param($desiredPath, $versionNumber)
           
    $foundFiles = Get-ChildItem $desiredPath\*AssemblyInfo.* -Recurse
    if ($foundFiles.Length -le 0)
    {
        Write-Verbose "No files found"
        return
    }

    foreach($file in $foundFiles)
    {   
        Write-Verbose $file.FullName
        
        $versionPattern1 = $assemblyVersionPatternCs
        if ($file.FullName.EndsWith(".vb")) {
            $versionPattern1 = $assemblyVersionPatternVb
        }
                
        $oldAssemblyVersionLine = Get-VersionLine $file $versionPattern1
        Write-Verbose $oldAssemblyVersionLine
        $oldAssemblyVersion = ,($oldAssemblyVersionLine | Get-VersionArray) | Convert-VersionArray
        $newAssemblyVersionLine = $oldAssemblyVersionLine.Replace($oldAssemblyVersion, $newVersionNumber)
        Write-Verbose $newAssemblyVersionLine
        $versionPattern2 = $versionPattern1.Replace("AssemblyVersion","AssemblyFileVersion")
        $oldAssemblyFileVersionLine = Get-VersionLine $file $versionPattern2
        Write-Verbose $oldAssemblyFileVersionLine
        $oldAssemblyFileVersion = ,($oldAssemblyFileVersionLine | Get-VersionArray) | Convert-VersionArray
        $newAssemblyFileVersionLine = $oldAssemblyFileVersionLine.Replace($oldAssemblyFileVersion, $newVersionNumber)
        Write-Verbose $newAssemblyFileVersionLine
    
        (Get-Content $file).Trim() | % {
            $_.Replace($oldAssemblyVersionLine, $newAssemblyVersionLine).Replace($oldAssemblyFileVersionLine, $newAssemblyFileVersionLine) 
        } | Set-Content $file -Encoding UTF8
    }
}

try {

    $scriptPath = Split-Path (Get-Variable MyInvocation).Value.MyCommand.Path 
    Set-Location $scriptPath

    $file = ([System.IO.FileInfo]"$scriptPath\Speedy\Properties\AssemblyInfo.cs")
    $versionArray = Get-VersionLine $file $assemblyVersionPatternCs | Get-VersionArray
    
    if ($Major -eq "+") {
        $versionArray[0] = ([int]$versionArray[0]) + 1
    } elseif ($Major.Length -gt 0) {
        $versionArray[0] = $Major
    }

    if ($Minor -eq "+") {
        $versionArray[1] = ([int]$versionArray[1]) + 1
    } elseif ($Minor.Length -gt 0) {
        $versionArray[1] = $Minor
    } elseif ($Major.Length -gt 0) {
        $Minor = "0"
        $versionArray[1] = $Minor
    }

    if ($Build -eq "+") {
        $versionArray[2] = ([int]$versionArray[2]) + 1
    } elseif ($Build.Length -gt 0) {
        $versionArray[2] = $Build
    } elseif ($Minor.Length -gt 0) {
        $Build = "0"
        $versionArray[2] = $Build
    }

    if ($Revision -eq "+") {
        $versionArray[3] = ([int]$versionArray[3]) + 1
    } elseif ($Revision.Length -gt 0) {
        $versionArray[3] = $Revision
    } elseif ($Build.Length -gt 0) {
        $Revision = "0"
        $versionArray[3] = $Revision
    }

    if (($Major.Length -le 0) -and ($Minor.Length -le 0) -and ($Build.Length -le 0) -and ($Revision.Length -le 0)) {
        $versionArray[3] = ([int]$versionArray[3]) + 1
    }

    $newVersionNumber = Convert-VersionArray $versionArray

    Write-Host "Updating Assembly Infos to $newVersionNumber"
    Set-BuildNumbers $scriptPath $newVersionNumber

    exit $LASTEXITCODE
} 
catch {
    Write-Host $_
    exit -1
}
finally {
    Set-Location $scriptPath
}