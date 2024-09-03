using System;
using System.Collections;
using System.Security.Cryptography;
using Dan.Enums;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Dan.Legacy
{
    /// <summary>
    /// This class is used to create and activate leaderboards.
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        [Header("UI Elements:")] 
        [SerializeField] private InputField publicKeyText;
        [SerializeField] private InputField secretKeyText;
        [SerializeField] private TMPro.TMP_InputField code;
        [SerializeField] private CanvasGroup codeSection;

        [Header("Animation Elements:")]
        [SerializeField] private Animator dan;
        [SerializeField] private GameObject cat;

        private const float TimeoutSession = 60f;
        private float _timeout;

        private string _exampleCode;

        public void GenerateKeys() => StartCoroutine(RequestKeys());

        private IEnumerator RequestKeys()
        {
            // Check if there is a timeout
            if (_timeout > 0)
            {
                Log.Show("Wait before you can send another request.", LogType.Error);
                yield break;
            }

            _timeout = TimeoutSession;
            
            yield return FadeImage(codeSection, false);
            
            Log.Show("Sending request...", LogType.Warning);
            cat.SetActive(true);
            
            var request = UnityWebRequest.Post(Constants.GetServerURL(), Requests.Form());
            var downloadHandler = request.downloadHandler;
            
            // Send the request and handle for errors.
            var requestFailed = false;
            yield return Requests.HandleRequest(request, ok => requestFailed = ok);
            
            // If the request failed - return.
            if (requestFailed) yield break;

            var publicKey = downloadHandler.text;
            Log.Show("Public key received, generating secret key...", LogType.Warning);
            
            // Display the public key to the user
            publicKeyText.text = publicKey;

            // Generate a secret key using AES (Advanced Encryption Standard)
            var aes = Aes.Create();
            aes.KeySize = 256;
            var secretKey = Convert.ToBase64String(aes.Key);
            
            var activationRequest = UnityWebRequest.Post(Constants.GetServerURL(Routes.Activate), Requests.Form(
                Requests.Field("publicKey", publicKey),
                Requests.Field("aes", secretKey)
            ));
            
            // Send another request and handle it for errors.
            requestFailed = false;
            yield return Requests.HandleRequest(request, ok => requestFailed = ok);
            
            // If the request failed - return.
            if (requestFailed) yield break;

            secretKeyText.text = secretKey;
            
            Log.Show(activationRequest.downloadHandler.text, LogType.Message);
            cat.SetActive(false);
            
            // Generate the code with proper format
            _exampleCode = Constants.ExampleCodeRaw.Replace("~", "\"" + publicKey + "\"")
                .Replace("`", "\"" + secretKey + "\"");

            code.text = Constants.ExampleCode.Replace("~", "\"" + publicKey + "\"")
                .Replace("`", "\"" + secretKey + "\"");
            
            dan.enabled = true;
            
            yield return new WaitForSeconds(5f);
            
            yield return FadeImage(codeSection, true);
        }
        
        private static IEnumerator FadeImage(CanvasGroup image, bool fadeIn)
        {
            for (var i = 0; i < 50; i++)
            {
                image.alpha += fadeIn ? 0.02f : -0.02f;
                yield return new WaitForSeconds(0.01f);
            }
        }

        public void CopyCode() => Clipboard.SetText(_exampleCode);

        public void VisitAuthorPage() => Application.OpenURL("https://danqzq.games");

        private void Update()
        {
            if (_timeout > 0)
                _timeout -= Time.deltaTime;
        }
    }
}
