using System.Collections;
using Dan.Enums;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Dan.Legacy
{
    /// <summary>
    /// This class is used for testing the downloading and uploading entries to leaderboards.
    /// </summary>
    public class LeaderboardTest : MonoBehaviour
    {
        [Header("UI References:")]
        [SerializeField] private Text[] highscoreFields;
        
        [SerializeField] private InputField publicKeyField, secretKeyField, nameField, highscoreField;

        public void DownloadHighscores() => StartCoroutine(DownloadHighscoresCoroutine());

        /// <summary>
        /// Requests highscores from a leaderboard using its public key.
        /// </summary>
        private IEnumerator DownloadHighscoresCoroutine()
        {
            // Create the form to send to the server.
            var request = UnityWebRequest.Post(Constants.GetServerURL(Routes.Get), Requests.Form(
                Requests.Field("publicKey", publicKeyField.text)
            ));
            
            // Send the request and handle for errors.
            var requestFailed = false;
            yield return Requests.HandleRequest(request, ok => requestFailed = ok);
            
            // If the request failed - return.
            if (requestFailed) yield break;

            // Parse the response.
            var leaderboard = JsonUtility.FromJson<LegacyLeaderboard>(request.downloadHandler.text);
            var highscores = leaderboard.lb;
            
            // Display the highscores in the UI.
            ClearHighscoreFields();
            for (var i = 0; i < highscores.Count && i < highscoreFields.Length; i++)
            {
                highscoreFields[i].text = $"{i + 1}) {highscores[i].name} | {highscores[i].highscore}";
            }
        }
        
        public void NewHighscore() => StartCoroutine(NewHighscoreCoroutine());

        /// <summary>
        /// Uploads a new highscore to a leaderboard using its secret key.
        /// </summary>
        private IEnumerator NewHighscoreCoroutine()
        {
            // Validate the highscore input field.
            if (!int.TryParse(highscoreField.text, out _))
            {
                Log.Show("Highscore can only be an integer ( ͡° ͜ʖ ͡°)", LogType.Error);
                yield break;
            }
            
            // Create the form to send to the server.
            var request = UnityWebRequest.Post(Constants.GetServerURL(Routes.Upload), Requests.Form(
                Requests.Field("secretKey", secretKeyField.text),
                Requests.Field("name", nameField.text),
                Requests.Field("highscore", highscoreField.text)
            ));

            // Send the request and handle for errors.
            var requestFailed = false;
            yield return Requests.HandleRequest(request, ok => requestFailed = ok);
            
            // If the request failed - return.
            if (requestFailed) yield break;
            
            // Display the success message.
            Log.Show(request.downloadHandler.text, LogType.Message);
        }

        private void ClearHighscoreFields()
        {
            foreach (var t in highscoreFields)
                t.text = "";
        }
    }
}