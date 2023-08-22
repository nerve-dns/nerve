Write-Host -ForegroundColor Green "Downloading newest release of Nerve"

$nerveReleaseZipFile = ".\nerve_windows.zip"
$installDirectory = "$env:LOCALAPPDATA\Programs\Nerve"

Invoke-WebRequest -Uri "https://github.com/nerve-dns/nerve/releases/latest/download/nerve_windows.zip" -OutFile $nerveReleaseZipFile

Write-Host -ForegroundColor Green "Installing Nerve to $installDirectory"

Expand-Archive -Path $nerveReleaseZipFile -DestinationPath $installDirectory -Force

Write-Host -ForegroundColor Green "Nerve successfully installed"

Remove-Item -Path $nerveReleaseZipFile

$confirm = Read-Host "Do you want to install Nerve as a service (recommended)? (y/N)"
if ($confirm -eq 'y')
{
    $params = @{
        Name = "NerveService"
        BinaryPathName = "$installDirectory\Service.exe"
        DisplayName = "Nerve DNS Server"
        StartupType = "Automatic"
        Description = "The Nerve (https://github.com/nerve-dns/nerve) DNS server service."
    }
    New-Service @params
    Start-Service -Name "NerveService"

    Write-Host -ForegroundColor Green "Nerve successfully registered and started as a service"
}
