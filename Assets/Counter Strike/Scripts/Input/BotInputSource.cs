using UnityEngine;

namespace FPSGame.Input
{
    public class BotInputSource : ActorInputSource
    {
        private ActorInputState currentState;

        public override ActorInputState ReadState()
        {
            return currentState;
        }

        public void SetState(ActorInputState nextState)
        {
            currentState = nextState;
        }

        private void LateUpdate()
        {
            currentState.JumpPressed = false;
            currentState.FirePressed = false;
            currentState.ReloadPressed = false;
            currentState.LookDelta = Vector2.zero;
        }
    }
}
