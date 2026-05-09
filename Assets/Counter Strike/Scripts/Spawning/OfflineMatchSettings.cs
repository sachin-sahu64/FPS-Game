using FPSGame.Rounds;
using UnityEngine;

namespace FPSGame.Spawning
{
    [CreateAssetMenu(menuName = "FPS Game/Spawning/Offline Match Settings", fileName = "OfflineMatchSettings")]
    public class OfflineMatchSettings : ScriptableObject
    {
        [SerializeField] [Range(1, 32)] private int playersPerSide = 32;
        [SerializeField] private bool includeLocalPlayer = true;
        [SerializeField] private TeamSide localPlayerSide = TeamSide.CounterTerrorists;
        [SerializeField] [Min(0f)] private float spawnScatterRadius = 1.5f;

        public int PlayersPerSide => playersPerSide;

        public bool IncludeLocalPlayer => includeLocalPlayer;

        public TeamSide LocalPlayerSide => localPlayerSide;

        public float SpawnScatterRadius => spawnScatterRadius;
    }
}
