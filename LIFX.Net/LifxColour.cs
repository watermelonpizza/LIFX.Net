using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Windows.UI;

namespace LIFX_Net
{
    public class LifxColour
    {
        public UInt16 Hue { get; set; }
        public UInt16 Saturation { get; set; }
        public UInt16 Luminosity { get; set; }
        public UInt16 Kelvin { get; set; }


        public static List<RGBColour> RGBColourList
        {
            get
            {
                var _Colours = typeof(Colors).GetRuntimeProperties().Select(c => new RGBColour()
                {
                    Colour = (Color)c.GetValue(null),
                    Name = c.Name
                });

                return _Colours.ToList();
            }
        }


        public static Dictionary<string, Color> ColourList
        {
            get
            {
                var _Colours = typeof(Colors).GetRuntimeProperties().Select(c => new
                {
                    Color = (Color)c.GetValue(null),
                    Name = c.Name.ToUpper()
                });

                return _Colours.ToDictionary(x => x.Name, x => x.Color);
            }
        }

        public LifxColour()
        {
            Hue = 0;
            Saturation = 0;
            Luminosity = 0;
            Kelvin = 0;
        }

        public static LifxColour FromRgbColor(string colourName)
        {
            try
            {
                return FromRgbColor(ColourList[colourName.ToUpper()]);
            }
            catch (KeyNotFoundException)
            {
                throw new ColorNotFoundException();
            }
        }

        public static LifxColour FromRgbColor(Color colour)
        {
            float r = colour.R / 255f, g = colour.G / 255f, b = colour.B / 255f;

            float max = Math.Max(Math.Max(r, b), g);
            float min = Math.Min(Math.Min(r, b), g);

            float h, s, l = (max + min) / 2;

            if (max == min)
            {
                h = s = 0;
            }
            else
            {
                float d = max - min;
                s = l < 0.5 ? d / (2 - max - min) : d / (max + min);

                if (r == max)
                    h = (g - b) / d + (g < b ? 6 : 0);
                else if (g == max)
                    h = (b - r) / d + 2;
                else
                    h = (r - g) / d + 4;

                h = h / 6;
            }

            return new LifxColour()
            {
                Hue = Convert.ToUInt16(h * 65535),
                Saturation = Convert.ToUInt16(s * 65535),
                Luminosity = Convert.ToUInt16(l * 65535)
            };
        }
    }

    public class RGBColour
    {
        public string Name { get; set; }
        public Color Colour { get; set; }
    }
}




