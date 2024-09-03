using System;

namespace Dan.App.Models
{
    [System.Serializable]
    public struct BlacklistUser
    {
        public string username;
        public string userGuid;
        
        public BlacklistUser(string username, string userGuid)
        {
            this.username = username;
            this.userGuid = userGuid;
        }
        
        public static bool operator==(BlacklistUser a, BlacklistUser b)
        {
            return a.username == b.username && a.userGuid == b.userGuid;
        }
        
        public static bool operator!=(BlacklistUser a, BlacklistUser b)
        {
            return a.username != b.username || a.userGuid != b.userGuid;
        }
        
        public bool Equals(BlacklistUser other)
        {
            return username == other.username && userGuid == other.userGuid;
        }

        public override bool Equals(object obj)
        {
            return obj is BlacklistUser other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(username, userGuid);
        }
    }
}