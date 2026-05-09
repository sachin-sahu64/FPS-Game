using System;
using UnityEngine;

namespace FPSGame.Economy
{
    public class PlayerEconomy : MonoBehaviour
    {
        [SerializeField] private int startingCash = 800;
        [SerializeField] private int maxCash = 16000;

        public event Action<int> CashChanged;

        public int CurrentCash { get; private set; }

        private void Awake()
        {
            ResetCash(startingCash);
        }

        public void ResetCash(int newValue)
        {
            CurrentCash = Mathf.Clamp(newValue, 0, maxCash);
            CashChanged?.Invoke(CurrentCash);
        }

        public void AddCash(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            CurrentCash = Mathf.Clamp(CurrentCash + amount, 0, maxCash);
            CashChanged?.Invoke(CurrentCash);
        }

        public bool SpendCash(int amount)
        {
            if (amount <= 0 || amount > CurrentCash)
            {
                return false;
            }

            CurrentCash -= amount;
            CashChanged?.Invoke(CurrentCash);
            return true;
        }
    }
}
