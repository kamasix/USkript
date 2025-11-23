# USkript Release Package Creator
# Creates a ZIP file ready for GitHub Release

$version = "0.1.0"
$releaseDir = "USkript-Release-$version"
$zipFile = "USkript-$version.zip"

Write-Host "Creating USkript release package..." -ForegroundColor Cyan

# Create release directory
if (Test-Path $releaseDir) {
    Remove-Item -Recurse -Force $releaseDir
}
New-Item -ItemType Directory -Path $releaseDir | Out-Null

# Copy main plugin DLL
Copy-Item "bin\Debug\netstandard2.1\USkript.dll" -Destination $releaseDir

# Copy example scripts
New-Item -ItemType Directory -Path "$releaseDir\Scripts" | Out-Null
Copy-Item "Examples\*.usk" -Destination "$releaseDir\Scripts\"

# Copy documentation
Copy-Item "README.md" -Destination $releaseDir
Copy-Item "GETTING_STARTED.md" -Destination $releaseDir
Copy-Item "SYNTAX_REFERENCE.md" -Destination $releaseDir
Copy-Item "API_REFERENCE.md" -Destination $releaseDir
Copy-Item "EXAMPLES.md" -Destination $releaseDir

# Copy config files
Copy-Item "config.yaml" -Destination $releaseDir
Copy-Item "translations.yaml" -Destination $releaseDir

# Create installation instructions
@"
# USkript $version - Installation

## Requirements
- Unturned server with OpenMod installed
- .NET Runtime compatible with netstandard2.1

## Installation Steps

1. Copy USkript.dll to your server's OpenMod plugins folder:
   OpenMod/plugins/USkript.dll

2. Create USkript directory for scripts:
   OpenMod/USkript/Scripts/

3. Copy example scripts from Scripts/ folder to:
   OpenMod/USkript/Scripts/

4. Copy config.yaml and translations.yaml to:
   OpenMod/USkript/

5. Restart your server

6. Test with command: /uskript reload

## Directory Structure
```
YourServer/
├─ OpenMod/
│  ├─ plugins/
│  │  └─ USkript.dll
│  └─ USkript/
│     ├─ config.yaml
│     ├─ translations.yaml
│     └─ Scripts/
│        ├─ welcome.usk
│        ├─ starter-kit.usk
│        ├─ chat-filter.usk
│        └─ auto-save.usk
```

## Getting Started
See GETTING_STARTED.md for creating your first script.

## Documentation
- README.md - Overview and features
- SYNTAX_REFERENCE.md - Complete syntax guide
- API_REFERENCE.md - Available actions and conditions
- EXAMPLES.md - Example scripts

## Support
- GitHub: https://github.com/kamasix/USkript
- Discord: https://discord.gg/7crjRskdyj
"@ | Out-File -FilePath "$releaseDir\INSTALL.txt" -Encoding UTF8

# Create ZIP
if (Test-Path $zipFile) {
    Remove-Item $zipFile
}
Compress-Archive -Path $releaseDir -DestinationPath $zipFile

Write-Host "✓ Created: $zipFile" -ForegroundColor Green
Write-Host ""
Write-Host "Contents:" -ForegroundColor Yellow
Get-ChildItem -Recurse $releaseDir | Select-Object FullName | ForEach-Object {
    $_.FullName.Replace("$PWD\$releaseDir\", "  - ")
}

Write-Host ""
Write-Host "Ready to upload to GitHub Release!" -ForegroundColor Cyan

# Cleanup
Remove-Item -Recurse -Force $releaseDir
