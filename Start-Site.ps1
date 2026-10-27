$ErrorActionPreference = 'Stop'
$projectPath = Join-Path $PSScriptRoot 'PcClub/PcClub.csproj'
Write-Host 'PC CLUB: http://localhost:5089/'
Write-Host 'Для остановки сайта нажмите Ctrl+C в этом окне.'
& dotnet run --project $projectPath --launch-profile http
