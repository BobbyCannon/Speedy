$dnsname = "speedy.local"
$cert = Get-ChildItem cert:\LocalMachine\My -Recurse | Where { $_.FriendlyName -eq $dnsname }
$rootcert = Get-ChildItem cert:\LocalMachine\Root -Recurse | Where { $_.FriendlyName -eq $dnsname }

if ($cert -eq $null) {
	Write-Host "Creating new self signed certificate"
	$cert = New-SelfSignedCertificate -FriendlyName $dnsname -KeyFriendlyName $dnsname -Subject $dnsname -DnsName $dnsname
}
	
if ($rootcert -eq $null) {
	Write-Host "Adding new self signed certificate to trusted root certificates"
	
	$store = New-Object System.Security.Cryptography.X509Certificates.X509Store "Root","LocalMachine"
	$store.Open("ReadWrite")
	$store.Add($cert)
	$store.Close()
	
	$bytes = $cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx, "123456")
	$certpath = [System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::Desktop) + "\" + $dnsname + ".pfx"
	Set-Content -Path $certpath -Value $bytes -Encoding Byte

	$cert | Remove-Item
	$cert.GetCertHashString()
}