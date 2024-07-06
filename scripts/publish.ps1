function Publish {
    param (
        [Parameter(Mandatory)]
        [string]
        $Runtime
    )
    
    Write-Host -ForegroundColor Green "Publishing for runtime $Runtime"

    dotnet publish ../src/Service/Service.csproj -c Release --runtime $Runtime -o publish/$Runtime -p:PublishTrimmed=true -p:PublishSingleFile=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true 1> $null
}

if ($args.Length -eq 0) {
    $supportedRuntimes = @("win-x64", "linux-x64", "linux-arm64")

    Write-Host "Publishing for $($supportedRuntimes.Length) runtimes"

    foreach ($supportedRuntime in $supportedRuntimes) {
        Publish -Runtime $supportedRuntime
    }
} else {
    Publish -Runtime $args[0]
}

Write-Host -ForegroundColor Green "Successfully published"
