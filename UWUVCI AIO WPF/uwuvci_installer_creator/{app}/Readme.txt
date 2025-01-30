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
For history as to what I've done on the project, I fixed a lot of bugs, and helped get UWUVCI-3 out of beta while introducing new features to UWUVCI like Widescreen for N64, DarkFilter removal for N64 and GBA, C2W for Wii, added support for Win7/8, support for Unix (helper app), etc...

If you'd like to donate to me
https://ko-fi.com/zestyts

If you'd like to donate to the creator, NicoAICP
https://ko-fi.com/uwuvci

If you have any questions about anything, feel free to reach out on the discord.


##############################################################################################################################################################################
									FAQ
##############################################################################################################################################################################
I, ZestyTS, have been making this FAQ since I took over UWUVCI-3, and with the update of V3.Z-B a bunch of things were overhauled or changed, so much that the FAQ needs to be completely redone.


1) I don't know how to use UWUVCI, can you show me?
	https://uwuvci-prime.github.io/UWUVCI-Resources/index
	Select console and follow the guide
 
2)  What games are compatible?
	https://uwuvci.net/
	On the top right, click "Compatibility" followed by the console.
	This compatibility guide is community driven, so some different setups may work
	For GCN, nothing is listed because Rhythm Heaven Fever works as a base for practically all the games

3) I don't understand what it means by "Base" in the drop down menu?
	This is the game that will be used as the base for the inject

4) What does it mean by "Base not downloaded"
	It means the base game can not be found
	Clicking the button "Enter TKey" and entering in the Title Key will fix that issue

5) How do I get the Title Key?
	Buy the base from the eshop
	Use Tik2SD to dump the title key

6) What does it mean by Common Key?
	This is the common key for your Wii U
	Using the nandbackup of your Wii U, there is a file named otp.bin
	Load the otp.bin file to get pass this
	If you don't have a nandbackup, you can follow this guide:
		https://wiiu.hacks.guide/aroma/nand-backup.html

7) The base is taking a while to download
	Are you trying to inject a GCN or a Wii game?
	If the answer is no, then the base should download in less than 5 minutes
	If it's taking a while anyway, it probably has to do with Nintendo's servers, trying again later might be the solution.
	If the answer is yes, then it might be because of what the base is
	Xenoblade Chronicles takes a very long time to get, there are reports of more than an hour
	Internet speed does play a role in this, the game is ~8.2GB
	Explanation: 
		8.2GB * 8 = 65,365Gb
		65,365Gb * 1024 = 67,108,864Mb

		67108864 / 10(mb per s) = 6710886.4 seconds
		6710886.4 / 3600 = 1864 hours
		1864 / 24 = ~78 days

8) My anti virus said [insert anything]
	This program doesn't have anything malicious in it

9) I can't find my game when I click the "Rom Path" button
	Your game is in a format that UWUVCI can't read
	You'll need to undo whatever you did to get it back to it's original form
	We don't condone piracy, so we won't really help past this

10) Wup install failing/Error 199-9999
	Download https://github.com/V10lator/SigpatchesModuleWiiU/releases/download/v1.0/01_sigpatches.rpx and place it in sd:/wiiu/environments/aroma/modules/setup

11) GCN/Wii Injects not working
	There are reports that SDUSB or ISFShax might be the reason why

12) Inject is created, but the game is having issues
	Rhythm Heaven US as a base works on nearly everything
	Stick to using the correct base/game for your region
	Make sure the base game is bigger or equal in size to the game you're trying to inject
	N64 games are notorious for giving issues, they may require different bases
	Check out the compatibility guide listed earlier

13) UWUVCI is throwing an error that reads something like "Could not find file 'bin\temp\pre.iso'"
	More than likely you have a bad dump of a game or UWUVCI doesn't like the trimmed dump
	Example: 
		if the dump is an "nkit" or a "wbfs" please try using the iso version instead

14) If you're having issues and the fix isn't listed here, see about updating UWUVCI.
	The latest version can be found here:
	https://github.com/stuff-by-3-random-dudes/UWUVCI-AIO-WPF/releases/latest
 
15) Help with Rom Mods and Hacks
	Don't expect that much help because mods or hacks add an extra level of complexity.
	If the mods work with real hardware then there's a chance it will work on the Wii U
	Ask the mod/hack's community, they'll be more helpful
	It's possible that someone already tried it, check out the compatibility guide.
 
16) Having problems with GCN injects?
	Usually has to do with Nintendont, you can check out their compatibility list here: https://wiki.gbatemp.net/wiki/Nintendont_Compatibility_List
	Here's their main GBATemp thread: 
		https://gbatemp.net/threads/nintendont.349258/
 
17) If you've tried everything in this list then there are a few more things
	See if your antivirus is getting in the way.
	Check to see if you installed to a OneDrive directory.

18) GCN injects boot to the nintendont menu
	You probably used TeconMoon injector before. 
	Delete nincfg.bin from the root of your sd card and the apps/nintendont folder, then do the sd setup again in UWUVCI.

19) Can't find injects in SaveMii
	You'll want to use a modified version called "SaveMii Inject MOD"

20) Path .../temp/temp missing
	Same issue as with Pre.iso, please redump and try again.

21) tmd.bin can't be found
	Same issue as with /temp/temp, please redump and try again

22) Official Video Guide?
	https://www.youtube.com/watch?v=I5UdYcVSRSA&list=PLbQMtrmXFIxQ1hpvu9m1th41vsaqnZ2Id

23) UWUVCI is broken or acting funny
	Check to see if you installed to OneDrive or if your Antivirus is blocking something

24) GB/C games aren't saving when using VC's reset button
	This is because it's using Goombacolor, please use the button reset combination instead

25) boot.dol not found
	UWUVCI mentioned after the GCN inject was done that your SD card must be setup with Nintendont

26) Stuck on "Copying to SD"
	Manually copy the folder yourself.
	Go to where UWUVCI is installed, and then look for the folder "InjectedGames"
	You'll find the folder you're looking for there, just copy it to the install folder on your sd card.

27) UWUVCI says that my drive is full (12G)
	The drive you installed UWUVCI does not have enough space, please install UWUVCI to a different drive that has enough space and try again.

28) Nkit error?
	Sounds like you pirated your game, don't do that.

29) UWUVCI doesn't open
	You're missing .Net Framework 4.8.1 Runtime, download/install it from here:
		https://dotnet.microsoft.com/en-us/download/dotnet-framework/net481

30) UWUVCI's progress bar gets stuck (outside of Downloading base)
	Try updating your tools (gear icon to the top right -> "Update Tools")
	