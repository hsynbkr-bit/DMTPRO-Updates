# DMTPRO Windows software trust and code-signing release gate

## Confirmed baseline (29 October 2026)

Read-only Windows GitHub Actions audit of pinned release `dmtpro-v2.0.3` confirms:
- `DMTPRO_Setup_2.0.3_win_x64.exe` (SHA-256 `98008f6617edeb9b15305e2d04d1cde424e46276a2d71388bb16984c3c60e655`): Authenticode **NotSigned**.
- Embedded `DMTPROLauncher.exe`: **NotSigned**.
- Embedded `DMTPRO.exe`: **NotSigned**.
- `Publisher=DemirMetraj` in NSIS uninstall metadata is **not** an Authenticode signature and does not establish publisher identity.
- Avast CyberCapture can examine unfamiliar files. Its popup is not proof that a file is malware; do not ask users to disable protection, bypass quarantine, or trust a file without investigation.

The unsigned release remains unchanged; **do not call it a signed, cleared or Avast-approved release**.

## Trust establishment: next signed release

1. Obtain an Organization Validated (OV) Windows code-signing identity from a public trusted CA, using the real registered company name. Organization and signing certificate eligibility must be verified by the certificate provider. OV keys generally require a hardware-backed key / HSM. Never commit keys, certificates with private keys, PFX files, or passwords to GitHub. Do not spoof a publisher name.
2. If considering Microsoft Azure Artifact Signing, verify country eligibility before committing: as of 2026, Microsoft says organizations in the US/Canada/EU/UK can use it; companies based only in Türkiye are not in the listed eligible regions. A global CA-issued OV certificate is the practical alternative.
3. Scan the source and each built PE, investigate any detections. If Avast confirms an incorrect detection, use its official false-positive submission process with the real detection name/ID. CyberCapture analysis should finish before considering a false-positive report.
4. Publish a stable *signed* version using a fresh version/tag. The correct order matters:
   - Build `DMTPROLauncher.exe` and `DMTPRO.exe`. Do not change the licensed metraj engine without separate DWG/AutoCAD regression testing.
   - **Sign and RFC3161 timestamp each executable first** with the same verified publisher.
   - Validate Authenticode signatures and timestamp trust with Windows, then compute per-file SHA-256.
   - Create the OTA ZIP and `version.json` using hashes **of the signed bytes**. Update the version and release tag in the manifest.
   - Build the NSIS single Setup embedding those exact signed files; **sign and timestamp the Setup last**. Verify its Authenticode publisher and timestamp. Hash and publish immutable signed final artifacts.
   - Check downloads again: published SHA-256 equals CI artifact; signed inner executables still report `Valid`; run install/update, license, and AutoCAD tests.
5. Run `scripts/assert-authenticode.ps1` as an unconditional release gate once signing is provisioned. Never simply label an unsigned result as approved.
6. Be consistent: repeat the same validated publisher identity across updates. Microsoft SmartScreen and third-party AV reputation can take time to build; no vendor guarantees zero warnings.
7. For updates: do not use source SHA-256 of the unsigned 2.0 engine as the expected hash after signing changes its bytes. Adapt the engine-preservation test to compare the exact approved *signed* engine artifact.
8. For the website: show the publisher's legally verified certificate subject, release number, complete final Setup SHA-256, and link to release notes. Do not promise that the program is 'virus-free' based solely on a hash or signature.

## Vendor / documentation references
- Avast CyberCapture: https://support.avast.com/en-en/article/antivirus-cybercapture/
- Avast file/URL false-positive reporting: https://support.avast.com/en-en/article/258/
- Microsoft SmartScreen publisher reputation: https://learn.microsoft.com/windows/apps/package-and-deploy/smartscreen-reputation
- Windows signing options (including geographic eligibility): https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/code-signing-options

## Release blocker

**Unresolved** until a CA-issued signing identity is provisioned and the real Windows signed artifacts pass the verification gate. Do not release an unsigned binary as a 'signed security fix'.
