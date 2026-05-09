using FPSGame.Rounds;
using UnityEngine;

namespace FPSGame.Spawning
{
    public class TeamSpawnPoint : MonoBehaviour
    {
        [SerializeField] private TeamSide side;

        public TeamSide Side => side;
    }
}
