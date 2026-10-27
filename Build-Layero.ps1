$ErrorActionPreference = 'Stop'
$pcClubStage = Join-Path $PSScriptRoot '.setup/layero-deploy'
$pcClubApp = Join-Path $pcClubStage 'app'
& dotnet publish (Join-Path $PSScriptRoot 'PcClub/PcClub.csproj') -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o $pcClubApp
if ($LASTEXITCODE -ne 0) {
    throw 'Не удалось собрать PC Club.'
}
Copy-Item (Join-Path $PSScriptRoot 'deploy/layero/*') -Destination $pcClubStage -Force
$pcClubSettingsPath = Join-Path $pcClubApp 'appsettings.json'
$pcClubSettings = Get-Content -LiteralPath $pcClubSettingsPath -Raw | ConvertFrom-Json
$pcClubSettings.PSObject.Properties.Remove('ConnectionStrings')
$pcClubSettings.AllowedHosts = '*'
$pcClubSettings | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $pcClubSettingsPath -Encoding utf8
Get-ChildItem -LiteralPath $pcClubApp -Filter 'appsettings.*.json' | ForEach-Object {
    Remove-Item -LiteralPath $_.FullName
}
Write-Host "Готово. Папка публикации: $pcClubStage"
