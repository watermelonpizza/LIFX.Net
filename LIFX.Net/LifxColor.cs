using System;

namespace LIFX_Net
{
    public class LifxColor
    {
        public UInt16 Hue { get; set; }
        public UInt16 Saturation { get; set; }
        public UInt16 Luminosity { get; set; }
        public UInt16 Kelvin { get; set; }

        //public LifxColor(Color color, UInt16 kelvinValue)
        //{
        //    DotNetColor = color;
        //    mKelvin = kelvinValue;
        //}

        public LifxColor()
        {
            Hue = 0;
            Saturation = 0;
            Luminosity = 0;
            Kelvin = 0;
        }

        //public HSLColor HSLColor
        //{
        //    get
        //    {
        //        return new HSLColor((double)(Hue * 240 / 65535), (double)(Saturation * 240 / 65535), (double)(Luminosity * 240 / 65535));
        //    }
        //    set
        //    {
        //        Hue = (ushort)(value.Hue * 65535 / 240);
        //        Saturation = (ushort)(value.Saturation * 65535 / 240);
        //        Luminosity = (ushort)(value.Luminosity * 65535 / 240);
        //    }
        //}

        //public Color DotNetColor
        //{
        //    get 
        //    {
        //        return (Color)HSLColor;
        //    }
        //    set
        //    {
        //        HSLColor = (HSLColor)value;
        //    }
        //}
    }
}




