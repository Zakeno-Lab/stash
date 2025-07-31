; Inno Setup Script for Stash Clipboard Manager
; This script creates a professional Windows installer

#define MyAppName "Stash"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Bakhyt"
#define MyAppURL "https://yourwebsite.com"
#define MyAppExeName "synapse.exe"
#define MyAppDescription "Advanced clipboard history manager for Windows"

[Setup]
; Basic app information
AppId={{E3B9C8A5-4B5C-4D3E-9F2A-1A2B3C4D5E6F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Installation directories
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes

; Output settings
OutputDir=..\dist
OutputBaseFilename=Stash-Setup-{#MyAppVersion}
SetupIconFile=assets\synapse.ico

; Compression settings for smaller installer
Compression=lzma2/max
SolidCompression=yes

; Windows version requirements
MinVersion=10.0
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

; Visual settings
WizardStyle=modern
WindowVisible=no
WindowShowCaption=yes
WindowResizable=no
DisableWelcomePage=no

; Privileges
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; Uninstall settings
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
; Optional features the user can choose
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "autostart"; Description: "Start {#MyAppName} automatically when Windows starts"; GroupDescription: "Startup options:"

[Files]
; Include ALL files from the publish directory, maintaining the folder structure
Source: "..\synapse\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; Start Menu shortcut (always created)
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "{#MyAppDescription}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"

; Desktop shortcut (optional)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "{#MyAppDescription}"; Tasks: desktopicon

[Registry]
; Auto-start with Windows (only if user selected the task)
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: autostart

; Application registry entries (for app settings/uninstall info)
Root: HKCU; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"

[Run]
; Start the app after installation (user can uncheck this)
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Kill the app before uninstalling to ensure clean removal
Filename: "{cmd}"; Parameters: "/C taskkill /IM synapse.exe /F"; Flags: runhidden

[Code]
// Pascal script section for advanced functionality

function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
begin
  // Check if app is already running
  if Exec('cmd.exe', '/C tasklist /FI "IMAGENAME eq synapse.exe" 2>nul | find /I "synapse.exe" >nul', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    if ResultCode = 0 then
    begin
      // App is running, ask user
      if MsgBox('Stash is currently running. Setup needs to close it to continue.' + #13#10 + #13#10 + 
                'Do you want to close Stash and continue with the installation?', 
                mbConfirmation, MB_YESNO) = IDYES then
      begin
        // Kill the process
        Exec('cmd.exe', '/C taskkill /IM synapse.exe /F', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
        Sleep(1000); // Give it time to close
        Result := True;
      end
      else
      begin
        Result := False; // User chose not to continue
      end;
    end
    else
    begin
      Result := True; // App not running, proceed
    end;
  end
  else
  begin
    Result := True; // Couldn't check, proceed anyway
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  ResultCode: Integer;
begin
  if CurUninstallStep = usUninstall then
  begin
    // Remove from Windows startup if it exists
    RegDeleteValue(HKEY_CURRENT_USER, 'Software\Microsoft\Windows\CurrentVersion\Run', 'Stash');
    
    // Try to kill the app if running
    Exec('cmd.exe', '/C taskkill /IM synapse.exe /F', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(500);
  end;
end;

// Custom messages for better user experience
procedure InitializeWizard();
begin
  WizardForm.WelcomeLabel2.Caption := 
    'This will install Stash on your computer.' + #13#10 + #13#10 +
    'Stash is an advanced clipboard history manager that:' + #13#10 +
    '  • Captures all clipboard content automatically' + #13#10 +
    '  • Provides instant search across clipboard history' + #13#10 +
    '  • Runs quietly in the system tray' + #13#10 +
    '  • Activates with Ctrl+Shift+C hotkey' + #13#10 + #13#10 +
    'Click Next to continue, or Cancel to exit Setup.';
end;