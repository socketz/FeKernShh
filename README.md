# FeKernShh - Neuter FeKern by unloading its driver
```
Usage: FeKernShh.exe <hunt|kill>
```

FeKern's driver it is always loaded at altitude 385600. The objective of this tool is to challenge the assumption that FireEye Activity Monitor are always collecting events. FeKernShh locates and unloads the driver using this strategy:

**1.** Uses `fltlib!FilterFindFirst` and `fltlib!FilterFindNext` to enumerate drivers on the system in place of crawling the registry.  
**2a.** If a driver is found at altitude 385600, it uses `kernel32!OpenProcessToken` and `advapi32!AdjustTokenPrivileges` to grant itself `SeLoadDriverPrivilege`.  
**2b.** If a driver was not found at 385600, it walks `HKLM\SYSTEM\CurrentControlSet\Services` looking for a "FeKern Instance" subkey and if found, assigns the required permission as desrcibed above.  
**3.** If it was able get the required privilege, it calls `fltlib!FilterUnload` to unload the driver.  

## Credits
This is a PoC of an adaptation of excelent work of [Shhmon](https://github.com/matterpreter/Shhmon) repository by Matt Hand ([@matterpreter](https://twitter.com/matterpreter)).

## Additional info
[Allocated altitudes](https://docs.microsoft.com/en-us/windows-hardware/drivers/ifs/allocated-altitudes)

[Filter Manager Concepts](https://docs.microsoft.com/en-us/windows-hardware/drivers/ifs/filter-manager-concepts)