# The Witness Puzzles

This is a puzzle game for Android (executable for Windows is also available), that i decided to create after completing [Jonathan Blow](https://twitter.com/jonathan_blow)'s game [The Witness](http://store.steampowered.com/app/210970/The_Witness).
I truly admire this game, and i wanted more of it. So i created The Witness Puzzles.  

In original The Witness there is an island to explore, filled with panels to solve.
A panel (usually) contains a grid with entry point, exit point and set of rules, and the goal is to draw the line from entry point to exit point in the way that satisfies all rules. That is not as easy as it sounds.

In my game i've made a procedural generator of such panels, so i can play these puzzles indefinitely.

<img src="./Screenshots/android 1.png" width="160" />&nbsp;
<img src="./Screenshots/android 2.png" width="160" />&nbsp;
<img src="./Screenshots/android 3.png" width="160" />&nbsp;
<img src="./Screenshots/android 4.png" width="160" />&nbsp;
<img src="./Screenshots/android 5.png" width="160" />&nbsp;

  
### Q: Where can i download it?
**A:** I don't have Google Play Console account as for now, so you can [download `.apk` here](https://github.com/SerGreen/TheWitnessPuzzles/releases/latest) and install it manually.
Supports Android 2.2 Froyo and above, but please note that the game may behave weird sometimes on Android 4.3 Jelly Bean or older. Android 4.4 KitKat or newer is advised.

You can also [download Windows version](https://github.com/SerGreen/TheWitnessPuzzles/releases/latest), but note that i developed it mainly for Android and controls on Windows may be not that handy.

### Q: How do i play it, again?
**A:** You have to draw a line from any of the start points to any of the end points, like in maze puzzles. But not every line will be accepted, because there are also some rules on a panel that you have to satisfy with your line.  
It has to be noted though that _**figuring out what those rules are in the first place is a big part of the original game**_, so my advise would be to go and play The Witness first to fully enjoy this masterpiece. But if you don't want to do it for some reason, but still want to play my game, i've made a [quick explanation of all the rules](https://github.com/SerGreen/TheWitnessPuzzles/blob/master/Puzzle%20Rules%20Guide/RulesGuide.md).


## Technologies and stuff
- .NET Framework 4.7 and C# 7.0
- [MonoGame Framework 3.6](http://www.monogame.net)
- Slightly modified [BloomFilter for Monogame and XNA](https://github.com/Kosmonaut3d/BloomFilter-for-Monogame-and-XNA) shader by Kosmonaut3d
- You'll need `Mobile development with .NET` workload as well as `Android NDK (R13B)` component in your Visual Studio in order to compile Android project

## Disclaimer
Sound effects i used are extracted sounds from the original The Witness that i found on the Internet. I do not own these assets, all rights to them belong to Jonathan Blow and Thekla Inc.  
All rights to The Witness belong to Jonathan Blow and Thekla Inc.
