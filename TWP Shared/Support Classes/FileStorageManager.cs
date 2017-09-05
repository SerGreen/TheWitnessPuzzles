using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using TheWitnessPuzzles;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;

namespace TWP_Shared
{
    public static class FileStorageManager
    {
        private static readonly string SETTINGS_FILE = "settings.cfg";
        private static readonly string CURRENT_PANEL_FILE = "current.panel";
        private static readonly string SOLVED_DIR = "lastSolved";
        private static readonly string DISCARDED_DIR = "lastDiscarded";
        private static readonly string FAVOURITE_DIR = "favourite";
        private static readonly string SOLVED_PANEL_FILE = "solved{0}_{1}.panel";
        private static readonly string DISCARDED_PANEL_FILE = "discarded{0}_{1}.panel";
        private static readonly string FAVOURITE_PANEL_FILE = "fav{0}_{1}.panel";

        private static readonly Regex ExtractSeedFromName = new Regex(@"(?<=_).+(?=\.panel)");
        private static readonly Regex ExtractNumberFromName = new Regex(@"\d+(?=_)");

        private static readonly int MAX_HISTORY_LENGTH = 12;

        private static IsolatedStorageFile isolatedStorage;

        static FileStorageManager()
        {
#if WINDOWS
            isolatedStorage = IsolatedStorageFile.GetUserStoreForDomain();
#else
            isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
#endif    
            SOLVED_PANEL_FILE = Path.Combine(SOLVED_DIR, SOLVED_PANEL_FILE);
            DISCARDED_PANEL_FILE = Path.Combine(DISCARDED_DIR, DISCARDED_PANEL_FILE);
            FAVOURITE_PANEL_FILE = Path.Combine(FAVOURITE_DIR, FAVOURITE_PANEL_FILE);

            if (!isolatedStorage.DirectoryExists(SOLVED_DIR))
                isolatedStorage.CreateDirectory(SOLVED_DIR);
            if (!isolatedStorage.DirectoryExists(DISCARDED_DIR))
                isolatedStorage.CreateDirectory(DISCARDED_DIR);
            if (!isolatedStorage.DirectoryExists(FAVOURITE_DIR))
                isolatedStorage.CreateDirectory(FAVOURITE_DIR);
        }

        /// <summary>
        /// Returns game settings as a string, values are separated with ':' character
        /// </summary>
        public static string LoadSettingsFile()
        {
            // Open isolated storage and read settings file
            if (isolatedStorage.FileExists(SETTINGS_FILE))
            {
                using (IsolatedStorageFileStream fs = isolatedStorage.OpenFile(SETTINGS_FILE, FileMode.Open))
                {
                    if (fs != null)
                    {
                        using (StreamReader reader = new StreamReader(fs))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }

            return null;
        }
        public static void SaveSettingsFile(string file)
        {
            // Open isolated storage and write the settings file
            using (IsolatedStorageFileStream fs = isolatedStorage.OpenFile(SETTINGS_FILE, FileMode.Create))
            {
                if (fs != null)
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.Write(file);
                    }
                }
            }
        }

        public static void SaveCurrentPanel(Puzzle currentPanel) => SavePanelToFile(currentPanel, CURRENT_PANEL_FILE);
        public static Puzzle LoadCurrentPanel() => LoadPanelFromFile(CURRENT_PANEL_FILE);
        public static void DeleteCurrentPanel() => DeletePanel(CURRENT_PANEL_FILE);

