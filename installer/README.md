# Stash Installer Build Instructions

## Prerequisites
1. Install Inno Setup 6 from https://jrsoftware.org/isdl.php
2. Build Stash in Release mode

## Building the Installer

### Method 1: Using Inno Setup GUI
1. Open Inno Setup Compiler
2. File → Open → Select `synapse-setup.iss`
3. Build → Compile (or press Ctrl+F9)
4. The installer will be created in the `dist` folder

### Method 2: Command Line
```bash
cd installer
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" synapse-setup.iss
```

## What the Installer Does
1. **Installs Stash** to Program Files
2. **Creates Start Menu shortcuts**
3. **Optionally creates Desktop shortcut**
4. **Optionally adds to Windows startup** (recommended)
5. **Handles running instances** gracefully
6. **Provides clean uninstall**

## Testing the Installer
1. Run the installer: `dist\Stash-Setup-1.0.0.exe`
2. Verify installation in Program Files
3. Check system tray for Stash icon
4. Test Ctrl+Shift+C hotkey
5. Verify auto-start (restart Windows)
6. Test uninstaller

## Customization
Edit `synapse-setup.iss` to change:
- Version number: `MyAppVersion`
- Publisher info: `MyAppPublisher` and `MyAppURL`
- Default installation options
- Icon and graphics

## Code Signing (Optional)
To sign your installer and avoid Windows security warnings:
1. Obtain a code signing certificate
2. Add to the [Setup] section:
   ```
   SignTool=signtool /f "path\to\certificate.pfx" /p password /t http://timestamp.server $f
   ```