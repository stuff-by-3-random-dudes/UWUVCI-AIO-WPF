Thanks for downloading UWUVCI-3!
If you didn't download us from the official source (https://github.com/stuff-by-3-random-dudes/UWUVCI-AIO-WPF/releases) then you might be using a custom version that someone else made.
If you're looking for the FAQ, keep scrolling.

If you're interested in the discord: 
https://discord.gg/mPZpqJJVmZ

If you're curious about the latest changes: 
https://github.com/stuff-by-3-random-dudes/UWUVCI-AIO-WPF/releases/latest

If you want to check out the Official UWUVCI 3 Video Series:
https://www.youtube.com/watch?v=I5UdYcVSRSA&list=PLbQMtrmXFIxQ1hpvu9m1th41vsaqnZ2Id

By the time you're reading this, active development on UWUVCI-3 would have ended.
I, ZestyTS, have been the active developer on UWUVCI-3 since late 2020, and I have been helping the original creator on making UWUVCI-Prime (aka 4).
For history as to what I've done on the project, I fixed a lot of bugs, and helped get UWUVCI-3 out of beta while introducing new features to UWUVCI like Widescreen for N64, DarkFilter removal for N64 and GBA, C2W for Wii, added support for Win7/8, partial support for Wine, etc...

If you'd like to donate to me
https://ko-fi.com/zestyts

If you'd like to donate to the creator, NicoAICP
https://ko-fi.com/uwuvci

If you have any questions about anything, feel free to reach out on the discord.


##############################################################################################################################################################################
									FAQ
##############################################################################################################################################################################
I, ZestyTS, have been making this FAQ since I took over UWUVCI-3, so I've done my best to make sure to label the versions each of these steps no longer work for.


1) I don't know how to use UWUVCI, can you show me?
	https://uwuvci-prime.github.io/UWUVCI-Resources/index
	Select console and follow the guide
 
2)  What games are compatible?
	https://uwuvci-prime.github.io/UWUVCI-Resources/index
	Select console, then select "Compatibility"
	This compatibility guide is community driven, so some different setups may work
	For GCN, nothing is listed because Rhythm Heaven Fever works as a base for practically all the games

3) It's hanging at "Downloading Tools"
	https://github.com/NicoAICP/UWUVCI-Tools
	Download these tools and put them in
	Documents/UWUVCI AIO/bin/Tools
	Overwrite if it gives you a prompt

4) When I inject the game, the game isn't being made (applicable for v3.0.4 and below)
	https://github.com/NicoAICP/CNUS_Packer/releases/tag/v1.1
	Download Win10-x64.rar
	Extract the files and place it in
	Documents/UWUVCI AIO/bin/Tools
	Overwrite if it gives you a prompt

5) UWUVCI is stuck on "Copying Injected Game" (applicable for v3.0.4 and below)
	https://github.com/NicoAICP/CNUS_Packer/releases
	Download the rar in there and put its contents under Documents/uwuvci aio/bin/tools and allow it to overwrite stuff

6) I don't understand what it means by "Base" in the drop down menu?
	This is the game that will be used as the base for the inject

7) What does it mean by "Base not downloaded"
	It means the base game can not be found
	Clicking the button "Enter TKey" and entering in the Title Key will fix that issue

8) How do I get the Title Key?
	Buy the base from the eshop
	Use Tik2SD to dump the title key

9) What does it mean by Common Key?
	This is the common key for your Wii U
	Using the nandbackup of your Wii U, there is a file named otp.bin
	Load the otp.bin file to get pass this
	If you don't have a nandbackup, you can follow these steps:
		Tiramisu:
		     https://wiiu.hacks.guide/#/tiramisu/nand-backup
		Mocha:
		     https://wiiu.hacks.guide/#/archive/mocha/online-exploit/nand-backup?id=archive-mocha-online-exploit
		Haxchi:
		    https://wiiu.hacks.guide/#/archive/haxchi/nand-backup

10) The base is taking a while to download
	Are you trying to inject a GCN or a Wii game?
	If the answer is no, then the base should download in less than 5 minutes
	If the answer is yes, then it might be because of what the base is
	Xenoblade Chronicles takes a very long time to get, there are reports of more than an hour
	Internet speed does play a role in this, the game is ~8.2GB
	Explanation: 
		8.2GB * 8 = 65,365Gb
		65,365Gb * 1024 = 67,108,864Mb

		67108864 / 10(mb per s) = 6710886.4 seconds
		6710886.4 / 3600 = 1864 hours
		1864 / 24 = ~78 days

11) My anti virus said [insert anything]
	This program doesn't have anything malicious in it

12) I can't find my game when I click the "Rom Path" button
	Your game is in a format that UWUVCI can't read
	You'll need to undo whatever you did to get it back to it's original form
	We don't condone piracy, so we won't really help past this

