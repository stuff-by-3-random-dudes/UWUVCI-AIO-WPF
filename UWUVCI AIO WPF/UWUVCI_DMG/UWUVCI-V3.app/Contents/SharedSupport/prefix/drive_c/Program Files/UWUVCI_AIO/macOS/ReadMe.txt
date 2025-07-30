Thanks for using UWUVCI-V3-HELPER.

This is a helper console app for Unix (non-Windows) users.
The purpose of this app is to help unix users get past the hurdles that some tools have not running in Wine.
While using the app, please read the console to see what it's doing, it clues you in on everything, so you shouldn't be confused as to what's going on.

I recommend you check out the guide video to have an idea of how this software works:
https://www.youtube.com/watch?v=aIXhjbinZc4&list=PLbQMtrmXFIxQ1hpvu9m1th41vsaqnZ2Id&index=8

Note: There is not a discord server for this app, or anything, I am the only person who knows how this works, please keep that in mind.

If you'd like to donate to me:
https://ko-fi.com/zestyts

If you have any questions, check out the FAQ below.

##############################################################################################################################################################################
									FAQ
##############################################################################################################################################################################
1) When do I run this app?
	Starting in UWUVCI V3Z-B there will be a pop-up window telling you to run this software.
	Read everything in the pop-up window so you know what to do.

2) How do I run this app?
	As the pop-up explains, open up a terminal, go to where UWUVCI is installed.
	Look for the folder of the system you're on (linux or mac)
	In there you will find a file called "UWUVCI-V3-Helper"
	Execute that file: "./UWUVCI-V3-Helper"

3) The application is not running.
	Sounds like you need to make the tools and applications executable.
	Do "sudo chmod +x" to the WIT, WSTRT, and UWUVCI-V3-HELPER files.

4) The application is still not running after doing chmod +x
	Linux users won't have any issues after that, but Mac users will!
	Thank Apple because this wlil get midly annoying.
	Since Apple won't approve of the app when I use my dev credentials, it's by default blocked, to unblock it you must run this line exactly how it's written, this includes the "." at the end
		sudo xattr -r -d com.apple.quarantine .

5) The application runs, but [insert tool name] is timing out
	So, this is a bit silly, the tool is packaged as both ARM and x86, but for some reason the ARM version shits out randomly.
	I haven't seen this issue on Linux, but they have distros, derivated distros, and flavors so I wouldn't be surprised if they run into it also
	Since I have an M2 Mac, that means I'm using Apple Silicon, aka ARM.
	What you'll need to do is either look at the tools.json file that lives one directory up, or read the console to see what command it tried to run, you will then force it to run as x86.
	Ths will have to be done outside of the helper app, here's an example of me using wit to copy a folder to a destination iso
		arch -x86_64 ./wit-mac copy "/users/zestyts/Downloads/Release/bin/temp/TEMP" --DEST "/users/zestyts/Downloads/Release/bin/temp/game.iso"
	Do not expect your command to look like mine 1 to 1, I'm the developer so my stuff is set up a bit differently

6) The helper app doesn't work, it keeps erroring out
	As stated earlier, the console app clues you in on everything, so just read what it's trying to do.
	If it's returning an error it's because the path that it's saying where the folder or file lives, doesn't actually exist.
	The helper app tries to figure out where the paths are stored, but it struggled a bit.
	Easiest solution would be to open up the "tools.json" file that lives one directory above this folder and edit the path to where it actually exists.
	Sure, this isn't automated, but now you have a way to make Wii/GCN injects
	Here's an example of what I mean:
		Let's say your tools.json file looks like this
			[{"ToolName":"wit","Arguments":"copy --source \"C:\\users\\zestyts\\kms\\SF8E01.wbfs\" --dest \"Z:\\home\\zestyts\\kms\\UWUVCI V3\\bin\\temp\\pre.iso\" -I","CurrentDirectory":"Z:\\home\\zestyts\\kms\\UWUVCI V3\\","Function":"Wii"}]
		You should already be able to see what the problem is, on Mac there is no "home" directory and on Linux there is no "users" directory, so somehow UWUVCI v3 incorrectly grabbed the path
		If we wanted to update to fix this file as a Linux user, we would update it to be this:
			[{"ToolName":"wit","Arguments":"copy --source \"C:\\home\\zestyts\\kms\\SF8E01.wbfs\" --dest \"Z:\\home\\zestyts\\kms\\UWUVCI V3\\bin\\temp\\pre.iso\" -I","CurrentDirectory":"Z:\\home\\zestyts\\kms\\UWUVCI V3\\","Function":"Wii"}]
		Obviously for Mac, you would change it the other way.
		Note: You could actually just write in the full correct path without the Windows paths so you could change it to be
			[{"ToolName":"wit","Arguments":"copy --source \"/users/zestyts/kms/SF8E01.wbfs\" --dest \"/home/zestyts/kms/UWUVCI V3/bin/temp\\pre.iso\" -I","CurrentDirectory":"/home/zestyts/kms/UWUVCI V3/","Function":"Wii"}]
		The helper app tries to fix the paths on your behalf so doing all of that extra work might not really be necessary.

7) I have a question that isn't answered here
	Reach out, I'll do my best to help out.
	I don't have a Discord Server or anything like that, so it might be best to reach out via the YouTube video that I linked above.