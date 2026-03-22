// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.Services.Time;

using UnityEngine;

using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;


namespace Code.Gameplay.Utils
{
  public class AutoRotate : ZenjexBehaviour
  {
    // Rotation speed & axis
    public Vector3 rotation;

    // Rotation space
    public Space space = Space.Self;

    [Zenjex] private ITimeService _timeService;

    void Update() => this.transform.Rotate(rotation * _timeService.DeltaAt60FPS, space);
  }
}
