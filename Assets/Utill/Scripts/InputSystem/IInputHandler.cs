using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IInputHandler
{
    int Priority { get; }
    bool HandleInput(InputAction action, InputAction.CallbackContext context);
}