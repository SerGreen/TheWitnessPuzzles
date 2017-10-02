![TWP_logo](./Screenshots/TWP_Logo.png)

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

### Q: What is the Library?
**A:** Library stores last 12 solved panels on the first tab, last 12 skipped panels on the second tab and all your saved panels on the last tab (up to 1000 actually). You can save the panel you liked with the heart button ❤. You can replay any panel from the Library later.


## Technologies and stuff
- .NET Framework 4.7 and C# 7.0
- [MonoGame Framework 3.6](http://www.monogame.net)
- Slightly modified [BloomFilter for Monogame and XNA](https://github.com/Kosmonaut3d/BloomFilter-for-Monogame-and-XNA) shader by Kosmonaut3d
- You'll need `Mobile development with .NET` workload (namely `Xamarin`) as well as `Android NDK (R13B)` component in your Visual Studio in order to compile Android project

## Solution structure
- `TWP Android` – Android build project, contains only main activity and configurations.
- `TWP Desktop` – Windows build project, contains only Main() method and configurations.
- `TWP Shared` – this is the first of the two main projects, contains all the game code. Its code is shared by Android and Desktop projects.
- `TWPBaseLib` – this is the second of the two main projects, contains all the core logic and algorithms.
- `TWPVisualizer` – this is more of a legacy project, where i tested BaseLib functionality. Should not be used. GUI totally is not foolproof.


## Disclaimer
Sound effects i used are extracted sounds from the original The Witness that i found on the Internet. I do not own these assets, all rights to them belong to Jonathan Blow and Thekla Inc.  
All rights to The Witness belong to Jonathan Blow and Thekla Inc.

&nbsp;
***
&nbsp;

## How to implement new panel generator class
_This note is for future me or anyone who wants to fork._  
Class should inherit from abstract `PanelGenerator` and should be registered in `DependencyInjector`'s static constructor as the default generator:
```c#
static DI()
{
    binds = new Dictionary<Type, object>();
    binds.Add(typeof(PanelGenerator), MyPanelGenerator.Instance);
}
```
Panel consists from three types of parts: nodes, edges and blocks.
- Node can be empty node, start point, end point or marked, i.e. has a hexagon rule on it. They have IDs and are numbered from 0 starting from top-left node, line by line.
- Edge connects two nodes. Edge can be normal, broken or marked, i.e. has a hexagon rule on it. They have IDs, that are determined as `nodeA_ID * 100 + nodeB_ID`, where nodeA is a node with the lowest ID number. For example, the ID of the edge between nodes 1 and 5 is 105 (0105), between 16 and 21 is 1621, between 0 and 6 is just 6 (0006).
- Blocks are the squares between the edges. Block can has a rule attached to it, such as Triangle, Sun, Tetris, etc.

You can create regular panel or symmetric panel like this:
```c#
// panelWidth and panelHeight are the size of the panel in blocks
Puzzle panel    = new Puzzle(panelWidth, panelHeight, palette.SingleLineColor, palette.BackgroundColor, palette.WallsColor, palette.ButtonsColor, seed);
Puzzle symPanel = new SymmetryPuzzle(panelWidth, panelHeight, isYSymmetry, isMirrorLineTransparent, palette.MainLineColor, palette.MirrorLineColor, palette.BackgroundColor, palette.WallsColor, palette.ButtonsColor, seed);
```
Now you can add rules, start and end points. Note, that it's your responsibility to add symmetrical start and end points to symmetric panel.  
```c#
// You can access nodes like this
panel.Nodes[startNodeID].SetState(NodeState.Start);
panel.Nodes[endNodeID].SetState(NodeState.End);
panel.Nodes[markedNodeID].SetStateAndColor(NodeState.Marked, Color.Yellow);

// You can access edges like this
panel.Edges.Find(x => x.Id == brokenEdgeID)?.SetState(EdgeState.Broken);
panel.Edges.Where(x => x.State == EdgeState.Normal).First().SetStateAndColor(EdgeState.Marked, Color.Aqua);

// You can set rules to blocks like this
panel.Grid[x, y].Rule = new ColoredSquareRule(Color.Magenta);
panel.Grid[2, 0].Rule = new SunPairRule(Color.Magenta);
panel.Grid[3, 1].Rule = new TriangleRule(3);
panel.Grid[4, 2].Rule = new EliminationRule(); // Eraser rule
panel.Grid[2, 3].Rule = new EliminationRule(Color.Magenta);
// Tetromino shape is set by bool[,] array
// Note, that it's transposed, i.e. it is filled by columns, not rows. Created shape is ══╝
panel.Grid[5, 2].Rule = new TetrisRule(new bool[,] { { false, true }, 
                                                     { false, true }, 
                                                     { true,  true } },
                                       isSubtractiveShape);
// This shape is ═╩═
panel.Grid[4, 3].Rule = new TetrisRotatableRule(new bool[,] { { false, true }, 
                                                              { true,  true }, 
                                                              { false, true } }, 
                                                isSubtractiveShape);
```
