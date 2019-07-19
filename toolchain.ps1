$basePath = "C:\Users\ckche\Desktop\ML Stock Project\ml-wallstreet\"

Stop-WebAppPool -Name "ml-wallstreet-ui"
$folder = $basePath + "ML-Wallstreet-UI"
Push-Location $folder
dotnet publish -c Release --self-contained -r win-x64
Remove-Item -Path ..\..\hosting\ML-Wallstreet-UI
Move-Item -Force -Path .\ML-Wallstreet-UI\bin\Release\netcoreapp2.2\win-x64\publish -Destination ..\..\hosting\ML-Wallstreet-UI
Pop-Location
Start-WebAppPool -Name "ml-wallstreet-ui"