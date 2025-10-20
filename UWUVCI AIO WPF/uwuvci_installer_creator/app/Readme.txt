============================================================
                   UWUVCI v3 ReadMe (By ZestyTS)
============================================================

Thank you for downloading UWUVCI-3!
If you didn’t download it from the official GitHub source, you might be using a modified version.

Official source:
  https://github.com/stuff-by-3-random-dudes/UWUVCI-AIO-WPF/releases

If you're looking for the FAQ, keep scrolling.
If you're curious about the Discord, the latest updates, or video guides, you’ll find everything below.

============================================================
 Community & Resources
============================================================
• Discord:
  https://discord.gg/mPZpqJJVmZ

• Latest changes:
  https://github.com/stuff-by-3-random-dudes/UWUVCI-AIO-WPF/releases/latest

• Official UWUVCI 3 Video Series:
  https://www.youtube.com/watch?v=I5UdYcVSRSA&list=PLbQMtrmXFIxQ1hpvu9m1th41vsaqnZ2Id

============================================================
 Project Overview
============================================================
By the time you're reading this, **active development on UWUVCI-3 has concluded.**
Future development continues under UWUVCI-Prime (v4), which adds full Mac and Linux support.

I’m **ZestyTS**, and since late 2020 I’ve been the primary developer maintaining and improving UWUVCI-3.
My goal was to fix legacy bugs, modernize the code, and make the program stable, fast, and cross-platform friendly.

============================================================
 Major Features Introduced in UWUVCI v3
============================================================
These are the major features and improvements I personally implemented:

• Widescreen support for N64
• DarkFilter (VFilter) removal for N64 and GBA
• C2W overclock patching for Wii injects
• GCT patching and Deflicker options for Wii
• Full support for Windows 7 and 8
• Helper App for Unix (Wine/macOS/Linux)
• Automatic dependency detection and installation
• Rewritten Installer with guided setup and OneDrive-safe paths
• “First-Run Tutorial” wizard
• Rewritten ReadMe Viewer
• Logging system (auto-clears after 7 days)
• Async refactor — faster inject creation and UI responsiveness
• CNUSPACKER and WiiUDownloader rewritten as DLLs
• Added tooltips, better error handling, and smoother UI behavior
• Updated to .NET Framework 4.8.1 and C# 8.0

In short — UWUVCI v3 became more self-contained, faster, and significantly more stable.

============================================================
 Support the Developers
============================================================
❤️ Donate to me (ZestyTS):  
  https://ko-fi.com/zestyts

💚 Donate to the original creator (NicoAICP):  
  https://ko-fi.com/uwuvci

============================================================
 Frequently Asked Questions (FAQ)
============================================================
Maintained by ZestyTS (2020–2025)  
This FAQ was rewritten during v3.Z-B after major system updates.
Please read carefully before assuming something is broken.

============================================================
 🔰 Getting Started
============================================================

Q1) I don’t know how to use UWUVCI.  
A) Go here:  
   https://uwuvci-prime.github.io/UWUVCI-Resources/index  
   Select your console and follow the steps exactly.  
   Don’t skip steps or use random YouTube guides.

------------------------------------------------------------

Q2) What games are compatible?  
A) Visit: https://uwuvci.net → click “Compatibility” (top right).  
   If a game isn’t listed, it’s **untested**, not unsupported.  
   For GameCube: Rhythm Heaven Fever (US) works for nearly all titles.

------------------------------------------------------------

Q3) What does “Base” mean in the dropdown?  
A) The base game is the template UWUVCI uses to inject your selected title.

------------------------------------------------------------

Q4) “Base not downloaded”?  
A) The base game is missing.  
   Fix: Click “Enter TKey” and input your Title Key for the purchased base.

------------------------------------------------------------

Q5) How do I get the Title Key?  
A) Buy the base from the eShop → dump using Tik2SD.  
   ⚠️ Title Key sharing = piracy. Don’t do that.

------------------------------------------------------------

Q6) What’s the Common Key?  
A) The Wii U system decryption key.  
   • Have a NAND backup? Use `otp.bin`.  
   • Otherwise, follow: https://wiiu.hacks.guide/aroma/nand-backup.html

------------------------------------------------------------

Q7) Base download stuck or slow?  
A) Nintendo’s servers can lag. Try again later.  
   • Normal injects: under 5 minutes  
   • Large games (e.g. Xenoblade): longer (~8 GB)

============================================================
 ⚙️ Setup & General Issues
============================================================

Q8) Antivirus flagged UWUVCI?  
A) False positive — whitelist it. Nothing malicious is inside.

------------------------------------------------------------

Q9) “Could not find file 'bin\\temp\\pre.iso'”?  
A) Bad or trimmed game dump. Use a clean ISO, **not** .nkit or .wbfs.

------------------------------------------------------------

