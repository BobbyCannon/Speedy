$ErrorActionPreference = "Stop"
$watch = [System.Diagnostics.Stopwatch]::StartNew()
$scriptPath = $PSScriptRoot
#$scriptPath = (Get-Location).Path
$productName = "Speedy";
$destination = "$scriptPath\Binaries"

Push-Location $scriptPath

function RunTest {
	param ($FilePath,$Filter)
	
	Write-Host $FilePath -Foreground Yellow
	
	$results = Invoke-UnitTests -FilePath $FilePath -Filter $Filter

	Write-Host "Processed $($results.Count) tests, Passed: $($($results | where { $_.Success }).Count), Failed: $($($results | where { !$_.Success }).Count)"
	
	$failures = $results | Where { !$_.Success }
	
	foreach ($failure in $failures) {
		Write-Host "$($failure.TestClass).$($failure.TestName)" -ForegroundColor Red
		#Write-Host "`t`t$($failure.Output.Replace(`"`r`n`", `"`"))"
		Write-Host "`t`t$($failure.Output)"
	}
}

RunTest -FilePath "$destination\Speedy.Tests\Speedy.Tests.dll"
RunTest -FilePath "$destination\Speedy.Samples.Tests\Speedy.Samples.Tests.dll"