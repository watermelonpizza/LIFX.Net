using System;
using System.Reflection;
using System.Linq;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Windows.UI;

namespace LIFX_Net
{
    public class LifxColour
    {
        [XmlAttribute()]
        public UInt16 Hue { get; set; }
        [XmlAttribute()]
        public UInt16 Saturation { get; set; }
        [XmlAttribute()]
        public UInt16 Luminosity { get; set; }
        [XmlAttribute()]
        public UInt16 Kelvin { get; set; }

        [XmlIgnore()]
        /// <summary>
        /// Returns a list of colours and their respective names based on the Windows.UI.Colors namespace.
        /// </summary>
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

        [XmlIgnore()]
        /// <summary>
        /// Returns a dictionary of the ColourName/Color key value pair based on the Windows.UI.Colors namespace
        /// </summary>
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

        /// <summary>
        /// Get an HSL based colour via the passed in colour name of the Windows.UI.Colors namespace
        /// </summary>
        /// <param name="colourName">The name of the colour</param>
        /// <returns>The HSL based colour</returns>
        public static LifxColour ColorToLifxColour(string colourName)
        {
            try
            {
                return ColorToLifxColour(ColourList[colourName.ToUpper()]);
            }
            catch (KeyNotFoundException e)
            {
                throw new ColorNotFoundException("Colour not found, invalid key", e);
            }
        }

        /// <summary>
        /// Converts an RGB based colour to a HSL colour space class
        /// </summary>
        /// <param name="colour">The RGB colour</param>
        /// <returns>An HSL colour</returns>
        public static LifxColour ColorToLifxColour(Color colour)
        {
            decimal hue = 0M, saturation = 0M, luminocity = 0M;

            decimal red = (decimal)colour.R / (decimal)byte.MaxValue;
            decimal green = (decimal)colour.G / (decimal)byte.MaxValue;
            decimal blue = (decimal)colour.B / (decimal)byte.MaxValue;

            decimal max = Math.Max(Math.Max(red, green), blue);
            decimal min = Math.Min(Math.Min(red, green), blue);
            decimal chroma = max - min;

            luminocity = (max + min) / 2M;

            if (chroma != 0)
            {
                hue = GetHue(red, green, blue, max, chroma) / 360M;
                saturation = GetSaturation(luminocity, chroma);
            }

            return new LifxColour()
            {
                Hue = (ushort)Math.Round(hue * ushort.MaxValue),
                Saturation = (ushort)Math.Round(saturation * ushort.MaxValue),
                Luminosity = (ushort)Math.Round(luminocity * ushort.MaxValue),
                Kelvin = 0
            };
        }

        private static decimal GetHue(decimal red, decimal green, decimal blue, decimal max, decimal chroma)
        {
            decimal hue = 0M;

            if (red == max)
                hue = ((green - blue) / chroma);
            else if (green == max)
                hue = ((blue - red) / chroma) + 2M;
            else
                hue = ((red - green) / chroma) + 4M;

            return 60M * ((hue + 6M) % 6M);
        }

        private static decimal GetSaturation(decimal luminosity, decimal chroma)
        {
            decimal saturation = 0M;

            if (luminosity < 0.5M)
                saturation = chroma / (1 * 2M);
            else
                saturation = chroma / (2M - 2M * luminosity);

            return saturation;
        }

        public static Color LifxColourToColor(LifxColour colour)
        {
            decimal hue = (decimal)colour.Hue / (decimal)ushort.MaxValue;
            decimal saturation = (decimal)colour.Saturation / (decimal)ushort.MaxValue;
            decimal luminosity = (decimal)colour.Luminosity / (decimal)ushort.MaxValue;
            decimal kelvin = (colour.Kelvin + 1500M) / 100M;
            
            decimal red = 0M, green = 0M, blue = 0M;

            if (saturation == 0M)
            {
                if (kelvin > 0M)
                    KelvinToRGB(kelvin, out red, out green, out blue);
                else
                    red = green = blue = luminosity;
            }
            else
            {
                GetRGBFromHSLWithChroma(hue, saturation, luminosity, out red, out green, out blue);
            }

            return new Color
            {
                A = 255,
                R = (byte)Math.Round(red * byte.MaxValue),
                G = (byte)Math.Round(green * byte.MaxValue),
                B = (byte)Math.Round(blue * byte.MaxValue)
            };
        }

        private static void GetRGBFromHSLWithChroma(decimal hue, decimal saturation, decimal luminosity, out decimal red, out decimal green, out decimal blue)
        {
            decimal max = 0M, min = 0M;

            max = luminosity < 0.5M ? luminosity * (1 + saturation) : (luminosity + saturation) - (luminosity * saturation);
            min = (luminosity * 2M) - max;

            red = ComponentFromHue(min, max, hue+(1M / 3M));
            green = ComponentFromHue(min, max, hue);
            blue = ComponentFromHue(min, max, hue - (1M / 3M));
        }

        private static decimal ComponentFromHue(decimal min, decimal max, decimal hue)
        {
            hue = (hue + 1M) % 1M;

            if ((hue * 6M) < 1)
                return min + (max - min) * 6M * hue;
            else if ((hue * 2M) < 1)
                return max;
            else if ((hue * 3M) < 2)
                return min + (max - min) * ((2M / 3M) - hue) * 6M;
            else
                return min;
        }

        private static void KelvinToRGB(decimal kelvin, out decimal red, out decimal green, out decimal blue)
        {
            float r, g, b;
            KelvinToRGB(Convert.ToSingle(kelvin), out r, out g, out b);

            red = Convert.ToDecimal(r);
            green = Convert.ToDecimal(g);
            blue = Convert.ToDecimal(b);
        }

        private static void KelvinToRGB(float kelvin, out float red, out float green, out float blue)
        {
            if (kelvin <= 66)
            {
                red = 255;
            }
            else
            {
                red = kelvin - 60;
                red = (float)(329.698727446 * Math.Pow(red, -0.1332047592));
            }

            if (kelvin <= 66)
            {
                green = kelvin;
                green = (float)(99.4708025861 * Math.Log(green) - 161.1195681661);
            }
            else
            {
                green = kelvin - 60;
                green = (float)(288.1221695283 * Math.Pow(green, -0.0755148492));
            }

            if (kelvin >= 66)
                blue = 255;
            else
            {
                if (kelvin <= 19)
                    blue = 0;
                else
                {
                    blue = kelvin - 10;
                    blue = (float)(138.5177312231 * Math.Log(blue) - 305.0447927307);
                }
            }

            if (red < 0) red = 0;
            if (red > 255) red = 255;

            if (blue < 0) blue = 0;
            if (blue > 255) blue = 255;

            if (green < 0) green = 0;
            if (green > 255) green = 255;

            red /= byte.MaxValue;
            green /= byte.MaxValue;
            blue /= byte.MaxValue;
        }
    }

    public class RGBColour
    {
        public string Name { get; set; }
        public string FriendlyName
        {
            get
            {
                var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

                return r.Replace(Name, " ");
            }
        }
        public Color Colour { get; set; }
    }
}