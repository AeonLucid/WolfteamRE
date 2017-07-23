# WolfteamRE (Aeria Games)

WolfteamRE is an educational project to learn more about Reverse Engineering. The main goal of this project is to create a functional fake client which can be used to read data from the in-game channel. This data can then be used to create player and clan statistics.

> **Why wolfteam?**  
> Because Wolfteam is a fun and "old" game. Another reason being is the really out of date rankings [on the official website](http://wolfteam.aeriagames.com/wtgame/rankings).

## Projects

This repository consists of three projects.

### - [Wolfteam.Server](https://github.com/AeonLucid/WolfteamRE/tree/master/Wolfteam.Server)

This was intended to be a small server emulator (or packet logger) to figure out how the client responds to packets. I tried to modify the NyxLauncherEnc.xfs with the iXFS editor by CarlosX, but the game wouldn't connect to my server on localhost.

### - [Wolfteam.Playground](https://github.com/AeonLucid/WolfteamRE/tree/master/Wolfteam.Server)

This is used to check out sniffed packets from the Wolfteam client and figure out certain variables such as the AuthRandom used in the first login packet.

### - [Wolfteam.Client](https://github.com/AeonLucid/WolfteamRE/tree/master/Wolfteam.Client)

This is the main project, used to connect to Wolfteam and obtain data from players and/or clans. Not much to say here, the code contains comments where necessary so make sure to check those out.

## Instructions

### Obtaining an AES key for your account

This has been tested on the launcher of 2014/7/11 (Check out the date modified attribute on the launcher).

**The AES key is bound to your username, so using it for another account is not possible.**

Make sure you have wireshark running so you can obtain the "AuthRandom" in the next step.

1. Open the "Launcher.exe" in [OllyDBG](http://www.ollydbg.de/).
2. Press "CTRL+G", enter address "0043C6CB" and press "OK".
3. Press "F2" to place a breakpoint on the address. [Example](https://user-images.githubusercontent.com/4643257/28502646-ef768d62-6ff6-11e7-8dc7-8a583de91a5e.png)
4. Press "F9" to run the launcher, wait until it is loaded and sign in like you normally would. If you have done all previous steps correctly, it will hit the breakpoint and freeze the application.
5. In the "Registers" window, press on the value after "ECX", then right click on it and press "Follow in dump". [Example](https://user-images.githubusercontent.com/4643257/28502643-ef74417e-6ff6-11e7-83cb-7c093c505e8b.png)
6. Highlight the following from the dump and press "Copy > To clipboard" or "CTL+C". [Example](https://user-images.githubusercontent.com/4643257/28502645-ef75e290-6ff6-11e7-9d41-40224e271710.png)
7. Paste it in notepad and remove everything except for the hex dump itself. The AES key in my example is "15C3F064AD6738FD38F958443B581A9E".

### Obtaining the "AuthRandom"

Run the "DoStuff" method from the "Wolfteam.Playground" project, it will give you the "AuthRandom" in the console output. You **must** capture use the captured packet from when obtaining the AES key. Simply copy hex "hex stream" from the first packet sent to "78.138.122.21" on port "8444" (**Wireshark filter**: "ip.addr == 78.138.122.21").

## Generate the AES key and "AuthRandom" automatically

This is not possible *yet*. I think the method(s) responsible for generating these are:
- 0x436740 (Start of creating the login packet)
- 0x449A70 (Initializes the key)
- 0x44F100 (Generates a part of the key)
- 0x44F370 (Finalises the key)

Click [here](https://user-images.githubusercontent.com/4643257/28502644-ef753214-6ff6-11e7-9078-3d42beb1fc4b.png) for a screenshot of the pseudo code.

I am not experienced enough to reconstruct this code in C#, so any help with this would be very welcome.

It looks very familiar to how a GunboundBroker Emulator used to generate a key ([Image](https://user-images.githubusercontent.com/4643257/28502647-ef8a93fc-6ff6-11e7-8ab2-00b9f34532d0.png)). This did not work for Wolfteam, I assume the key generation has been altered a bit. If you want to check out the SpecialSHA class from GunboundBroker, click [here](https://github.com/AeonLucid/WolfteamRE/blob/master/Wolfteam.Playzone/Gunbound/SpecialSHA.cs).