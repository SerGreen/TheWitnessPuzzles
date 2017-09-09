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
                             new Color(197, 207, 232), new Color(246, 254, 36), new Color(252, 36, 52))
            //new ColorPalette(new Color(), new Color(), new Color(),
            //                 new Color(), new Color(), new Color())
        };
    }
}
