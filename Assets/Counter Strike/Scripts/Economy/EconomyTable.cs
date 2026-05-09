using System;
using System.Collections.Generic;
using UnityEngine;

namespace FPSGame.Economy
{
    [Serializable]
    public struct EconomyRewardEntry
    {
        public EconomyRewardType rewardType;
        public int cashAmount;
    }

    [CreateAssetMenu(menuName = "FPS Game/Economy/Economy Table", fileName = "EconomyTable")]
    public class EconomyTable : ScriptableObject
    {
        [SerializeField] private EconomyRewardEntry[] rewards =
        {
            new() { rewardType = EconomyRewardType.EnemyEliminated, cashAmount = 300 },
            new() { rewardType = EconomyRewardType.BombPlantedT, cashAmount = 800 },
            new() { rewardType = EconomyRewardType.RoundWin, cashAmount = 3250 },
            new() { rewardType = EconomyRewardType.DefuseKitPurchase, cashAmount = 400 }
        };

        public int GetReward(EconomyRewardType rewardType)
        {
            for (int index = 0; index < rewards.Length; index++)
            {
                if (rewards[index].rewardType == rewardType)
                {
                    return rewards[index].cashAmount;
                }
            }

            return 0;
        }

        public IReadOnlyList<EconomyRewardEntry> Rewards => rewards;
    }
}