Q10) “Path .../temp/temp missing” or “tmd.bin can’t be found”?  
A) Same issue — invalid dump. Redump properly.

------------------------------------------------------------

Q11) UWUVCI doesn’t open.  
A) Install .NET Framework 4.8.1:  
   https://dotnet.microsoft.com/en-us/download/dotnet-framework/net481  
   Still not opening? See Q30.

------------------------------------------------------------

Q12) UWUVCI says “Drive is full (12 GB)”.  
A) Move UWUVCI to a drive with more free space.

------------------------------------------------------------

Q13) UWUVCI crashes, UI disappears, or acts strange.  
A) Check that:  
   • You didn’t install in OneDrive / cloud folder  
   • Antivirus isn’t blocking background tools  
   • You extracted the ZIP before running

============================================================
 💾 Injection & Compatibility
============================================================

Q14) Inject created but game doesn’t launch properly.  
A) Check all of these:  
   • Correct base game (region-matched)  
   • Base ≥ target game size  
   • Use unmodified ROMs  
   • For N64, different bases behave differently — test another one

------------------------------------------------------------

Q15) WUP install fails / Error 199-9999.  
A) Missing sigpatches.  
   Download:  
   https://github.com/V10lator/SigpatchesModuleWiiU/releases/download/v1.0/01_sigpatches.rpx  
   Place in:  
   sd:/wiiu/environments/aroma/modules/setup

------------------------------------------------------------

Q16) GCN/Wii injects not working.  
A) Usually SDUSB or ISFShax environment issues.  
   Also verify your Nintendont setup (see Q19).

------------------------------------------------------------

Q17) GCN inject boots to the Nintendont menu.  
A) You used TeconMoon injector before.  
   Fix:  
   • Delete `nincfg.bin` from SD root  
   • Delete `apps/nintendont` folder  
   • Re-run “SD Setup” in UWUVCI

------------------------------------------------------------

Q18) “boot.dol not found”.  
A) Nintendont not set up on SD. Run “SD Setup” again.

------------------------------------------------------------

Q19) SaveMii can’t find my injects.  
A) Use **SaveMii Inject MOD**, not the vanilla version.

------------------------------------------------------------

Q20) GB/C games don’t save when using the VC reset button.  
A) Normal behavior — GoombaColor doesn’t handle VC resets.  
   Use the in-game reset button combo instead.

------------------------------------------------------------

Q21) “NKit error?”  
A) You used a pirated or modified dump. Use a real ISO.  
   UWUVCI does **not** support illegal or altered files.

------------------------------------------------------------

Q22) “Stuck on ‘Copying to SD’”.  
A) Manually copy it yourself:  
   Go to UWUVCI’s `InjectedGames` folder → move the inject to your SD card.

------------------------------------------------------------

Q23) Help with ROM hacks or mods?  
A) Mods are unsupported.  
   If it runs on real hardware, it might work here — but ask the mod’s community.  
   UWUVCI can’t guarantee mod compatibility.

============================================================
 🧰 Advanced Troubleshooting
============================================================

Q24) “UWUVCI still won’t open” after installing .NET.  
A) Check Windows Event Viewer for crash details.  
   If it references missing DLLs, rerun the installer.

------------------------------------------------------------

Q25) “Could not load CNUSPACKER.dll” or similar.  
A) Required DLLs are missing — rerun the installer to restore them.

------------------------------------------------------------

Q26) UWUVCI’s progress bar gets stuck.  
A) Update Tools: click ⚙️ → “Update Tools”.

------------------------------------------------------------

Q27) Mac/Linux version?  
A) UWUVCI-3 uses WPF, a Windows-only framework.  
   Use Wine or CrossOver — UWUVCI auto-detects non-Windows systems.  
   UWUVCI-Prime (v4) will be natively cross-platform.

------------------------------------------------------------

Q28) Where are the Log and Settings files?  
A) Windows:  
      %localappdata%\UWUVCI-V3  
   Mac/Linux (Wine):  
      ~/.wine/drive_c/users/$USER/AppData/Local/UWUVCI-V3

------------------------------------------------------------

Q29) “An error message popped up.”  
A) **Read it.**  
   UWUVCI’s messages are written to tell you exactly what’s wrong.  
   If it mentions a file, check that path.  
   If it says “missing dependency,” rerun the installer.  
   If it says “drive full,” free up space.  
   It’s not random — it’s there to help you.

============================================================
 📺 Extra Resources
============================================================
Official Video Guide:  
  https://www.youtube.com/watch?v=I5UdYcVSRSA&list=PLbQMtrmXFIxQ1hpvu9m1th41vsaqnZ2Id  

Discord Support:  
  https://discord.gg/mPZpqJJVmZ

============================================================
 End of ReadMe
============================================================
Maintained by ZestyTS — UWUVCI v3, the final and most stable version.
