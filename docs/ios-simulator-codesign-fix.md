# iOS Simulator Code Signature Fix

## Problem

The app crashed immediately on launch in every iOS simulator with:

```
Exception Type: EXC_BAD_ACCESS (SIGKILL – Code Signature Invalid)
Termination Reason: Namespace CODESIGNING, Code 2, Invalid Page
```

The crash happened inside `dyld4::prepareSim` before any app code ran, meaning the OS was killing the process during library loading.

Two things caused this.

**1. Hardcoded `RuntimeIdentifier` in the `.csproj`**

```xml
<!-- old — forced device RID for every iOS build -->
<IPhoneSdkVersion ...>iphoneos</IPhoneSdkVersion>
<RuntimeIdentifier ...>ios-arm64</RuntimeIdentifier>
```

These properties told MSBuild to always compile for a physical device (`ios-arm64`), even when deploying to the simulator. The resulting binary was signed with a device identity and rejected by the simulator's dyld, which expects `iossimulator-arm64` with an ad-hoc signature.

**2. Stale ad-hoc signatures on bundled .NET runtime dylibs**

Even after fixing the RID, the pre-compiled .NET runtime libraries that ship with the iOS SDK workload (`libmonosgen-2.0.dylib`, `libSystem.Native.dylib`, etc.) carry signatures that macOS 26's stricter dyld page-validation rejects. The dylibs pass `codesign -v` individually but still fail at mmap time in the simulator.

## Fix

**Remove the hardcoded overrides** so the build system picks the correct RID automatically:

```xml
<!-- Brokey_APP.csproj — removed IPhoneSdkVersion and RuntimeIdentifier lines -->
<SupportedOSPlatformVersion Condition="... == 'ios'">15.0</SupportedOSPlatformVersion>
```

**Add a post-build re-signing target** that runs after every simulator build:

```xml
<Target Name="ResignSimulatorBundle"
        AfterTargets="Codesign"
        Condition="$(RuntimeIdentifier.StartsWith('iossimulator'))">
    <Exec Command="find &quot;$(AppBundleDir)&quot; -name &quot;*.dylib&quot; -exec codesign --force --sign - --timestamp=none {} \;" />
    <Exec Command="codesign --force --sign - --timestamp=none &quot;$(AppBundleDir)/$(AssemblyName)&quot;" />
    <Exec Command="codesign --force --sign - --timestamp=none &quot;$(AppBundleDir)&quot;" />
</Target>
```

This re-signs all dylibs, the main executable, and the bundle with fresh ad-hoc signatures after the SDK's own signing step, which resolves the stale-signature page fault.

## Notes

- This only runs for simulator builds (`iossimulator-*`). Device builds are unaffected.
- If this keeps recurring after a .NET SDK workload update, re-clean the iOS bin/obj and rebuild — the new runtime dylibs will get fresh signatures on the next build.
- The repo still lacks a `.gitignore`. Bin and obj folders are currently tracked, which is not ideal.