        public static void AddPanelToSolvedList(Puzzle panel)
        {
            AddPanelToLastNList(panel, SOLVED_PANEL_FILE);
            // If solved panel was on discarded list, then remove it from discarded
            DeletePanelFromList(panel, DISCARDED_PANEL_FILE);
        }
        public static void AddPanelToDiscardedList(Puzzle panel) => AddPanelToLastNList(panel, DISCARDED_PANEL_FILE);
        private static void AddPanelToLastNList(Puzzle panel, string fileNamePattern)
        {
            // Get list of saved panels
            string[] fileNames = isolatedStorage.GetFileNames(string.Format(fileNamePattern, "???", "*"));
            int listLength = fileNames.Length;

            // If given panel is already saved, then abort saving
            if (fileNames.Any(x => ExtractSeedFromName.Match(x).Value == panel.Seed.ToString("D9")))
                return;

            // If there are saved all N panels, then delete the last one
            if (listLength >= MAX_HISTORY_LENGTH)
            {
                string seed = ExtractSeedFromName.Match(fileNames[MAX_HISTORY_LENGTH - 1]).Value;
                DeletePanel(string.Format(fileNamePattern, (MAX_HISTORY_LENGTH - 1).ToString("D3"), seed));
                listLength--;
            }

            // Shift all panels by one place, so the 2nd becomes 3rd and so on
            for (int i = listLength - 1; i >= 0; i--)
            {
                string seed = ExtractSeedFromName.Match(fileNames[i]).Value;
                isolatedStorage.MoveFile(string.Format(fileNamePattern, i.ToString("D3"), seed), string.Format(fileNamePattern, (i + 1).ToString("D3"), seed));
            }

            // Now save the new panel as the 1st
            SavePanelToFile(panel, string.Format(fileNamePattern, "000", panel.Seed.ToString("D9")));
        }

        /// <summary>
        /// Deletes panels's file and shifts all panels after it backwards
        /// </summary>
        private static void DeletePanelFromList(Puzzle panel, string fileNamePattern)
        {
            // Get this panel's file name
            string[] file = isolatedStorage.GetFileNames(string.Format(fileNamePattern, "???", panel.Seed.ToString("D9")));
            if (file.Length > 0)
            {
                string dir = Path.GetDirectoryName(fileNamePattern);
                DeletePanel(Path.Combine(dir, file[0]));
                // Shift panels, that were after deleted one
                string[] fileNames = isolatedStorage.GetFileNames(string.Format(fileNamePattern, "???", "*"));
                // Extract number of deleted panel's file
                int number = int.Parse(ExtractNumberFromName.Match(file[0]).Value);
                // Shift panels backwards
                for (int i = number; i < fileNames.Length; i++)
                {
                    string seed = ExtractSeedFromName.Match(fileNames[i]).Value;
                    isolatedStorage.MoveFile(string.Format(fileNamePattern, (i + 1).ToString("D3"), seed), string.Format(fileNamePattern, i.ToString("D3"), seed));
                }
            }
        }

        public static void AddPanelToFavourites(Puzzle panel)
        {
            int favsCount = GetFavouritePanelsNames().Length;
            SavePanelToFile(panel, string.Format(FAVOURITE_PANEL_FILE, favsCount.ToString("D3"), panel.Seed.ToString("D9")));
        }
        public static void DeletePanelFromFavourites(Puzzle panel) => DeletePanelFromList(panel, FAVOURITE_PANEL_FILE);
        public static bool IsPanelInFavourites(Puzzle panel) => isolatedStorage.GetFileNames(string.Format(FAVOURITE_PANEL_FILE, "???", panel.Seed.ToString("D9"))).Length > 0;

        public static string[] GetSolvedPanelsNames() => isolatedStorage.GetFileNames(string.Format(SOLVED_PANEL_FILE, "???", "*")).Select(x => Path.Combine(SOLVED_DIR, x)).ToArray();
        public static string[] GetDiscardedPanelsNames() => isolatedStorage.GetFileNames(string.Format(DISCARDED_PANEL_FILE, "???", "*")).Select(x => Path.Combine(DISCARDED_DIR, x)).ToArray();
        public static string[] GetFavouritePanelsNames() => isolatedStorage.GetFileNames(string.Format(FAVOURITE_PANEL_FILE, "???", "*")).Select(x => Path.Combine(FAVOURITE_DIR, x)).ToArray();

