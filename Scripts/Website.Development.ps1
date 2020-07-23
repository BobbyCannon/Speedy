Import-Module WebAdministration
iisreset

Restart-WebAppPool -Name "Speedy"
Get-Process vbcs* | Stop-Process
iisreset.exe

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
