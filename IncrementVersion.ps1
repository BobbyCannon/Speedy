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

function Get-VersionArray {
    param (
        [Parameter(Mandatory=$true,Position=1,ValueFromPipeline=$true)]
        [string] $VersionLine
    )

    $version = $VersionLine | Select-String $assemblyPattern | % { $_.Matches.Value }
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

function Convert-VersionArray {
    param (
        [Parameter(Mandatory=$true,ValueFromPipeline=$true)]
        [object[]] $VersionArray
    )
        
    return "{0}.{1}.{2}.{3}" -f $VersionArray[0], $VersionArray[1], $VersionArray[2], $VersionArray[3]
}

function Set-BuildNumbers {
    param ($desiredPath, $versionNumber)
           
    $files = Get-ChildItem -Path $desiredPath -Filter *.csproj -Recurse
    
    foreach ($file in $files) {
	   	$fileXml = [xml] (Get-Content $file.FullName -Raw)
		
		if ($fileXml.Project.PropertyGroup[0].AssemblyVersion -ne $null) {
    		Write-Verbose $file.FullName
			$fileXml.Project.PropertyGroup[0].AssemblyVersion = $versionNumber
			$fileXml.Project.PropertyGroup[0].FileVersion = $versionNumber
			
			foreach ($group in $fileXml.Project.PropertyGroup) {
				if ($group.Version) {
					$group.Version = $versionNumber
				}
			}
			
    		Set-Content -Path $file.FullName -Value (Format-Xml -Data $fileXml.OuterXml) -Encoding UTF8
    	}
    }
}

try {
	#$scriptPath = "C:\Workspaces\GitHub\Speedy"
    $scriptPath = Split-Path (Get-Variable MyInvocation).Value.MyCommand.Path 
    Set-Location $scriptPath

    $file = ([System.IO.FileInfo] "$scriptPath\Speedy\Speedy.csproj")
    $fileXml = [xml] (Get-Content $file.FullName -Raw)
    $versionArray = Get-VersionArray -VersionLine $fileXml.Project.PropertyGroup.AssemblyVersion[0].ToString()
        
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

    Write-Host "Updating versions to $newVersionNumber"
    Set-BuildNumbers $scriptPath $newVersionNumber

	return $newVersionNumber
    exit $LASTEXITCODE
} 
catch {
    Write-Host $_
    exit -1
}
finally {
    Set-Location $scriptPath
}