        private static void SavePanelToFile(Puzzle panel, string fileName)
        {
            using (IsolatedStorageFileStream fs = isolatedStorage.OpenFile(fileName, FileMode.Create))
            {
                if (fs != null)
                {
                    using (BinaryWriter br = new BinaryWriter(fs))
                    {
                        // Seed of panel
                        br.Write(panel.Seed);                               // int

                        // Panel size
                        br.Write(panel.Width);                              // int
                        br.Write(panel.Height);                             // int

                        // Is panel symmetric and panel colors
                        if (panel is SymmetryPuzzle sym)
                        {
                            br.Write(true);                                 // bool
                            br.Write(sym.Y_Mirrored);                       // bool
                            br.Write(sym.MainColor.PackedValue);            // uint
                            br.Write(sym.MirrorColor.PackedValue);          // uint
                        }
                        else
                        {
                            br.Write(false);                                // bool
                            br.Write(panel.MainColor.PackedValue);          // uint
                        }

                        br.Write(panel.BackgroundColor.PackedValue);        // uint
                        br.Write(panel.WallsColor.PackedValue);             // uint
                        br.Write(panel.ButtonsColor.PackedValue);           // uint

                        // Start nodes
                        List<Node> startNodes = panel.Nodes.Where(x => x.State == NodeState.Start).ToList();
                        br.Write(startNodes.Count);                         // int
                        foreach (var start in startNodes)
                            br.Write(start.Id);                             // int

                        // End nodes
                        List<Node> endNodes = panel.Nodes.Where(x => x.State == NodeState.Exit).ToList();
                        br.Write(endNodes.Count);                           // int
                        foreach (var end in endNodes)
                            br.Write(end.Id);

                        // Marked nodes
                        List<Node> markedNodes = panel.Nodes.Where(x => x.State == NodeState.Marked).ToList();
                        br.Write(markedNodes.Count);                        // int
                        foreach (var node in markedNodes)
                        {
                            br.Write(node.Id);                              // int
                            br.Write(node.HasColor);                        // bool
                            if (node.HasColor)
                                br.Write(node.Color.Value.PackedValue);     // uint
                        }

                        // Broken edges
                        List<Edge> brokenEdges = panel.Edges.Where(x => x.State == EdgeState.Broken).ToList();
                        br.Write(brokenEdges.Count);                        // int
                        foreach (var edge in brokenEdges)
                            br.Write(edge.Id);                              // int

                        // Marked edges
                        List<Edge> markedEdges = panel.Edges.Where(x => x.State == EdgeState.Marked).ToList();
                        br.Write(markedEdges.Count);                        // int
                        foreach (var edge in markedEdges)
                        {
                            br.Write(edge.Id);                              // int
                            br.Write(edge.HasColor);                        // bool
                            if (edge.HasColor)
                                br.Write(edge.Color.Value.PackedValue);     // uint
                        }

                        // Colored square rules
                        List<Block> coloredSquareBlocks = panel.Blocks.Where(x => x.Rule is ColoredSquareRule).ToList();
                        br.Write(coloredSquareBlocks.Count);                // int
                        foreach (var block in coloredSquareBlocks)
                        {
                            var rule = block.Rule as ColoredSquareRule;
                            br.Write(block.Id);                             // int
                            br.Write(rule.Color.Value.PackedValue);         // uint
                        }

                        // Sun rules
                        List<Block> sunBlocks = panel.Blocks.Where(x => x.Rule is SunPairRule).ToList();
                        br.Write(sunBlocks.Count);                          // int
                        foreach (var block in sunBlocks)
                        {
                            var rule = block.Rule as SunPairRule;
                            br.Write(block.Id);                             // int
                            br.Write(rule.Color.Value.PackedValue);         // uint
                        }

                        // Triangle rules
                        List<Block> triangleBlocks = panel.Blocks.Where(x => x.Rule is TriangleRule).ToList();
                        br.Write(triangleBlocks.Count);                     // int
                        foreach (var block in triangleBlocks)
                        {
                            var rule = block.Rule as TriangleRule;
                            br.Write(block.Id);                             // int
                            br.Write(rule.Power);                           // int
                        }

                        // Elimination rules
                        List<Block> eliminationBlocks = panel.Blocks.Where(x => x.Rule is EliminationRule).ToList();
                        br.Write(eliminationBlocks.Count);                  // int
                        foreach (var block in eliminationBlocks)
                        {
                            var rule = block.Rule as EliminationRule;
                            br.Write(block.Id);                             // int
                            br.Write(rule.HasColor);                        // bool
                            if (rule.HasColor)
                                br.Write(rule.Color.Value.PackedValue);     // uint
                        }

                        // Tetris rules
                        List<Block> tetrisBlocks = panel.Blocks.Where(x => x.Rule is TetrisRule).ToList();
                        br.Write(tetrisBlocks.Count);                       // int
                        foreach (var block in tetrisBlocks)
                        {
                            var rule = block.Rule as TetrisRule;
                            br.Write(block.Id);                             // int
                            br.Write(rule.HasColor);                        // bool
                            if (rule.HasColor)
                                br.Write(rule.Color.Value.PackedValue);     // uint

                            br.Write(rule is TetrisRotatableRule);          // bool
                            br.Write(rule.IsSubtractive);                   // bool

                            // Saving shape
                            br.Write(rule.Width);                           // int
                            br.Write(rule.Height);                          // int
                            for (int i = 0; i < rule.Width; i++)
                                for (int j = 0; j < rule.Height; j++)
                                    br.Write(rule.Shape[i, j]);             // bool
                        }
                    }
                }
            }
        }
        public static Puzzle LoadPanelFromFile(string fileName)
        {
            if (isolatedStorage.FileExists(fileName))
            {
                using (IsolatedStorageFileStream fs = isolatedStorage.OpenFile(fileName, FileMode.Open))
                {
                    if (fs != null)
                    {
                        using (BinaryReader br = new BinaryReader(fs))
                        {
                            Puzzle panel;

                            // Panel's seed
                            int seed = br.ReadInt32();

                            // Panel size
                            int width = br.ReadInt32();
                            int height = br.ReadInt32();

                            // Is panel symmetric and panel colors
                            bool isSymmetric = br.ReadBoolean();
                            bool Y_mirrored = false;
                            Color mainColor, mirrorColor = Color.Black, backgroundColor, wallColor, buttonsColor;
                            if(isSymmetric)
                            {
                                Y_mirrored = br.ReadBoolean();
                                mainColor = new Color(br.ReadUInt32());
                                mirrorColor = new Color(br.ReadUInt32());
                            }
                            else
                            {
                                mainColor = new Color(br.ReadUInt32());
                            }

                            backgroundColor = new Color(br.ReadUInt32());
                            wallColor = new Color(br.ReadUInt32());
                            buttonsColor = new Color(br.ReadUInt32());

                            // Creating panel
                            if (isSymmetric)
                                panel = new SymmetryPuzzle(width, height, Y_mirrored, mainColor, mirrorColor, backgroundColor, wallColor, buttonsColor, seed);
                            else
                                panel = new Puzzle(width, height, mainColor, backgroundColor, wallColor, buttonsColor, seed);
                            
                            // Start nodes
                            int startNodesCount = br.ReadInt32();
                            for (int i = 0; i < startNodesCount; i++)
                                panel.Nodes[br.ReadInt32()].SetState(NodeState.Start);

                            // End nodes
                            int endNodesCount = br.ReadInt32();
                            for (int i = 0; i < endNodesCount; i++)
                                panel.Nodes[br.ReadInt32()].SetState(NodeState.Exit);

                            // Marked nodes
                            int markedNodesCount = br.ReadInt32();
                            for (int i = 0; i < markedNodesCount; i++)
                            {
                                int nodeID = br.ReadInt32();
                                if (br.ReadBoolean())
                                    panel.Nodes[nodeID].SetStateAndColor(NodeState.Marked, new Color(br.ReadUInt32()));
                                else
                                    panel.Nodes[nodeID].SetState(NodeState.Marked);
                            }

                            // Broken edges
                            int brokenEdgesCount = br.ReadInt32();
                            for (int i = 0; i < brokenEdgesCount; i++)
                            {
                                int edgeID = br.ReadInt32();
                                panel.Edges.Find(x => x.Id == edgeID).SetState(EdgeState.Broken);
                            }

                            // Marked edges
                            int markedEdgesCount = br.ReadInt32();
                            for (int i = 0; i < markedEdgesCount; i++)
                            {
                                int edgeID = br.ReadInt32();
                                if (br.ReadBoolean())
                                    panel.Edges.Find(x => x.Id == edgeID).SetStateAndColor(EdgeState.Marked, new Color(br.ReadUInt32()));
                                else
                                    panel.Edges.Find(x => x.Id == edgeID).SetState(EdgeState.Marked);
                            }

                            List<Block> blocks = panel.Blocks.ToList();

                            // Colored square rules
                            int coloredSquaresCount = br.ReadInt32();
                            for (int i = 0; i < coloredSquaresCount; i++)
                            {
                                int blockID = br.ReadInt32();
                                Color col = new Color(br.ReadUInt32());
                                blocks.Find(x => x.Id == blockID).Rule = new ColoredSquareRule(col);
                            }

                            // Sun rules
                            int sunsCount = br.ReadInt32();
                            for (int i = 0; i < sunsCount; i++)
                            {
                                int blockID = br.ReadInt32();
                                Color col = new Color(br.ReadUInt32());
                                blocks.Find(x => x.Id == blockID).Rule = new SunPairRule(col);
                            }

                            // Triangle rules
                            int trianglesCount = br.ReadInt32();
                            for (int i = 0; i < trianglesCount; i++)
                            {
                                int blockID = br.ReadInt32();
                                int power = br.ReadInt32();
                                blocks.Find(x => x.Id == blockID).Rule = new TriangleRule(power);
                            }

                            // Elimination rules
                            int eliminatorsCount = br.ReadInt32();
                            for (int i = 0; i < eliminatorsCount; i++)
                            {
                                int blockID = br.ReadInt32();
                                Color? col = null;
                                if (br.ReadBoolean())
                                    col = new Color(br.ReadUInt32());

                                blocks.Find(x => x.Id == blockID).Rule = new EliminationRule(col);
                            }

                            // Tetris rules
                            int tetrisCount = br.ReadInt32();
                            for (int i = 0; i < tetrisCount; i++)
                            {
                                int blockID = br.ReadInt32();
                                Color? col = null;
                                if (br.ReadBoolean())
                                    col = new Color(br.ReadUInt32());

                                bool rotatable = br.ReadBoolean();
                                bool subtractive = br.ReadBoolean();

                                // Read shape
                                int shapeWidth = br.ReadInt32();
                                int shapeHeight = br.ReadInt32();
                                int shapeSize = shapeWidth * shapeHeight;
                                bool[,] shape = new bool[shapeWidth, shapeHeight];
                                for (int k = 0; k < shapeSize; k++)
                                {
                                    int x = k / shapeHeight;
                                    int y = k - x * shapeHeight;
                                    shape[x, y] = br.ReadBoolean();
                                }

                                if (rotatable)
                                    blocks.Find(x => x.Id == blockID).Rule = new TetrisRotatableRule(shape, subtractive, col);
                                else
                                    blocks.Find(x => x.Id == blockID).Rule = new TetrisRule(shape, subtractive, col);
                            }

                            return panel;
                        }
                    }
                }
            }

            return null;
        }
        private static void DeletePanel(string fileName)
        {
            if (isolatedStorage.FileExists(fileName))
                isolatedStorage.DeleteFile(fileName);
        }
    }
}
