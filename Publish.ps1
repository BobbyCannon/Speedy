$files = Get-Item "C:\Binaries\Speedy\*.nupkg"
foreach ($file in $files) {
	& "nuget.exe" push $file.FullName
}