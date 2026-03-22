// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;

namespace Code.Infrastructure.Services.Time
{
  public class UnityTimeService : ITimeService
  {
    private bool _paused;

    public float DeltaTime => !_paused ? UnityEngine.Time.deltaTime : 0;

    public float DeltaAt60FPS => !_paused ? UnityEngine.Time.deltaTime / 0.016f : 0;

    public float DeltaAt100FPS => !_paused ? UnityEngine.Time.deltaTime / 0.01f : 0;

    public float DeltaAtOffset => !_paused ? UnityEngine.Time.deltaTime + 0.01f : 0;

    public DateTime UtcNow => DateTime.UtcNow;

    public float UnscaledDeltaTime => !_paused ? UnityEngine.Time.unscaledDeltaTime : 0;


    public void StopTime() => _paused = true;

    public void StartTime() => _paused = false;
  }
}
