Unicode true
!include "MUI2.nsh"
!define APPKEY "Software\DemirMetraj\DMTPRO"
!define UNINSTALLKEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\DMTPRO"
Name "DMTPRO 2.0.0 - Ikon Duzeltmesi"
OutFile "dist/DMTPRO_Setup_2.0.0_IconFix_win_x64.exe"
InstallDir "$LOCALAPPDATA\Programs\DMTPRO"
InstallDirRegKey HKCU "${APPKEY}" "InstallPath"
RequestExecutionLevel user
SetCompressor /SOLID lzma
ShowInstDetails show
Icon "dist/DMTPRO_Icon.ico"
UninstallIcon "dist/DMTPRO_Icon.ico"
VIProductVersion "2.0.0.2"
VIAddVersionKey "ProductName" "DMTPRO"
VIAddVersionKey "FileDescription" "DMTPRO Setup - Icon Fix"
VIAddVersionKey "CompanyName" "DemirMetraj"
VIAddVersionKey "FileVersion" "2.0.0.2"
VIAddVersionKey "LegalCopyright" "Copyright 2026 DemirMetraj"
!define MUI_ABORTWARNING
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
 File "/oname=DMTPRO.exe" "dist/DMTPRO_patched.exe"
 File "dist/original/support-config.json"
 File "dist/DMTPRO_Icon.ico"
 WriteRegStr HKCU "${APPKEY}" "InstallPath" "$INSTDIR"
 CreateDirectory "$SMPROGRAMS\DMTPRO"
 CreateDirectory "$DESKTOP"
 CreateShortCut "$SMPROGRAMS\DMTPRO\DMTPRO.lnk" "$INSTDIR\DMTPRO.exe" "" "$INSTDIR\DMTPRO_Icon.ico" 0
 CreateShortCut "$DESKTOP\DMTPRO.lnk" "$INSTDIR\DMTPRO.exe" "" "$INSTDIR\DMTPRO_Icon.ico" 0
 WriteUninstaller "$INSTDIR\Uninstall.exe"
 CreateShortCut "$SMPROGRAMS\DMTPRO\Kaldir.lnk" "$INSTDIR\Uninstall.exe"
 WriteRegStr HKCU "${UNINSTALLKEY}" "DisplayName" "DMTPRO 2.0.0"
 WriteRegStr HKCU "${UNINSTALLKEY}" "DisplayVersion" "2.0.0"
 WriteRegStr HKCU "${UNINSTALLKEY}" "Publisher" "DemirMetraj"
 WriteRegStr HKCU "${UNINSTALLKEY}" "DisplayIcon" "$INSTDIR\DMTPRO.exe"
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
 Delete "$INSTDIR\support-config.json"
 Delete "$INSTDIR\DMTPRO_Icon.ico"
 Delete "$INSTDIR\Uninstall.exe"
 RMDir "$INSTDIR"
 DeleteRegKey HKCU "${UNINSTALLKEY}"
 DeleteRegKey HKCU "${APPKEY}"
SectionEnd
