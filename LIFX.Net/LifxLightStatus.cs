using System;

namespace LIFX_Net
{
    public class LifxLightStatus
    {
        public LifxColor Colour { get; set; }
        public LifxPowerState PowerState { get; set; }
        public UInt16 DimState { get; set; }
        public String Label { get; set; }
        public UInt64 Tags { get; set; }

        public LifxLightStatus()
        {
            Colour = new LifxColor();
            PowerState = LifxPowerState.Off;
            DimState = 0;
            Label = "";
            Tags = 0;
        }
    }
}




