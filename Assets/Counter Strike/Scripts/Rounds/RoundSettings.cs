using UnityEngine;

namespace FPSGame.Rounds
{
    [CreateAssetMenu(menuName = "FPS Game/Rounds/Round Settings", fileName = "RoundSettings")]
    public class RoundSettings : ScriptableObject
    {
        [SerializeField] private float freezeTime = 15f;
        [SerializeField] private float roundTime = 115f;
        [SerializeField] private float bombTime = 40f;
        [SerializeField] private float buyTime = 20f;
        [SerializeField] private int maxRounds = 24;
        [SerializeField] private int startingCash = 800;

        public float FreezeTime => freezeTime;
        public float RoundTime => roundTime;
        public float BombTime => bombTime;
        public float BuyTime => buyTime;
        public int MaxRounds => maxRounds;
        public int StartingCash => startingCash;
    }
}
