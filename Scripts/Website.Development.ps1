Import-Module WebAdministration
Restart-WebAppPool -Name "Speedy"
iisreset

Get-Process vbcs* | Stop-Process

Get-EnvironmentVariable -Target Machine -Variable "ASPNETCORE_ENVIRONMENT"
Set-EnvironmentVariable -Target Machine -Variable "ASPNETCORE_ENVIRONMENT" -Value "Development"
Set-EnvironmentVariable -Target Machine -Variable "ASPNETCORE_ENVIRONMENT" -Value "Release"

Invoke-SqlNonQuery -Database "master" -Query "DROP DATABASE [Speedy]"
Invoke-SqlNonQuery -Database "master" -Query "DROP DATABASE [Sample]"
Invoke-SqlNonQuery -Database "master" -Query "DROP DATABASE [Sample2]"

Remove-Item C:\Workspaces\GitHub\Speedy\Speedy.Samples.Sql\Migrations -Force -Recurse
Remove-Item C:\Workspaces\GitHub\Speedy\Speedy.Samples.Sqlite\Migrations -Force -Recurse
