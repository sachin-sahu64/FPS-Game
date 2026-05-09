using System;
using System.Collections.Generic;
using FPSGame.Combat;
using FPSGame.Economy;
using FPSGame.Objectives;
using UnityEngine;

namespace FPSGame.Rounds
{
    public enum RoundState
    {
        Warmup,
        FreezeTime,
        Live,
        BombPlanted,
        RoundEnd
    }

    public class RoundManager : MonoBehaviour
    {
        [SerializeField] private RoundSettings settings;
        [SerializeField] private EconomyTable economyTable;
        [SerializeField] private bool startOnAwake = true;

        private readonly List<TeamMember> roster = new();
        private readonly HashSet<Health> subscribedHealths = new();
        private float stateEndsAt;
        private int roundsPlayed;

        public event Action<RoundState, float> StateChanged;

        public RoundState CurrentState { get; private set; } = RoundState.Warmup;

        public BombObjective ActiveBomb { get; private set; }

        public IReadOnlyList<TeamMember> Roster => roster;

        public bool IsRoundActive => CurrentState == RoundState.Live || CurrentState == RoundState.BombPlanted;

        public float TimeRemaining => Mathf.Max(0f, stateEndsAt - Time.time);

        private void Awake()
        {
            RefreshRoster();
            SubscribeToRoster();
        }

        private void Start()
        {
            if (startOnAwake)
            {
                StartMatch();
            }
        }

        private void Update()
        {
            if (settings == null)
            {
                return;
            }

            switch (CurrentState)
            {
                case RoundState.FreezeTime when Time.time >= stateEndsAt:
                    BeginLiveRound();
                    break;
                case RoundState.Live when Time.time >= stateEndsAt:
                    EndRound(TeamSide.CounterTerrorists);
                    break;
                case RoundState.RoundEnd when Time.time >= stateEndsAt && roundsPlayed < settings.MaxRounds:
                    BeginFreezeTime();
                    break;
            }
        }

        public void StartMatch()
        {
            roundsPlayed = 0;
            RefreshRoster();
            SubscribeToRoster();

            foreach (TeamMember member in roster)
            {
                member.Health?.ResetState();
                member.Economy?.ResetCash(settings != null ? settings.StartingCash : 800);
            }

            BeginFreezeTime();
        }

        public void OnBombPlanted(BombObjective bomb)
        {
            ActiveBomb = bomb;
            CurrentState = RoundState.BombPlanted;
            stateEndsAt = Time.time + (settings != null ? settings.BombTime : 40f);
            StateChanged?.Invoke(CurrentState, TimeRemaining);
            GrantReward(TeamSide.Terrorists, EconomyRewardType.BombPlantedT);
        }

        public void OnBombDefused(BombObjective bomb)
        {
            EndRound(TeamSide.CounterTerrorists);
        }

        public void OnBombExploded(BombObjective bomb)
        {
            EndRound(TeamSide.Terrorists);
        }

        public void RegisterKill(TeamMember killer)
        {
            if (killer?.Economy != null)
            {
                killer.Economy.AddCash(economyTable != null ? economyTable.GetReward(EconomyRewardType.EnemyEliminated) : 300);
            }
        }

        public void RegisterTeamMember(TeamMember member)
        {
            if (member == null || roster.Contains(member))
            {
                return;
            }

            roster.Add(member);
            SubscribeMember(member);
        }

        private void BeginFreezeTime()
        {
            CurrentState = RoundState.FreezeTime;
            stateEndsAt = Time.time + (settings != null ? settings.FreezeTime : 15f);
            
            if (ActiveBomb == null)
            {
                ActiveBomb = FindFirstObjectByType<BombObjective>();
            }

            if (ActiveBomb != null)
            {
                ActiveBomb.ResetBomb();
                AssignBombToRandomTerrorist();
            }

            StateChanged?.Invoke(CurrentState, TimeRemaining);
        }

        private void AssignBombToRandomTerrorist()
        {
            if (ActiveBomb == null) return;

            List<TeamMember> terrorists = new();
            foreach (TeamMember member in roster)
            {
                if (member != null && member.Side == TeamSide.Terrorists && member.Health != null && member.Health.IsAlive)
                {
                    terrorists.Add(member);
                }
            }

            if (terrorists.Count > 0)
            {
                TeamMember luckyT = terrorists[UnityEngine.Random.Range(0, terrorists.Count)];
                ActiveBomb.SetCarried(luckyT);
            }
        }

        private void BeginLiveRound()
        {
            CurrentState = RoundState.Live;
            stateEndsAt = Time.time + (settings != null ? settings.RoundTime : 115f);
            StateChanged?.Invoke(CurrentState, TimeRemaining);
        }

        private void EndRound(TeamSide winningSide)
        {
            if (CurrentState == RoundState.RoundEnd)
            {
                return;
            }

            CurrentState = RoundState.RoundEnd;
            stateEndsAt = Time.time + 6f;
            roundsPlayed++;
            ActiveBomb = null;
            StateChanged?.Invoke(CurrentState, TimeRemaining);
            GrantReward(winningSide, EconomyRewardType.RoundWin);
        }

        private void GrantReward(TeamSide rewardedSide, EconomyRewardType rewardType)
        {
            int reward = economyTable != null ? economyTable.GetReward(rewardType) : 0;

            foreach (TeamMember member in roster)
            {
                if (member.Side == rewardedSide)
                {
                    member.Economy?.AddCash(reward);
                }
            }
        }

        private void RefreshRoster()
        {
            roster.Clear();
            roster.AddRange(FindObjectsByType<TeamMember>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        }

        private void SubscribeToRoster()
        {
            foreach (TeamMember member in roster)
            {
                SubscribeMember(member);
            }
        }

        private void SubscribeMember(TeamMember member)
        {
            if (member?.Health != null && subscribedHealths.Add(member.Health))
            {
                member.Health.Died += HandleMemberDied;
            }
        }

        private void HandleMemberDied(Health health, DamageInfo damageInfo)
        {
            if (health == null || !health.TryGetComponent(out TeamMember victim))
            {
                return;
            }

            HandleDeath(victim, damageInfo);
        }

        private void HandleDeath(TeamMember victim, DamageInfo damageInfo)
        {
            if (damageInfo.Instigator != null && damageInfo.Instigator.TryGetComponent(out TeamMember killer))
            {
                RegisterKill(killer);
            }

            if (!IsRoundActive)
            {
                return;
            }

            bool terroristsAlive = false;
            bool counterTerroristsAlive = false;

            foreach (TeamMember member in roster)
            {
                if (member.Health == null || !member.Health.IsAlive)
                {
                    continue;
                }

                if (member.Side == TeamSide.Terrorists)
                {
                    terroristsAlive = true;
                }
                else
                {
                    counterTerroristsAlive = true;
                }
            }

            if (!terroristsAlive)
            {
                EndRound(TeamSide.CounterTerrorists);
            }
            else if (!counterTerroristsAlive)
            {
                EndRound(TeamSide.Terrorists);
            }
        }
    }
}
