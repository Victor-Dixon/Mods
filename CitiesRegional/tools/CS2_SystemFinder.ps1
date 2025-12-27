# CS2 System Finder Tool
# A tool I wished I had during development - quickly find game systems by keyword

param(
    [string]$Keyword = "",
    [string]$LogPath = "D:\mods\CS2\Cities.Skylines.II.v1.5.3f1\game\BepInEx\LogOutput.log"
)

Write-Host "🔍 CS2 System Finder" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $LogPath)) {
    Write-Host "❌ Log file not found: $LogPath" -ForegroundColor Red
    Write-Host "   Make sure the game has run at least once." -ForegroundColor Yellow
    exit 1
}

Write-Host "📄 Reading log file: $LogPath" -ForegroundColor Green
Write-Host ""

# Extract system discovery entries
$systems = Get-Content $LogPath | 
    Select-String "\[Discovery\]" | 
    Where-Object { $_.Line -match "Game\." -or $_.Line -match "Colossal\." } |
    ForEach-Object {
        if ($_.Line -match "\[Discovery\]\s+(.+)") {
            $_.Matches.Groups[1].Value.Trim()
        }
    } |
    Where-Object { $_ -ne "" } |
    Sort-Object -Unique

if ($systems.Count -eq 0) {
    Write-Host "⚠️  No systems found in log." -ForegroundColor Yellow
    Write-Host "   Make sure you've loaded into a city (not just main menu)." -ForegroundColor Yellow
    exit 0
}

Write-Host "📊 Found $($systems.Count) unique systems" -ForegroundColor Green
Write-Host ""

# Filter by keyword if provided
if ($Keyword) {
    Write-Host "🔎 Filtering by keyword: '$Keyword'" -ForegroundColor Cyan
    Write-Host ""
    $filtered = $systems | Where-Object { $_ -match $Keyword }
    
    if ($filtered.Count -eq 0) {
        Write-Host "❌ No systems found matching '$Keyword'" -ForegroundColor Red
        exit 0
    }
    
    Write-Host "✅ Found $($filtered.Count) matching systems:" -ForegroundColor Green
    Write-Host ""
    $filtered | ForEach-Object {
        Write-Host "  • $_" -ForegroundColor White
    }
} else {
    # Show all systems, grouped by namespace
    Write-Host "📋 All Systems (grouped by namespace):" -ForegroundColor Green
    Write-Host ""
    
    $grouped = $systems | Group-Object {
        if ($_ -match "^([^.]+\.)") {
            $matches[1]
        } else {
            "Other"
        }
    }
    
    foreach ($group in $grouped | Sort-Object Name) {
        Write-Host "  📦 $($group.Name)" -ForegroundColor Yellow
        foreach ($system in $group.Group | Sort-Object) {
            Write-Host "     • $system" -ForegroundColor White
        }
        Write-Host ""
    }
    
    Write-Host ""
    Write-Host "💡 Tip: Use -Keyword 'Population' to filter systems" -ForegroundColor Cyan
    Write-Host "   Example: .\CS2_SystemFinder.ps1 -Keyword 'Economy'" -ForegroundColor Gray
}

Write-Host ""
Write-Host "✨ Done!" -ForegroundColor Green


