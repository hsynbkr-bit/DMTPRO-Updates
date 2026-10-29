Unicode true
!include "MUI2.nsh"
!define APPKEY "Software\DemirMetraj\DMTPRO"
!define UNINSTALLKEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\DMTPRO"
Name "DMTPRO 2.0.1 - Otomatik Guncelleme"
OutFile "${__FILEDIR__}\..\dist\DMTPRO_Setup_2.0.1_win_x64.exe"
InstallDir "$LOCALAPPDATA\Programs\DMTPRO"
RequestExecutionLevel user
SetCompressor /SOLID lzma
ShowInstDetails show
Icon "${__FILEDIR__}\..\dist\original/DMTPRO_Icon.ico"
UninstallIcon "${__FILEDIR__}\..\dist\original/DMTPRO_Icon.ico"
VIProductVersion "2.0.1.0"
VIAddVersionKey "ProductName" "DMTPRO"
VIAddVersionKey "FileDescription" "DMTPRO 2.0.1 Setup - Online Updater"
VIAddVersionKey "CompanyName" "DemirMetraj"
VIAddVersionKey "FileVersion" "2.0.1.0"
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "Turkish"

Section "DMTPRO" SecMain
  SetShellVarContext current
  SetOutPath "$INSTDIR"
  File "/oname=DMTPRO.exe" "${__FILEDIR__}\..\dist\original/DMTPRO.exe"
  File "/oname=DMTPROLauncher.exe" "${__FILEDIR__}\..\dist\updater/DMTPROLauncher.exe"
  File "${__FILEDIR__}\..\dist\original/support-config.json"
  File "${__FILEDIR__}\..\dist\original/DMTPRO_Icon.ico"
  File "${__FILEDIR__}\..\dist\version.txt"
  WriteRegStr HKCU "${APPKEY}" "InstallPath" "$INSTDIR"
  CreateDirectory "$SMPROGRAMS\DMTPRO"
  CreateShortCut "$SMPROGRAMS\DMTPRO\DMTPRO.lnk" "$INSTDIR\DMTPROLauncher.exe" "" "$INSTDIR\DMTPRO_Icon.ico" 0
  CreateShortCut "$DESKTOP\DMTPRO.lnk" "$INSTDIR\DMTPROLauncher.exe" "" "$INSTDIR\DMTPRO_Icon.ico" 0
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  CreateShortCut "$SMPROGRAMS\DMTPRO\Kaldir.lnk" "$INSTDIR\Uninstall.exe"
  WriteRegStr HKCU "${UNINSTALLKEY}" "DisplayName" "DMTPRO 2.0.1 (Otomatik Guncelleme)"
  WriteRegStr HKCU "${UNINSTALLKEY}" "DisplayVersion" "2.0.1"
  WriteRegStr HKCU "${UNINSTALLKEY}" "Publisher" "DemirMetraj"
  WriteRegStr HKCU "${UNINSTALLKEY}" "DisplayIcon" "$INSTDIR\DMTPRO_Icon.ico"
  WriteRegStr HKCU "${UNINSTALLKEY}" "UninstallString" '"$INSTDIR\Uninstall.exe"'
  WriteRegStr HKCU "${UNINSTALLKEY}" "InstallLocation" "$INSTDIR"
  WriteRegDWORD HKCU "${UNINSTALLKEY}" "NoModify" 1
  WriteRegDWORD HKCU "${UNINSTALLKEY}" "NoRepair" 1
SectionEnd

Section "Uninstall"
  SetShellVarContext current
  Delete "$DESKTOP\DMTPRO.lnk"
  Delete "$SMPROGRAMS\DMTPRO\DMTPRO.lnk"
  Delete "$SMPROGRAMS\DMTPRO\Kaldir.lnk"
  RMDir "$SMPROGRAMS\DMTPRO"
  Delete "$INSTDIR\DMTPRO.exe"
  Delete "$INSTDIR\DMTPROLauncher.exe"
  Delete "$INSTDIR\DMTPRO_Icon.ico"
  Delete "$INSTDIR\support-config.json"
  Delete "$INSTDIR\version.txt"
  Delete "$INSTDIR\Uninstall.exe"
  RMDir "$INSTDIR"
  DeleteRegKey HKCU "${UNINSTALLKEY}"
  DeleteRegKey HKCU "${APPKEY}"
SectionEnd
