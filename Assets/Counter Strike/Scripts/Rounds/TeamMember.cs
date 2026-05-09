using FPSGame.Combat;
using FPSGame.Economy;
using UnityEngine;

namespace FPSGame.Rounds
{
    public class TeamMember : MonoBehaviour
    {
        [SerializeField] private TeamSide side;
        [SerializeField] private bool isBot;
        [SerializeField] private Health health;
        [SerializeField] private PlayerEconomy economy;

        public TeamSide Side => side;

        public bool IsBot => isBot;

        public Health Health => health;

        public PlayerEconomy Economy => economy;

        public void Configure(TeamSide newSide, bool botControlled)
        {
            side = newSide;
            isBot = botControlled;
        }

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            if (economy == null)
            {
                economy = GetComponent<PlayerEconomy>();
            }

            if (GetComponent<DamageFlashFeedback>() == null)
            {
                gameObject.AddComponent<DamageFlashFeedback>();
            }

            if (GetComponent<WorldHealthBar>() == null)
            {
                gameObject.AddComponent<WorldHealthBar>();
            }
        }

        private void Reset()
        {
            health = GetComponent<Health>();
            economy = GetComponent<PlayerEconomy>();
        }
    }
}
