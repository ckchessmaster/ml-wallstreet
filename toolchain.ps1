Stop-WebAppPool -Name "ml-wallstreet-ui"
$folder = Resolve-Path "ML-Wallstreet-UI"
Push-Location $folder
dotnet publish -c Release --self-contained -r win-x64
Move-Item -Path .\ML-Wallstreet-UI\bin\Release\netcoreapp2.2\win-x64\publish -Destination ..\..\hosting\ML-Wallstreet-UI
Pop-Location
Start-WebAppPool -Name "ml-wallstreet-ui"

