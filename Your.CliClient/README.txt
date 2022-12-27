Zertifikat Erstellung/Installation/Verwendung
Erstellung: Windows-PowerShell (Requirements: Admin-Modus, at least Windows11)
     Gemäss: https://learn.microsoft.com/en-us/azure/active-directory/develop/howto-create-self-signed-certificate
     $cnName = "localhost"    ## Replace with desired value
     $dnsName = "WS777"    ## Replace with desired value
     $cert = New-SelfSignedCertificate -Subject "CN=$cnName" -DnsName "$dnsName" -CertStoreLocation "Cert:\CurrentUser\My" -KeyExportPolicy Exportable -KeySpec Signature -KeyLength 2048 -KeyAlgorithm RSA -HashAlgorithm SHA256
     $mypwd = ConvertTo-SecureString -String "{myPassword}" -Force -AsPlainText  ## Replace {myPassword} with desired value
     Export-PfxCertificate -Cert $cert -FilePath "C:\Users\alpha\Downloads\localhostClientServer.pfx" -Password $mypwd   ## Specify your preferred location
     Hinweise:
         Das Certifikat wird als "Client Authentication" und "Server Authentication" erstellt und kann daher für Client und Server verwendet werden.
         Das Certifikat wird im Store "Certificates - Current User" unter Personal addiert.
Installation
     Server Authentication
         Obiges Zertifikat "via" File im Store "Certificates - Local computer" unter "Personal" importieren.
         --> Dient als Server Certificate für die ServerAuthentication gegenüber dem Client. 
         Obiges Zertifikat "via" File im Store "Certificates - Current User" unter "Trustet Root Certification Authorities" importieren.
         --> Damit der Client dem "ServerCertificate" vertraut muss es unter die "Trustet Root Certification Authorities". 
     Client Authentication
         Obiges Zertifikat "via" File im Store "Certificates - Current User" unter "Personal" importieren.
         --> Dient als Client Certificate für die CertificateAuthentication gegenüber dem Server. 
Verwendung
     Server: Obiges Zertifikatfile verwenden.
     Client: Obiges Zertifikatfile verwenden.
