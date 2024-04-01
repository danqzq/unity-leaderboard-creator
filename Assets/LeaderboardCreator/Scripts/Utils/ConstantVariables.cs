using Dan.Enums;

namespace Dan
{
    internal static class ConstantVariables
    {
        internal const string GUID_KEY = "LEADERBOARD_CREATOR___LOCAL_GUID";
        
        internal static string GetServerURL(Routes route = Routes.None, string extra = "")
        {
            return SERVER_URL + route switch
            {
                Routes.Authorize => "/authorize",
                Routes.Get => "/get",
                Routes.Upload => "/entry/upload",
                Routes.UpdateUsername => "/entry/update-username",
                Routes.DeleteEntry => "/entry/delete",
                Routes.GetPersonalEntry => "/entry/get",
                Routes.GetEntryCount => "/entry/count",
                _ => "/"
            } + extra;
        }

        private const string SERVER_URL = "https://lcv2-server.danqzq.games";
    }
}