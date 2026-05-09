using UnityEngine;

namespace FPSGame.Input
{
    public struct ActorInputState
    {
        public Vector2 Move;
        public Vector2 LookDelta;
        public bool JumpPressed;
        public bool CrouchHeld;
        public bool RunHeld;
        public bool FireHeld;
        public bool FirePressed;
        public bool ReloadPressed;
        public bool InteractHeld;
        public bool InteractPressed;
    }
}
