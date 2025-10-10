# Start Azurite Storage Emulator
# This script starts Azurite in a new window so you can see its logs

Write-Host "Starting Azurite Storage Emulator..." -ForegroundColor Green

# Get the Azurite command path
$azuritePath = (Get-Command azurite -ErrorAction SilentlyContinue).Source

if (-not $azuritePath) {
    Write-Host "Azurite not found. Make sure it's installed via npm:" -ForegroundColor Red
    Write-Host "npm install -g azurite" -ForegroundColor Yellow
    exit 1
}

Write-Host "Found Azurite at: $azuritePath" -ForegroundColor Cyan

# Start Azurite in a new window
Start-Process cmd -ArgumentList '/k', 'azurite' -WindowStyle Normal

Write-Host "Azurite started in a new window." -ForegroundColor Green
Write-Host "Blob Service:  http://127.0.0.1:10000" -ForegroundColor Cyan
Write-Host "Queue Service: http://127.0.0.1:10001" -ForegroundColor Cyan
Write-Host "Table Service: http://127.0.0.1:10002" -ForegroundColor Cyan
Write-Host ""
Write-Host "To stop Azurite, close the command window or press Ctrl+C in that window." -ForegroundColor Yellow
