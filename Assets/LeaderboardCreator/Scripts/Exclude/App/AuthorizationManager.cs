using System;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Dan.App
{
    public class AuthorizationManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _usernameInputField, _passwordInputField;
        [SerializeField] private TMP_InputField _tokenInputField;

        [SerializeField] private CanvasGroup _loginMenu, _tokenMenu;
        
        [SerializeField] private Loader _loader;
        
        private void Start()
        {
            if (PlayerPrefs.GetString(Requests.ITCH_TOKEN_SAVE_KEY) != "")
            {
                ProceedToNextScene();
            }
            
            _loginMenu.alpha = 1;
            _loginMenu.interactable = true;
            _loginMenu.blocksRaycasts = true;
            
            _tokenMenu.alpha = 0;
            _tokenMenu.interactable = false;
            _tokenMenu.blocksRaycasts = false;
        }
        
        public void OnLoginButtonPressed()
        {
            Application.OpenURL(
                "https://itch.io/user/oauth?client_id=c83529366a0683ba795faffb6089b180" +
                "&scope=profile%3Ame&response_type=token&redirect_uri=urn%3Aietf%3Awg%3Aoauth%3A2.0%3Aoob");
            
            _loginMenu.alpha = 0;
            _loginMenu.interactable = false;
            _loginMenu.blocksRaycasts = false;
            
            _tokenMenu.alpha = 1;
            _tokenMenu.interactable = true;
            _tokenMenu.blocksRaycasts = true;
        }
        
        public void OnCompleteLoginButtonPressed()
        {
            var token = _tokenInputField.text;
            if (string.IsNullOrEmpty(token))
            {
                Log.Show("Token is empty.", LogType.Error);
                return;
            }

            var request = UnityWebRequest.Get(ConstantVariables.SERVER_URL + "/login-itch");
            request.SetRequestHeader("Authorization", $"Bearer {token}");
            _loader.Activate();
            SendRequest(request, isSuccessful =>
            {
                if (!isSuccessful) 
                    return;
                Log.Show("Authorization successful.", LogType.Message);
                PlayerPrefs.SetString(Requests.ITCH_TOKEN_SAVE_KEY, token);
                ProceedToNextScene();
            });
        }
        
        private void SendRequest(UnityWebRequest request, Action<bool> callback)
        {
            _loader.Activate();
            StartCoroutine(Requests.HandleRequest(request, isSuccessful =>
            {
                _loader.Deactivate();
                callback(isSuccessful);
            }));
        }
        
        private void ProceedToNextScene()
        {
            SceneManager.LoadScene("Main");
        }
    }
}
