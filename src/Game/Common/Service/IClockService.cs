using System;

namespace Game.Game.Common.Service;

/// <summary>
/// Current time, behind an interface so time-dependent logic stays testable.
/// </summary>
public interface IClockService
{
    DateTime Now { get; }
}
