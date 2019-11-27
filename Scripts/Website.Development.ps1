Import-Module WebAdministration
Restart-WebAppPool -Name "Speedy"
iisreset

Get-Process vbcs* | Stop-Process

Get-EnvironmentVariable -Target Machine -Variable "ASPNETCORE_ENVIRONMENT"
Set-EnvironmentVariable -Target Machine -Variable "ASPNETCORE_ENVIRONMENT" -Value "Development"
Set-EnvironmentVariable -Target Machine -Variable "ASPNETCORE_ENVIRONMENT" -Value "Release"

Invoke-SqlNonQuery -Database "master" -Query "ALTER DATABASE [Speedy] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Speedy]"
Invoke-SqlNonQuery -Database "master" -Query "ALTER DATABASE [Speedy2] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Speedy2]"
Invoke-SqlNonQuery -Database "master" -Query "ALTER DATABASE [Sample] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Sample]"
Invoke-SqlNonQuery -Database "master" -Query "ALTER DATABASE [Sample2] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Sample2]"

$tempPath = [System.IO.Path]::GetTempPath()
Get-ChildItem  -Filter *.db -Directory "$tempPath\SpeedyTests\" -Recurse

Remove-Item C:\Workspaces\GitHub\Speedy\Speedy.Samples.Sql\Migrations -Force -Recurse
Remove-Item C:\Workspaces\GitHub\Speedy\Speedy.Samples.Sqlite\Migrations -Force -Recurse
