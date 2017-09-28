using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared
{
    public static class ColorPalettesLibrary
    {
        public class ColorPalette
        {
            private const int paletteSize = 6;
            private Color[] palette;

            public ColorPalette(Color bgColor, Color wallColor, Color btnColor, Color lineColor, Color mainLineColor, Color mirLineColor)
                    => palette = new[] { bgColor, wallColor, btnColor, lineColor, mainLineColor, mirLineColor };

            public Color BackgroundColor { get => palette[0]; }
            public Color WallsColor { get => palette[1]; }
            public Color ButtonsColor { get => palette[2]; }
            public Color SingleLineColor { get => palette[3]; }
            public Color MainLineColor { get => palette[4]; }
            public Color MirrorLineColor { get => palette[5]; }
        }

        public static int Size { get => Palettes.Length; }

        public static readonly ColorPalette[] Palettes =
        {
            // Black & White
            new ColorPalette(new Color(116, 116, 116), new Color(29, 29, 29), new Color(116, 116, 116),
                             new Color(255, 255, 255), new Color(0, 255, 255), new Color(255, 238, 0)),
            // Red
            new ColorPalette(new Color(255, 122, 115), new Color(113, 48, 45), new Color(204, 74, 67),
                             new Color(255, 255, 255), new Color(255, 26, 14), new Color(14, 255, 222)),
            // Violet
            new ColorPalette(new Color(148, 80, 170), new Color(64, 32, 75), new Color(148, 80, 170),
                             new Color(245, 215, 255), new Color(255, 229, 14), new Color(175, 255, 14)),
            // Pink
            new ColorPalette(new Color(252, 155, 191), new Color(155, 90, 114), new Color(252, 155, 191),
                             new Color(255, 249, 251), new Color(255, 60, 132), new Color(88, 92, 255)),
            // Black fuchsia
            new ColorPalette(new Color(116, 116, 116), new Color(29, 29, 29), new Color(199, 8, 79),
                             new Color(236, 0, 87), new Color(236, 0, 87), new Color(255, 242, 0)),
            // Chocolate
            new ColorPalette(new Color(255, 202, 176), new Color(113, 67, 45), new Color(204, 111, 67),
                             new Color(255, 92, 14), new Color(255, 92, 14), new Color(28, 166, 255)),
            // Yellow
            new ColorPalette(new Color(232, 219, 113), new Color(94, 88, 39), new Color(226, 209, 62),
                             new Color(1, 178, 254), new Color(1, 178, 254), new Color(255, 0, 77)),
            // Swamp green
            new ColorPalette(new Color(140, 204, 99), new Color(52, 82, 35), new Color(112, 199, 55),
                             new Color(240, 255, 0), new Color(240, 255, 0), new Color(255, 60, 0)),
            // Blue
            new ColorPalette(new Color(51, 84, 159), new Color(33, 42, 65), new Color(84, 106, 156),
                             new Color(197, 207, 232), new Color(246, 254, 36), new Color(252, 36, 52)),
            // Others
            new ColorPalette(new Color(57,49,36), new Color(126,113,100), new Color(57,49,36),
                             new Color(63,223,150), new Color(63,223,150), new Color(232, 120, 64)),
            new ColorPalette(new Color(44,103,62), new Color(7,41,19), new Color(44,103,62),
                             new Color(153,233,15), new Color(153,233,15), new Color(16, 232, 232)),
            new ColorPalette(new Color(62,41,183), new Color(21,13,54), new Color(62,41,183),
                             new Color(237, 134, 82), new Color(237, 134, 82), new Color(237, 83, 152)),
            new ColorPalette(new Color(34,47,70), new Color(0,133,125), new Color(34,47,70),
                             new Color(255,255,255), new Color(255, 221, 0), new Color(255, 0, 187)),
            new ColorPalette(new Color(21,28,52), new Color(59,68,86), new Color(21,28,52),
                             new Color(0, 45, 209), new Color(0, 45, 209), new Color(173, 0, 135)),
            new ColorPalette(new Color(124,72,72), new Color(50,10,24), new Color(124,72,72),
                             new Color(249,106,0), new Color(249,106,0), new Color(247, 247, 0)),
            new ColorPalette(new Color(140,99,74), new Color(39,23,5), new Color(140,99,74),
                             new Color(160,0,32), new Color(160,0,32), new Color(0, 109, 145)),
            new ColorPalette(new Color(42,43,44), new Color(64,65,66), new Color(42,43,44),
                             new Color(255,255,255), new Color(255,255,0), new Color(0, 255, 255)),
            new ColorPalette(new Color(85,90,93), new Color(35,40,43), new Color(85,90,93),
                             new Color(245,209,0), new Color(245,209,0), new Color(255, 38, 0)),
            new ColorPalette(new Color(92,72,52), new Color(34,19,19), new Color(92,72,52),
                             new Color(255,151,0), new Color(255,151,0), new Color(255, 0, 157)),
            new ColorPalette(new Color(96,92,191), new Color(45,45,89), new Color(96,92,191),
                             new Color(117,249,0), new Color(117,249,0), new Color(2, 247, 247)),
            new ColorPalette(new Color(76,111,44), new Color(22,33,4), new Color(76,111,44),
                             new Color(255,255,0), new Color(255,255,0), new Color(0,255,0)),
            new ColorPalette(new Color(139,124,99), new Color(64,52,42), new Color(139,124,99),
                             new Color(198,33,0), new Color(198,33,0), new Color(250, 250, 0)),
            new ColorPalette(new Color(219,201,143), new Color(76,73,65), new Color(219,201,143),
                             new Color(178,4,0), new Color(178,4,0), new Color(152, 0, 175)),
            new ColorPalette(new Color(61,92,97), new Color(8,33,41), new Color(61,92,97),
                             new Color(0,235,235), new Color(0,235,235), new Color(218, 73, 32)),
            new ColorPalette(new Color(72,79,76), new Color(1,191,71), new Color(72,79,76),
                             new Color(255, 255, 0), new Color(255, 255, 0), new Color(200, 39, 209)),
            new ColorPalette(new Color(68,69,74), new Color(44,14,54), new Color(68,69,74),
                             new Color(190,80,193), new Color(190,80,193), new Color(81, 118, 193)),
            new ColorPalette(new Color(42,38,27), new Color(140,73,13), new Color(42,38,27),
                             new Color(209,156,0), new Color(209,156,0), new Color(226, 0, 0)),
            new ColorPalette(new Color(35,35,35), new Color(97,98,0), new Color(35,35,35),
                             new Color(212,171,0), new Color(212,171,0), new Color(216, 0, 79)),
            new ColorPalette(new Color(255,196,8), new Color(173,118,76), new Color(255,196,8),
                             new Color(255,126,10), new Color(255,126,10), new Color(255, 55, 0)),
            new ColorPalette(new Color(52,93,212), new Color(10,46,82), new Color(52,93,212),
                             new Color(255,255,255), new Color(0, 255, 200), new Color(255, 255, 0)),
            new ColorPalette(new Color(251,215,106), new Color(60,60,60), new Color(250,250,250),
                             new Color(177,255,63), new Color(177,255,63), new Color(255, 65, 48)),
            new ColorPalette(new Color(66,96,91), new Color(21,23,26), new Color(66,96,91),
                             new Color(255,255,255), new Color(255, 251, 38), new Color(255, 38, 222)),
            new ColorPalette(new Color(106,147,136), new Color(40,40,39), new Color(106,147,136),
                             new Color(255,255,255), new Color(104, 255, 24), new Color(24, 220, 255)),
            new ColorPalette(new Color(0,148,132), new Color(43,55,77), new Color(0,148,132),
                             new Color(255,255,255), new Color(228, 255, 25), new Color(255, 40, 24))
        };
    }
}
