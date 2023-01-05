$ErrorActionPreference = 'Stop'

This should not be ran as a file

Import-Module WebAdministration
iisreset

Clear-Host
Restart-WebAppPool -Name "Speedy"
Get-Process vbcs* | Stop-Process

& 'C:\Workspaces\GitHub\Speedy\Deploy.ps1' -Configuration "Debug" -TargetFramework 'net7.0'
& 'C:\Workspaces\GitHub\Speedy\Deploy.ps1' -Configuration "Debug" -TargetFramework 'net6.0'
& 'C:\Workspaces\GitHub\Speedy\Deploy.ps1' -Configuration "Debug" -TargetFramework 'netcoreapp3.1'


# Open-File 'C:\Workspaces\GitHub\Speedy\Deploy.ps1'

& 'C:\Workspaces\GitHub\Speedy\Build.ps1' -Configuration "Release" -BuildNumber 0
& 'C:\Workspaces\GitHub\Speedy\Build.ps1' -Configuration "Release" -BuildNumber 0 -VersionSuffix 'RC9'
& 'C:\Workspaces\GitHub\Speedy\Build.ps1' -Configuration "Release" -BuildNumber 0 -VersionSuffix 'pre'

# Open-File 'C:\Workspaces\GitHub\Speedy\Build.ps1'

#
# Releasing / Nugets
#

# Remove all Pre-release from development
Get-ChildItem "C:\Workspaces\Nuget\Development\" -Filter "Speedy.*-pre.nupkg"
Get-ChildItem "C:\Workspaces\Nuget\Development\" -Filter "Speedy.*-pre.nupkg" | Remove-Item

# Open the nuget folder
ii 'C:\Workspaces\Nuget\Development'
ii 'C:\Workspaces\Nuget\Release'

# Build a final release
& 'C:\Workspaces\GitHub\Speedy\Build.ps1' -BuildNumber 0 -Configuration Release



#
# Web and Database 
#

Remove-Item "C:\inetpub\Speedy" -Recurse -Force

Get-EnvironmentVariable -Target Machine -Variable "ASPNETCORE_ENVIRONMENT"
Set-EnvironmentVariable -Target Machine -Variable "ASPNETCORE_ENVIRONMENT" -Value "Development"
Set-EnvironmentVariable -Target Machine -Variable "ASPNETCORE_ENVIRONMENT" -Value "Release"

Invoke-SqlNonQuery -Database "master" -Query "ALTER DATABASE [Speedy] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Speedy]" -ErrorAction Ignore
Invoke-SqlNonQuery -Database "master" -Query "ALTER DATABASE [Speedy2] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Speedy2]" -ErrorAction Ignore
Invoke-SqlNonQuery -Database "master" -Query "ALTER DATABASE [Sample] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Sample]" -ErrorAction Ignore
Invoke-SqlNonQuery -Database "master" -Query "ALTER DATABASE [Sample2] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Sample2]" -ErrorAction Ignore

Invoke-WebRequest -Uri "https://speedy.local"

#
# Automation Stuff
#

& "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\UIAVerify\VisualUIAVerifyNative.exe"
& "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x86\UIAVerify\VisualUIAVerifyNative.exe"


#
# Build for some Automation Test requirements
#

$msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Professional\Msbuild\Current\Bin\MSBuild.exe"
$configuration = "Debug"

Remove-Item "C:\Workspaces\GitHub\Speedy\Speedy.Winforms.Example\bin" -Recurse
Remove-Item "C:\Workspaces\GitHub\Speedy\Speedy.Winforms.Example\obj" -Recurse

Get-Item "C:\Workspaces\GitHub\Speedy\Speedy.Winforms.Example\bin\Debug"

Clear-Host

Get-Process msbuild | Stop-Process

& nuget.exe restore "C:\Workspaces\GitHub\Speedy\Speedy.sln"

& $msbuild "C:\Workspaces\GitHub\Speedy\Speedy.Winforms.Example\Speedy.Winforms.Example.csproj" /p:Configuration=$configuration /p:Platform=x86 /p:TargetFramework="net48" /t:Rebuild /v:m
& $msbuild "C:\Workspaces\GitHub\Speedy\Speedy.Winforms.Example\Speedy.Winforms.Example.csproj" /p:Configuration=$configuration /p:Platform=x64 /p:TargetFramework="net48" /t:Rebuild /v:m
& $msbuild "C:\Workspaces\GitHub\Speedy\Speedy.Winforms.Example\Speedy.Winforms.Example.csproj" /p:Configuration=$configuration /p:Platform=x86 /p:TargetFramework="net5.0-windows" /t:Rebuild /v:m
& $msbuild "C:\Workspaces\GitHub\Speedy\Speedy.Winforms.Example\Speedy.Winforms.Example.csproj" /p:Configuration=$configuration /p:Platform=x64 /p:TargetFramework="net5.0-windows" /t:Rebuild /v:m
& $msbuild "C:\Workspaces\GitHub\Speedy\Speedy.Winforms.Example\Speedy.Winforms.Example.csproj" /p:Configuration=$configuration /p:Platform=x86 /p:TargetFramework="net6.0-windows" /t:Rebuild /v:m
& $msbuild "C:\Workspaces\GitHub\Speedy\Speedy.Winforms.Example\Speedy.Winforms.Example.csproj" /p:Configuration=$configuration /p:Platform=x64 /p:TargetFramework="net6.0-windows" /t:Rebuild /v:m


#
# Cleanup
#

$tempPath = [System.IO.Path]::GetTempPath()
Get-ChildItem *.db -Path "$tempPath\Speedy\" -Recurse | Remove-Item
Get-ChildItem *.db -Path "$tempPath\SpeedyTests\" -Recurse | Remove-Item
Get-ChildItem *.db -Path "C:\Workspaces\GitHub\Speedy" -Recurse | Remove-Item

Remove-Item C:\Workspaces\GitHub\Speedy\Speedy.Client.Data\Migrations -Force -Recurse
Remove-Item C:\Workspaces\GitHub\Speedy\Speedy.Website.Data.Sql\Migrations -Force -Recurse
Remove-Item C:\Workspaces\GitHub\Speedy\Speedy.Website.Data.Sqlite\Migrations -Force -Recurse

# Remove All SSL Certificates

$rootcert = Get-ChildItem cert:\LocalMachine\root -Recurse | Where { $_.FriendlyName -eq $dnsname }

if ($rootcert -ne $null)
{
	Write-Host "Removing Certificate..."
	$store = New-Object System.Security.Cryptography.X509Certificates.X509Store "Root", "LocalMachine"
	$store.Open("ReadWrite")
	$store.Remove($rootcert)
	$store.Close()
}

$cert = Get-ChildItem cert:\LocalMachine\WebHosting -Recurse | Where { $_.FriendlyName -eq $dnsname }

if ($cert -ne $null)
{
	Write-Host "Removing Certificate..."
	$store = New-Object System.Security.Cryptography.X509Certificates.X509Store "WebHosting", "LocalMachine"
	$store.Open("ReadWrite")
	$store.Remove($cert)
	$store.Close()
}
