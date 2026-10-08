using System;

namespace Game.Game.Common.Service;

public class ClockService : IClockService
{
    public DateTime Now => DateTime.Now;
}
