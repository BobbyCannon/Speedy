This should not be ran as a file

Import-Module WebAdministration
iisreset

Restart-WebAppPool -Name "Speedy"
Get-Process vbcs* | Stop-Process

& 'C:\Workspaces\GitHub\Speedy\Deploy.ps1' -Configuration "Debug"

Remove-Item "C:\inetpub\Speedy" -Recurse -Force

Get-EnvironmentVariable -Target Machine -Variable "ASPNETCORE_ENVIRONMENT"
Set-EnvironmentVariable -Target Machine -Variable "ASPNETCORE_ENVIRONMENT" -Value "Development"
Set-EnvironmentVariable -Target Machine -Variable "ASPNETCORE_ENVIRONMENT" -Value "Release"

Invoke-SqlNonQuery -Database "master" -Query "ALTER DATABASE [Speedy] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Speedy]"
Invoke-SqlNonQuery -Database "master" -Query "ALTER DATABASE [Speedy2] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Speedy2]"
Invoke-SqlNonQuery -Database "master" -Query "ALTER DATABASE [Sample] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Sample]"
Invoke-SqlNonQuery -Database "master" -Query "ALTER DATABASE [Sample2] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Sample2]"

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
