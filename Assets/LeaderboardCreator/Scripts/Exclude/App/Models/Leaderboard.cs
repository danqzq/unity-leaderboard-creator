using Dan.Legacy;

namespace Dan.App.Models
{
    [System.Serializable]
    public class Leaderboard
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public bool UniqueUsernames { get; set; }
        public bool IsInAscendingOrder { get; set; }
        public bool IsProfanityEnabled { get; set; }
        public bool IsSuperLeaderboard { get; set; }
        public BlacklistUser[] BlacklistUsers { get; set; }
    }
}