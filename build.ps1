param([switch]$FrameworkDependent)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $projectRoot
try {
    dotnet test .\AssistDriftCarFH5.sln -c Release
    if ($FrameworkDependent) {
        dotnet publish .\JSE\JSE.csproj -c Release -r win-x64 --self-contained false -o .\artifacts\JSE
    } else {
        dotnet publish .\JSE\JSE.csproj -c Release -r win-x64 --self-contained true -o .\artifacts\JSE
    }
    Write-Host "Pacote criado em $projectRoot\artifacts\JSE" -ForegroundColor Green
} finally {
    Pop-Location
}
