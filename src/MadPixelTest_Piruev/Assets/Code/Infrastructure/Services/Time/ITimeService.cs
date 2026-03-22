// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;

namespace Code.Infrastructure.Services.Time
{
  public interface ITimeService
  {
    /// <summary>
    /// Raw deltatime if not paused.
    /// </summary>
    float DeltaTime { get; }

    /// <summary>
    /// Deltatime scaled to 60 FPS.
    /// </summary>
    float DeltaAt60FPS { get; }

    /// <summary>
    /// Deltatime scaled to 100 FPS.
    /// </summary>
    float DeltaAt100FPS { get; }

    /// <summary>
    /// Late delta.
    /// </summary>
    float DeltaAtOffset { get; }

    float UnscaledDeltaTime { get; }

    DateTime UtcNow { get; }

    public void StopTime();
    public void StartTime();
  }
}
