using UnityEngine;
using UnityEngine.InputSystem;

namespace FPSGame.Input
{
    public class KeyboardMouseInputSource : ActorInputSource
    {
        public override bool ShouldLockCursor => true;

        public override ActorInputState ReadState()
        {
            ActorInputState state = default;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed)
                {
                    state.Move.y += 1f;
                }

                if (Keyboard.current.sKey.isPressed)
                {
                    state.Move.y -= 1f;
                }

                if (Keyboard.current.aKey.isPressed)
                {
                    state.Move.x -= 1f;
                }

                if (Keyboard.current.dKey.isPressed)
                {
                    state.Move.x += 1f;
                }

                state.JumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
                state.CrouchHeld = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.cKey.isPressed;
                state.RunHeld = Keyboard.current.leftShiftKey.isPressed;
                state.ReloadPressed = Keyboard.current.rKey.wasPressedThisFrame;
                state.InteractHeld = Keyboard.current.eKey.isPressed;
                state.InteractPressed = Keyboard.current.eKey.wasPressedThisFrame;
            }

            if (Mouse.current != null)
            {
                state.LookDelta = Mouse.current.delta.ReadValue();
                state.FireHeld = Mouse.current.leftButton.isPressed;
                state.FirePressed = Mouse.current.leftButton.wasPressedThisFrame;
            }

            if (state.Move.sqrMagnitude > 1f)
            {
                state.Move.Normalize();
            }

            return state;
        }
    }
}
