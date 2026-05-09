using UnityEngine;

namespace FPSGame.Input
{
    public abstract class ActorInputSource : MonoBehaviour
    {
        public virtual bool ShouldLockCursor => false;

        public abstract ActorInputState ReadState();
    }
}
