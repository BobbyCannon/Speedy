Get-Process vbcs* -ErrorAction Ignore | Stop-Process
Get-Process dotnet* -ErrorAction Ignore | Stop-Process

(Get-IISAppPool -Name Speedy).Recycle()
