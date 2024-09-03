using Dan.Main;
using UnityEngine;

namespace Dan.Models
{
    [System.Serializable]
    public struct Entry
    {
        public string Username => username;
        public long Score => score;
        public ulong Date => date;
        public string Extra => extra;
        public int Rank => rank;
        public string UserGuid => userGuid;
        
        public string username;
        public long score;
        public ulong date;
        public string extra;
        public int rank;
        public string userGuid;
        
        /// <summary>
        /// Returns whether the entry is the current user's entry.
        /// </summary>
        public bool IsMine() => UserGuid == LeaderboardCreator.UserGuid;

        /// <summary>
        /// Returns the rank of the entry with its suffix.
        /// </summary>
        /// <returns>Rank + suffix (e.g. 1st, 2nd, 3rd, 4th, 5th, etc.).</returns>
        public string RankSuffix()
        {
            var lastDigit = rank % 10;
            var lastTwoDigits = rank % 100;

            var suffix = lastDigit == 1 && lastTwoDigits != 11 ? "st" :
                lastDigit == 2 && lastTwoDigits != 12 ? "nd" :
                lastDigit == 3 && lastTwoDigits != 13 ? "rd" : "th";

            return $"{rank}{suffix}";
        }

        public float GetScoreFloat(int decimals = 2) => score / Mathf.Pow(10, decimals);
        
        public double GetScoreDouble(int decimals = 2) => score / Mathf.Pow(10, decimals);
        
        public decimal GetScoreDecimal(int decimals = 2) => score / (decimal)Mathf.Pow(10, decimals);
    }
}