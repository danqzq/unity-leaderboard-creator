using System;
using Dan.Models;

namespace Dan.Main
{
    public class LeaderboardReference
    {
        public string Key { get; }

        public LeaderboardReference(string key) => Key = key;

        public void UploadNewEntry(string username, int score, Action<bool> callback = null, Action<string> errorCallback = null) => 
            LeaderboardCreator.UploadNewEntry(Key, username, score, callback, errorCallback);
        
        public void UploadNewEntry(string username, int score, string extraData, Action<bool> callback = null, Action<string> errorCallback = null) => 
            LeaderboardCreator.UploadNewEntry(Key, username, score, extraData, callback, errorCallback);

        public void GetEntries(Action<Entry[]> callback, Action<string> errorCallback = null) => 
            LeaderboardCreator.GetLeaderboard(Key, callback, errorCallback);
        
        public void GetEntries(bool isAscending, Action<Entry[]> callback, Action<string> errorCallback = null) => 
            LeaderboardCreator.GetLeaderboard(Key, isAscending, callback, errorCallback);
        
        public void GetEntries(LeaderboardSearchQuery query, Action<Entry[]> callback, Action<string> errorCallback = null) => 
            LeaderboardCreator.GetLeaderboard(Key, query, callback, errorCallback);
        
        public void GetEntries(bool isAscending, LeaderboardSearchQuery query, Action<Entry[]> callback, Action<string> errorCallback = null) =>
            LeaderboardCreator.GetLeaderboard(Key, isAscending, query, callback, errorCallback);
        
        public void GetPersonalEntry(Action<Entry> callback, Action<string> errorCallback = null) => 
            LeaderboardCreator.GetPersonalEntry(Key, callback, errorCallback);
        
        public void GetEntryCount(Action<int> callback, Action<string> errorCallback = null) => 
            LeaderboardCreator.GetEntryCount(Key, callback, errorCallback);
        
        public void DeleteEntry(Action<bool> callback = null, Action<string> errorCallback = null) => 
            LeaderboardCreator.DeleteEntry(Key, callback, errorCallback);
        
        public void ResetPlayer(Action onReset = null) => LeaderboardCreator.ResetPlayer(onReset);
    }
}