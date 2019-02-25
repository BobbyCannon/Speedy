#Requires -RunAsAdministrator

$siteName = "Speedy"
$dnsname = "speedy.local"

$cert = Get-ChildItem cert:\LocalMachine\My -Recurse  | Where {	$_.FriendlyName -eq $dnsname }
$rootcert = Get-ChildItem cert:\LocalMachine\Root -Recurse  | Where { $_.FriendlyName -eq $dnsname }

if (($cert -eq $null) -and ($rootcert -eq $null))
{
	Write-Host "Creating new self signed certificate"
	$cert = New-SelfSignedCertificate -FriendlyName $dnsname -KeyFriendlyName $dnsname -Subject $dnsname -DnsName $dnsname
}

if ($rootcert -eq $null)
{
	Write-Host "Adding new self signed certificate to trusted root certificates"

	$store = New-Object System.Security.Cryptography.X509Certificates.X509Store "Root", "LocalMachine"
	$store.Open("ReadWrite")
	$store.Add($cert)
	$store.Close()

	$bytes = $cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx, "123456")
	$certpath = [System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::Desktop) + "\" + $dnsname + ".pfx"
	Set-Content -Path $certpath -Value $bytes -Encoding Byte

	$cert | Remove-Item
	$cert.GetCertHashString()
}

#
# Provision the website
#

Import-Module WebAdministration

$site = Get-Website $siteName
$sitePath = "C:\inetpub\$siteName"

if ($site -eq $null)
{
	New-Item $sitePath -ItemType Directory -ErrorAction Ignore | Out-Null
	New-Website -Name $siteName -PhysicalPath $sitePath
	
	$site = Get-Website $siteName
	$webPath = "IIS:\Sites\$siteName"
		
	$bindings = @()
	$bindings += @{ protocol="http"; bindingInformation="*:80:$dnsname"}
	$bindings += @{ protocol="https"; bindingInformation="*:443:$dnsname"; sslFlags=1 }
		
	Set-ItemProperty -Path $webPath -Name Bindings -Value $bindings

	$certificate = (Get-ChildItem Cert:\LocalMachine\Root -Recurse | Where { $_.Subject.Contains($dnsname) })
	
	$binding = Get-WebBinding -Name $siteName -Protocol "https"
	$binding.AddSslCertificate($certificate.GetCertHashString(), "Root")
	$pool = Get-Item "IIS:\AppPools\$siteName" -ErrorAction Ignore
	
	if ($pool -eq $null)
	{
	    New-WebAppPool -Name $siteName
	    $pool = Get-Item IIS:\AppPools\$siteName
	}
	
	Set-ItemProperty IIS:\Sites\$siteName -Name applicationPool -Value $siteName
	Start-WebAppPool -Name $siteName
	
	$site.Start()
}
else
{
	Write-Host "Website already created"
}