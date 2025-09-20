# SOTCLaney-U522
Built using Unity 2022.3, AR Foundation 5, Mapbox V3
### Cloning on Windows - You may need to Enable Long Paths Support
1. Turn on long paths in Git

    Run this `git config --global core.longpaths true`

2. Turn on long paths in Windows itself

    Open PowerShell as Administrator and run:
   
     ```powershell
   New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem" ` -Name "LongPathsEnabled" -Value 1 -PropertyType DWORD -Force
     ```

    Or via Group Policy (gpedit.msc → Local Computer Policy → Computer Configuration → Administrative Templates → System → Filesystem → Enable Win32 long paths → Enabled).

4. Reboot afterwards.