13) Wup install failing/Error 199-9999
	Enable cfw first
	Verify Wup files are complete and correct
	Re-injecting might solve the issue

14) UWUVCI crashes or the injects aren't being made
	Double check that .Net Core is installed (applicable for v3.0.4 and below) (applicable for v3.F and up)
		If it's not, install here:
			https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.6-windows-x64-installer
	Double check that .Net 6 is installed (applicable for v3.99s)
		If it's not, install here:
			(x64 version)
			https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.8-windows-x64-installer
			(x86 version)
			https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.8-windows-x86-installer

15) UWUVCI is stuck at downloading needed data (applicable for v3.0.4 and below)
	Download and extract this zip's content where you have UWUVCI installed (usually Documents/UWUVCI AIO)
	https://cdn.discordapp.com/attachments/388351824714792970/763296223251136512/Needed_Data_FIX.zip

16) Inject is created, but the game is having issues
	Rhythm Heaven US as a base works on nearly everything
	Stick to using the correct base/game for your region
	Make sure the base game is bigger or equal in size to the game you're trying to inject
	N64 games are notorious for giving issues, they may require different bases
	Check out the compatibility guide listed earlier

17) UWUVCI is throwing an error that reads something like "Could not find file 'bin\temp\pre.iso'"
	More than likely you have a bad dump of a game or UWUVCI doesn't like the trimmed dump
	Example: 
		if the dump is an "nkit" or a "wbfs" please try using the iso version instead

18) TGA image files will not render a preview and could cause issues in UWUVCI
	If the images are pulled by UWUVCI for you, you can manually grab the tga files and convert them to png (just google this)
	You can find where these images are stored by going here:
		https://github.com/UWUVCI-Prime/uwuvci-images
		And looking under the right console and the GameID for the game you're trying to use.
 
19) If you're having issues and the fix isn't listed here, see about updating UWUVCI.
	The latest version can be found here:
	https://github.com/stuff-by-3-random-dudes/UWUVCI-AIO-WPF/releases/latest
 
20) Help with Rom Mods and Hacks
	Don't expect that much help because mods or hacks add an extra level of complexity.
	If the mods work with real hardware then there's a chance it will work on the Wii U
	Ask the mod/hack's community, they'll be more helpful
	It's possible that someone already tried it, check out the compatibility guide.
 
21) Having problems with GCN injects?
	Usually has to do with Nintendont, you can check out their compatibility list here: https://wiki.gbatemp.net/wiki/Nintendont_Compatibility_List
	Here's their main GBATemp thread: 
		https://gbatemp.net/threads/nintendont.349258/
 
22) If you've tried everything in this list then there are a few more things
	Turn off Antivirus software, it can block downloads or block somethings from running
	Run as Admin
	If it still isn't working then please feel free to ask for help in the discord under the support channel

23) GC injects boot to the nintendont menu
	You probably used TeconMoon injector before. 
	Delete nincfg.bin from the root of your sd card and the apps/nintendont folder, then do the sd setup again in UWUVCI.

24) Can't find injects in SaveMii
	You'll want to use a modified version called "SaveMii Inject MOD"

25) Injections randomly failing or hanging? (3.99+)
	Close out of UWUVCI via Task Manager then
	Try deleting the Tools folder:
	Documents/UWUVCI AIO/bin/Tools
	Reopen UWUVCI and try inject again

26) ??? injects giving you problems?
	??? means that the images didn't properly get loaded into the inject
	Try using images that aren't automatically grabbed from UWUVCI

27) Pre.iso error
	You are most likely injecting with a wbfs or nkit.iso file, this file has data trimmed.
	Some of that data that was trimmed is required to make an inject.
	Solution to error would be to re-rip from disk and use the iso file.

28) Path .../temp/temp missing
	There seems to be an error with the images
	Rechoose your rom so that UWUVCI can retry downloading them, or
	create new images, or
	remove your added images

29) Pre.iso error #2
	Your Filename might have unsupported characters. Rename your iso you want to inject to for example "sample.iso" and try again.

30) Error 199-9999 when launching inject on Aroma
	Download https://github.com/V10lator/SigpatchesModuleWiiU/releases/download/v1.0/01_sigpatches.rpx and place it in sd:/wiiu/environments/aroma/modules/setup

31) Tmd.bin error? Common/Title Key not working?
	Wine/Proton/Virtualization are known to be a little wonky.
	If it's a Title Key problem, try using these bases
		https://github.com/NicoAICP/UWUVCI-VCB/tree/master/Net6

32) Official Video Guide?
	https://www.youtube.com/watch?v=I5UdYcVSRSA&list=PLbQMtrmXFIxQ1hpvu9m1th41vsaqnZ2Id