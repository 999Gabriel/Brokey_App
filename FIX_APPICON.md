# App Icon Build Fix

## Problem

The iOS/macCatalyst build fails with:
```
actool: None of the input catalogs contained a matching stickers icon set, app icon set, or icon stack named "brokey_travel_icon".
```

### Root Cause

There are three things that need to be in sync:

1. **The `.csproj`** — currently correct:
   ```xml
   <MauiIcon Include="Resources\AppIcon\brokey_icon.png"/>
   ```

2. **The icon file** — `Resources/AppIcon/brokey_icon.png` — exists and is correct.

3. **The cached `AppManifest.plist`** — THIS is the problem. The file at:
   ```
   obj/Debug/net10.0-ios/iossimulator-arm64/AppManifest.plist
   ```
   is a stale binary plist from a previous build that still contains:
   ```
   Assets.xcassets/brokey_travel_icon.appiconset
   ```
   (`brokey_travel_icon` was the old app icon name.)

   Because the MSBuild target `_CompileAppManifest` is skipped as "up-to-date", this stale plist is passed to `actool`, which then looks for `brokey_travel_icon.appiconset` — but the xcassets only contains `brokey_icon.appiconset` (the new icon). Hence the error.

## Fix Required

Run a clean build to delete all stale MSBuild artifacts so `AppManifest.plist` is regenerated correctly:

```bash
cd /Users/gabriel/RiderProjects/Brokey_APP
dotnet clean Brokey_APP.sln
dotnet build Brokey_APP/Brokey_APP.csproj -f net10.0-ios
```

Or in Rider: **Build → Clean Solution**, then rebuild.

After cleaning, MSBuild will regenerate `AppManifest.plist` with `brokey_icon.appiconset` as the icon reference, and `actool` will resolve it correctly.

## Files involved

| File | Status |
|---|---|
| `Brokey_APP/Brokey_APP.csproj` | ✅ Correct — references `brokey_icon.png` |
| `Brokey_APP/Resources/AppIcon/brokey_icon.png` | ✅ Exists — 1024×1024 PNG |
| `Brokey_APP/obj/Debug/net10.0-ios/**/AppManifest.plist` | ❌ Stale — still references `brokey_travel_icon` |
| `Brokey_APP/obj/Debug/net10.0-maccatalyst/**/AppManifest.plist` | ❌ Stale — still references `brokey_travel_icon` |

Delete the entire `obj/` directory (or run `dotnet clean`) and the next build will succeed.
