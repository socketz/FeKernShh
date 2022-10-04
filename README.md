# FeKernShh - Neuter FeKern by unloading its driver
```
Usage: FeKernShh.exe <hunt|kill>
```

FeKern's driver it is always loaded at altitude 388360. The objective of this tool is to challenge the assumption that FireEye Activity Monitor are always collecting events. FeKernShh locates and unloads the driver using this strategy:

**1.** Uses `fltlib!FilterFindFirst` and `fltlib!FilterFindNext` to enumerate drivers on the system in place of crawling the registry.  
**2a.** If a driver is found at altitude 388360, it uses `kernel32!OpenProcessToken` and `advapi32!AdjustTokenPrivileges` to grant itself `SeLoadDriverPrivilege`.
**2b.** If a driver was not found at 388360, it walks `HKLM\SYSTEM\CurrentControlSet\Services` looking for a "FeKern Instance" subkey and if found, assigns the required permission as desrcibed above.
**3.** If it was able get the required privilege, it calls `fltlib!FilterUnload` to unload the driver.  

## Defensive Guidance
There are a few interesting events surrounding this tactic that should be evaluated:
- **Sysmon Event ID 255** - Error message with a detail of `DriverCommunication`
- **Windows System Event ID 1** - From the source "FilterManager" stating `File System Filter '\<DriverName\>' (Version 0.0, \<Timstamp\>) unloaded successfully.`
- **Windows Security Event ID 4672** - `SeLoadDriverPrivileges` being granted to an account other than `SYSTEM`
- **Sysmon Event ID 1/Windows Security Event 4688** - Abnormal high-integrity process correlating with the driver unload. This event would be the last before the driver error in Sysmon

**Mitre ATT&CK References**: [T1054](https://attack.mitre.org/techniques/T1054/), [T1089](https://attack.mitre.org/techniques/T1089/)

## Credits :raised_hands:
This is a PoC of an adaptation of excelent work of [Shhmon](https://github.com/matterpreter/Shhmon) repository by Matt Hand ([@matterpreter](https://twitter.com/matterpreter)).

## Additional info
[Allocated altitudes](https://docs.microsoft.com/en-us/windows-hardware/drivers/ifs/allocated-altitudes)

[Filter Manager Concepts](https://docs.microsoft.com/en-us/windows-hardware/drivers/ifs/filter-manager-concepts)
