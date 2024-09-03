using System;
using System.Collections.Generic;
using System.Linq;
using Dan.App.Models;
using Dan.Enums;
using Dan.Main;
using Dan.Models;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Dan.App
{
    public class AppManager : MonoBehaviour
    {
#if UNITY_EDITOR
        private const string SERVER_URL = "http://localhost:10000";
#else
        private const string SERVER_URL = "https://lcv3-server.danqzq.games";
#endif
        [SerializeField] private CanvasGroup _manageMenu;
        [SerializeField] private TextMeshProUGUI _manageMenuTitle;
        
        [SerializeField] private Loader _loader;

        [SerializeField] private TextMeshProUGUI _serverStatusText;
        
        [SerializeField] private GameObject _noLeaderboardsAddedText;

        [SerializeField] private GameObject _leaderboardDisplayPrefab;
        [SerializeField] private Transform _leaderboardDisplayParent;
        
        [SerializeField] private GameObject _leaderboardEntryPrefab;
        [SerializeField] private Transform _leaderboardEntryParent;

        [SerializeField] private TMP_InputField _newEntryUsername, _newEntryScore, _newEntryExtra;
        [SerializeField] private Toggle _ascendingOrderToggle, _profanityFilterToggle, _uniqueUsernamesToggle;
        [SerializeField] private TMP_InputField _secretKeyInputField;

        [SerializeField] private TextMeshProUGUI _usernameTableText;
        [SerializeField] private TMP_InputField _usernameSearchInputField,
            _skipInputField, _takeInputField, _scoreMultiplierInputField;

        [SerializeField] private ParticleSystem _confettiEffect;

        [SerializeField] private CanvasGroup _skipTakeHint;

        [SerializeField] private GameObject _deleteMenu, _superDeleteMenu;
        
        [SerializeField] private TMP_InputField _belowScoreInputField, _aboveScoreInputField;
        [SerializeField] private TMP_InputField _belowRankInputField, _aboveRankInputField;
        [SerializeField] private TMP_InputField _afterDaysInputField;

        [SerializeField] private GameObject _resetKeysMenu;
        [SerializeField] private TextMeshProUGUI _resetKeysText;
        [SerializeField] private Button _resetKeysButton;

        [SerializeField] private GameObject _blacklistButton;
        [SerializeField] private CanvasGroup _blacklistMenu;
        [SerializeField] private TMP_InputField _blacklistUsernameInputField;
        [SerializeField] private TMP_InputField _blacklistUserIDInputField;
        [SerializeField] private GameObject _blacklistedUserPrefab;
        [SerializeField] private Transform _blacklistedUserContent;
        
        private List<Leaderboard> _savedLeaderboards;
        
        private Leaderboard _currentLeaderboard;
        private LeaderboardSearchQuery _searchQuery;

        private int _actionCounter;
        
        internal static event Action<float> OnScoreMultiplierChanged;

        private void Start()
        {
            _actionCounter = PlayerPrefs.GetInt("actionCounter", 0);
            
            if (PlayerPrefs.HasKey("savedLeaderboards"))
                _savedLeaderboards = JsonConvert.DeserializeObject<List<Leaderboard>>(PlayerPrefs.GetString("savedLeaderboards"));
            
            _savedLeaderboards ??= new List<Leaderboard>();

            foreach (var leaderboard in _savedLeaderboards) 
                CreateLeaderboardDisplay(leaderboard);

            LeaderboardDisplay.OnNameChanged += SaveLeaderboards;
            GetServerStatus();
            
            _searchQuery = LeaderboardSearchQuery.Default;
            
            _scoreMultiplierInputField.onValueChanged.AddListener(value =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    OnScoreMultiplierChanged?.Invoke(1f);
                    return;
                }
                OnScoreMultiplierChanged?.Invoke(float.Parse(value));
            });
        }
        
        private void OnDestroy()
        {
            LeaderboardDisplay.OnNameChanged -= SaveLeaderboards;
        }

        public void GetServerStatus()
        {
            var request = UnityWebRequest.Get(SERVER_URL);
            SendRequest(request, isSuccessful =>
            {
                if (!isSuccessful)
                {
                    _serverStatusText.text = "<b><color=red>Failed to connect to server!</color></b>";
                    return;
                }
                _serverStatusText.text = "<b>Server Status:</b>\n" + request.downloadHandler.text;
                Log.Show("Server status has been updated!", LogType.Message);
            });
        }

        public void VerifyPurchase()
        {
            var request = UnityWebRequest.Get(SERVER_URL + "/verify-itch-purchase");
            SendRequest(request, isSuccessful =>
            {
                if (!isSuccessful) return;
                Log.Show("Verified, adding your leaderboards...", LogType.Message);
                
                TryAddLeaderboardsFromAccount();
            });
        }

        [System.Serializable]
        private struct MyLeaderboardsResponse
        {
            public string[] keys;
        }

        private void TryAddLeaderboardsFromAccount()
        {
            var request = UnityWebRequest.Get(SERVER_URL + "/get-my-leaderboards");
            SendRequest(request, isSuccessful =>
            {
                if (!isSuccessful) return;
                var response = JsonUtility.FromJson<MyLeaderboardsResponse>(request.downloadHandler.text);
                foreach (var key in response.keys)
                    AddExistingLeaderboardViaString(key);
            });
        }
        
        private void SaveLeaderboards()
        {
            PlayerPrefs.SetString("savedLeaderboards", JsonConvert.SerializeObject(_savedLeaderboards));
            PlayerPrefs.Save();
            ClearLeaderboardDisplays();
            foreach (var lb in _savedLeaderboards) 
                CreateLeaderboardDisplay(lb);
        }

        public void Create()
        {
            var request = UnityWebRequest.Get(SERVER_URL + "/create");
            SendRequest(request, isSuccessful => OnLeaderboardAdded(isSuccessful, request));
        }

        private void AddExistingLeaderboardViaString(string key, string keptName = null, int order = 0)
        {
            if (ValidateForEmpty(key)) return;
            
            if (_savedLeaderboards.Count(x => x.Key == key) != 0)
            {
                Log.Show("Leaderboard is already added", LogType.Warning);
                return;
            }
            
            var request = UnityWebRequest.Get(SERVER_URL + "/get?key=" + key);
            SendRequest(request, isSuccessful => OnLeaderboardAdded(isSuccessful, request, keptName, order));
        }

        private string OnLeaderboardAdded(bool isSuccessful, UnityWebRequest request, string keptName = null, int order = 0)
        {
            if (!isSuccessful) return null;
            var leaderboard = JsonConvert.DeserializeObject<Leaderboard>(request.downloadHandler.text);
            leaderboard ??= new Leaderboard();
            leaderboard.Name = keptName ?? leaderboard.Name;
            if (string.IsNullOrEmpty(leaderboard.Name))
                leaderboard.Name = $"leaderboard-{Random.Range(0, 100000):00000}";
            _savedLeaderboards.Insert(order, leaderboard);
            SaveLeaderboards();
            GetServerStatus();
            Log.Show($"Added {leaderboard.Name} successfully!", LogType.Message);

            // if (!leaderboard.IsSuperLeaderboard)
            //     return leaderboard.Key;
            
            // _confettiEffect.Play();
            // IEnumerator LogWithDelay()
            // {
            //     yield return new WaitForSeconds(1f);
            //     Log.Show("WOOHOO! You got an advanced leaderboard!", LogType.Message);
            // }
            // StartCoroutine(LogWithDelay());

            return leaderboard.Key;
        }
        
        public void IntegrationTutorial()
        {
            Application.OpenURL("https://www.youtube.com/watch?v=v0aWwSkC-4o");
        }

        public void DeleteLeaderboard()
        {
            var request = UnityWebRequest.Post(SERVER_URL + "/delete", Requests.Form(
                Requests.Field("key", _currentLeaderboard.Key)));
            SendRequest(request, isSuccessful =>
            {
                if (!isSuccessful) return;
                CloseMenu(_manageMenu);
                Log.Show($"{_currentLeaderboard.Name} has been deleted successfully!", LogType.Message);
                _savedLeaderboards.Remove(_currentLeaderboard);
                _currentLeaderboard = null;
                SaveLeaderboards();
                GetServerStatus();
            });
        }
        
        public void UploadEntry()
        {
            if (ValidateForEmpty(_newEntryUsername.text, _newEntryScore.text)) return;
            var request = UnityWebRequest.Post(SERVER_URL + "/entry/upload", Requests.Form(
                Requests.Field("username", _newEntryUsername.text),
                Requests.Field("userGuid", LeaderboardCreator.UserGuid + "---" + _actionCounter++),
                Requests.Field("score", _newEntryScore.text),
                Requests.Field("extra", string.IsNullOrEmpty(_newEntryExtra.text) ? " " : _newEntryExtra.text),
                Requests.Field("key", _currentLeaderboard.Key)));
            SendRequest(request, isSuccessful =>
            {
                if (!isSuccessful) return;
                Log.Show("Entry has been uploaded successfully!", LogType.Message);
                ClearEntryDisplays();
                LoadEntries();
                GetServerStatus();
            });
            
            PlayerPrefs.SetInt("actionCounter", _actionCounter);
        }
        
        private void LoadEntries()
        {
            if (_searchQuery.Take > 100)
            {
                Log.Show("You can only load up to 100 entries at a time!", LogType.Error);
                return;
            }

            var query = $"?key={_currentLeaderboard.Key}";
            query += _searchQuery.ChainQuery();
            var request = UnityWebRequest.Get(SERVER_URL + "/get" + query + "&entriesOnly=1");
            SendRequest(request, isSuccessful =>
            {
                if (!isSuccessful) return;
                var entries = JsonConvert.DeserializeObject<List<Entry>>(request.downloadHandler.text);
                
                if (entries == null) return;
                for (var i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    CreateEntryDisplay(entry, EditEntry, DeleteEntry);
                }
            });
        }
        
        private void EditEntry(Entry entry)
        {
            if (ValidateForEmpty(entry.Score.ToString())) return;
            var request = UnityWebRequest.Post(SERVER_URL + "/edit-entry", Requests.Form(
                Requests.Field("userGuid", entry.UserGuid),
                Requests.Field("score", entry.Score.ToString()),
                Requests.Field("username", entry.Username),
                Requests.Field("extra", string.IsNullOrEmpty(entry.Extra) ? " " : entry.Extra),
                Requests.Field("key", _currentLeaderboard.Key)));
            SendRequest(request, isSuccessful =>
            {
                if (!isSuccessful) return;
                Log.Show("Entry has been edited successfully!", LogType.Message);
                ClearEntryDisplays();
                LoadEntries();
            });
        }
        
        private void DeleteEntry(string username, string userGuid)
        {
            var field = string.IsNullOrEmpty(userGuid) ? 
                Requests.Field("username", username) : 
                Requests.Field("userGuid", userGuid);
            var request = UnityWebRequest.Post(SERVER_URL + "/entry/delete", Requests.Form(field,
                Requests.Field("key", _currentLeaderboard.Key)));
            SendRequest(request, isSuccessful =>
            {
                if (!isSuccessful) return;
                Log.Show("Entry has been deleted successfully!", LogType.Message);
                ClearEntryDisplays();
                LoadEntries();
            });
        }

        private void SendRequest(UnityWebRequest request, Action<bool> callback)
        {
            _loader.Activate();
            request.SetRequestHeader("Authorization", PlayerPrefs.GetString(Requests.ITCH_TOKEN_SAVE_KEY));
            StartCoroutine(Requests.HandleRequest(request, isSuccessful =>
            {
                _loader.Deactivate();
                callback(isSuccessful);
            }));
        }
        
        private void SendRequest<T>(UnityWebRequest request, Action<bool, T> callback)
        {
            _loader.Activate();
            StartCoroutine(Requests.HandleRequest(request, isSuccessful =>
            {
                _loader.Deactivate();
                if (!isSuccessful)
                {
                    callback(false, default);
                    return;
                }
                callback(true, JsonConvert.DeserializeObject<T>(request.downloadHandler.text));
            }));
        }

        private void CreateLeaderboardDisplay(Leaderboard leaderboard)
        {
            _noLeaderboardsAddedText.gameObject.SetActive(false);
            var leaderboardDisplay = Instantiate(_leaderboardDisplayPrefab, _leaderboardDisplayParent);
            leaderboardDisplay.GetComponent<LeaderboardDisplay>().Init(leaderboard,
                () => OnOpenLeaderboard(leaderboard), () => OnResetLeaderboard(leaderboard));
        }
        
        private void OnOpenLeaderboard(Leaderboard leaderboard) 
        {
            OpenMenu(_manageMenu);
            _manageMenuTitle.text = $"Managing Leaderboard: {leaderboard.Name}";
                
            _uniqueUsernamesToggle.onValueChanged.RemoveAllListeners();
            _uniqueUsernamesToggle.isOn = leaderboard.UniqueUsernames;
            _uniqueUsernamesToggle.onValueChanged.AddListener(ToggleUniqueUsernames);
                
            _ascendingOrderToggle.onValueChanged.RemoveAllListeners();
            _ascendingOrderToggle.isOn = leaderboard.IsInAscendingOrder;
            _ascendingOrderToggle.onValueChanged.AddListener(ToggleAscendingOrder);
                
            _profanityFilterToggle.onValueChanged.RemoveAllListeners();
            _profanityFilterToggle.isOn = leaderboard.IsProfanityEnabled;
            _profanityFilterToggle.onValueChanged.AddListener(ToggleProfanityFilter);

            _currentLeaderboard = leaderboard;

            _searchQuery.Skip = 0;
            _skipInputField.text = "0";
                
            _searchQuery.Take = 10;
            _takeInputField.text = "10";

            if (!PlayerPrefs.HasKey("skipTakeHint"))
            {
                OpenMenu(_skipTakeHint);
                PlayerPrefs.SetInt("skipTakeHint", 1);
            }

            _manageMenu.GetComponentInChildren<Outline>().enabled = leaderboard.IsSuperLeaderboard;
            _deleteMenu.SetActive(!leaderboard.IsSuperLeaderboard);
            _superDeleteMenu.SetActive(leaderboard.IsSuperLeaderboard);
            _blacklistButton.SetActive(leaderboard.IsSuperLeaderboard);

            if (leaderboard.IsSuperLeaderboard)
            {
                foreach (Transform t in _blacklistedUserContent) 
                    Destroy(t.gameObject);

                _currentLeaderboard.BlacklistUsers ??= Array.Empty<BlacklistUser>();
                foreach (var blacklistUser in _currentLeaderboard.BlacklistUsers)
                {
                    var blacklistedUser = Instantiate(_blacklistedUserPrefab, _blacklistedUserContent);
                    blacklistedUser.GetComponentInChildren<TextMeshProUGUI>().text = blacklistUser.username + " | " + blacklistUser.userGuid;
                    blacklistedUser.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    {
                        _currentLeaderboard.BlacklistUsers = _currentLeaderboard.BlacklistUsers.Where(x => x != blacklistUser).ToArray();
                        Destroy(blacklistedUser.gameObject);
                    });
                }
            }
                    
            ClearEntryDisplays();
            LoadEntries();
        }

        private void OnResetLeaderboard(Leaderboard leaderboard)
        {
            var secretKey = leaderboard.Key;
            var order = _savedLeaderboards.IndexOf(leaderboard);
            _savedLeaderboards.Remove(leaderboard);
            
            AddExistingLeaderboardViaString(secretKey, leaderboard.Name, order);
        }

        [Serializable]
        public struct NewKeyResponse
        {
            public string key;
        }
        
        private void OnResetLeaderboardKey(Leaderboard leaderboard)
        {
            _resetKeysMenu.SetActive(true);
            _resetKeysButton.onClick.RemoveAllListeners();
            _resetKeysText.text = $"Are you sure you want to <b><color=yellow>reset the key</color></b> for {leaderboard.Name}?";
            _resetKeysButton.onClick.AddListener(() =>
            {
                var request = UnityWebRequest.Post(SERVER_URL + "/reset-key", Requests.Form(
                    Requests.Field("key", leaderboard.Key)));
                _loader.Activate();
                SendRequest<NewKeyResponse>(request, (isSuccessful, obj) =>
                {
                    if (!isSuccessful)
                    {
                        Log.Show("Failed to reset key!", LogType.Error);
                        _loader.Deactivate();
                        return;
                    }
                    Log.Show("Key has been reset successfully!", LogType.Message);
                    _resetKeysMenu.SetActive(false);
                    _loader.Deactivate();
                    leaderboard.Key = obj.key;
                    SaveLeaderboards();
                    GetServerStatus();
                });
            });
        }
        
        private void ClearLeaderboardDisplays()
        {
            foreach (Transform child in _leaderboardDisplayParent)
                Destroy(child.gameObject);
            _noLeaderboardsAddedText.gameObject.SetActive(true);
        }
        
        private void CreateEntryDisplay(Entry entry, Action<Entry> onEdit, Action<string, string> onDelete)
        {
            var leaderboardEntry = Instantiate(_leaderboardEntryPrefab, _leaderboardEntryParent);
            float multiplier;
            if (!float.TryParse(_scoreMultiplierInputField.text, out multiplier))
                multiplier = 1f;
            leaderboardEntry.GetComponent<EntryDisplay>().SetEntry(entry, onEdit, onDelete, _currentLeaderboard.IsSuperLeaderboard,
                multiplier);
            
            if (_searchQuery.Username == entry.Username ||
                string.Equals(_searchQuery.Username, entry.Username, StringComparison.CurrentCultureIgnoreCase))
                leaderboardEntry.GetComponent<EntryDisplay>().HighlightUsername();
        }
        
        private void ClearEntryDisplays()
        {
            foreach (Transform entryDisplay in _leaderboardEntryParent)
                Destroy(entryDisplay.gameObject);
        }

        private void ToggleUniqueUsernames(bool isOn)
        {
            var request = UnityWebRequest.Post(SERVER_URL + "/update", Requests.Form(
                Requests.Field("uniqueUsernames", isOn ? "true" : "false"),
                Requests.Field("key", _currentLeaderboard.Key)));
            SendRequest(request, isSuccessful =>
            {
                if (!isSuccessful) return;
                _currentLeaderboard.UniqueUsernames = isOn;
                Log.Show("Unique usernames has been updated successfully!", LogType.Message);
                SaveLeaderboards();
            });
        }
        
        private void ToggleAscendingOrder(bool isOn)
        {
            var request = UnityWebRequest.Post(SERVER_URL + "/update", Requests.Form(
                Requests.Field("isInAscendingOrder", isOn ? "true" : "false"),
                Requests.Field("key", _currentLeaderboard.Key)));
            SendRequest(request, isSuccessful =>
            {
                if (!isSuccessful) return;
                _currentLeaderboard.IsInAscendingOrder = isOn;
                Log.Show("Sorting order has been updated successfully!", LogType.Message);
                SaveLeaderboards();
                ClearEntryDisplays();
                LoadEntries();
            });
        }
        
        private void ToggleProfanityFilter(bool isOn)
        {
            var request = UnityWebRequest.Post(SERVER_URL + "/update", Requests.Form(
                Requests.Field("isProfanityEnabled", isOn ? "true" : "false"),
                Requests.Field("key", _currentLeaderboard.Key)));
            SendRequest(request, isSuccessful =>
            {
                if (!isSuccessful) return;
                _currentLeaderboard.IsProfanityEnabled = isOn;
                Log.Show("Profanity filter has been updated successfully!", LogType.Message);
                SaveLeaderboards();
            });
        }
        
        public void CopyPublicKey()
        {
            if (_currentLeaderboard == null || string.IsNullOrEmpty(_currentLeaderboard.Key)) return;
            Clipboard.SetText(_currentLeaderboard.Key);
            Log.Show("Key revealed. Copy it from above.", LogType.Warning);
        }
        
        public void OpenMenu(CanvasGroup menu)
        {
            menu.alpha = 1;
            menu.interactable = true;
            menu.blocksRaycasts = true;
        }
        
        public void CloseMenu(CanvasGroup menu)
        {
            menu.alpha = 0;
            menu.interactable = false;
            menu.blocksRaycasts = false;
        }

        public void ToggleMenu(CanvasGroup menu)
        {
            if (menu.alpha == 0)
                OpenMenu(menu);
            else
                CloseMenu(menu);
        }

        public void OnCreditsClick()
        {
            
        }
        
        public void OnWebsiteClick()
        {
            Application.OpenURL("https://www.danqzq.games");
        }

        private static bool ValidateForEmpty(params string[] texts)
        {
            if (!texts.Any(string.IsNullOrEmpty)) return false;
            Log.Show("Please fill all the required fields!", LogType.Error);
            return true;
        }
        
        public void OnSkipTextValueChanged(string value)
        {
            var parsedValue = string.IsNullOrEmpty(value) ? 0 : int.Parse(value);
            if (parsedValue < 0) parsedValue = 0;
            if (parsedValue == _searchQuery.Skip) return;
            
            _searchQuery.Skip = parsedValue;
            
            if (_loader.IsActive) return;
            
            ClearEntryDisplays();
            LoadEntries();
        }
        
        public void OnTakeTextValueChanged(string value)
        {
            var parsedValue = string.IsNullOrEmpty(value) ? 0 : int.Parse(value);
            if (parsedValue < 0) parsedValue = 0;
            if (parsedValue == _searchQuery.Take) return;

            _searchQuery.Take = parsedValue;

            if (_loader.IsActive) return;
            
            ClearEntryDisplays();
            LoadEntries();
        }
        
        public void OnTimePeriodDropdownValueChanged(int value)
        {
            _searchQuery.TimePeriod = value == 1 ? TimePeriodType.Today :
                value == 2 ? TimePeriodType.ThisWeek :
                value == 3 ? TimePeriodType.ThisMonth :
                value == 4 ? TimePeriodType.ThisYear : TimePeriodType.AllTime;

            ClearEntryDisplays();
            LoadEntries();
        }

        public void OnSearchUsernameEndEdit()
        {
            _searchQuery.Username = _usernameSearchInputField.text;
            
            if (string.IsNullOrEmpty(_searchQuery.Username))
            {
                _usernameTableText.text = "Username";
                _usernameTableText.fontStyle = FontStyles.Normal;
            }
            else
            {
                _usernameTableText.text = _searchQuery.Username;
                _usernameTableText.fontStyle = FontStyles.Underline;
                _skipInputField.text = "";
                _takeInputField.text = "";
            }

            ClearEntryDisplays();
            LoadEntries();
        }
        
        public void ClearUsernameSearch()
        {
            if (string.IsNullOrEmpty(_searchQuery.Username)) return;
            
            _usernameTableText.text = "Username";
            _usernameTableText.fontStyle = FontStyles.Normal;
            
            _searchQuery.Username = "";
            _usernameSearchInputField.text = "";
            
            ClearEntryDisplays();
            LoadEntries();
        }

        public void ClearLeaderboard()
        {
            var multipartForm = new List<IMultipartFormSection>
            {
                Requests.Field("key", _currentLeaderboard.Key)
            };
            var request = UnityWebRequest.Post(SERVER_URL + "/clear", multipartForm);
            SendRequest(request, isSuccessful =>
            {
                if (!isSuccessful) return;
                Log.Show("Leaderboard has been cleared successfully!", LogType.Message);
                ClearEntryDisplays();
                LoadEntries();
            });
        }

        public void DeleteManyEntries()
        {
            var multipartForm = new List<IMultipartFormSection>
            {
                Requests.Field("key", _currentLeaderboard.Key)
            };
            if (!string.IsNullOrEmpty(_belowScoreInputField.text))
                multipartForm.Add(Requests.Field("belowScore", _belowScoreInputField.text));
            if (!string.IsNullOrEmpty(_aboveScoreInputField.text))
                multipartForm.Add(Requests.Field("aboveScore", _aboveScoreInputField.text));
            if (!string.IsNullOrEmpty(_belowRankInputField.text))
                multipartForm.Add(Requests.Field("belowRank", _belowRankInputField.text));
            if (!string.IsNullOrEmpty(_aboveRankInputField.text))
                multipartForm.Add(Requests.Field("aboveRank", _aboveRankInputField.text));
            if (!string.IsNullOrEmpty(_afterDaysInputField.text))
                multipartForm.Add(Requests.Field("beforeDays", _afterDaysInputField.text));
            
            var request = UnityWebRequest.Post(SERVER_URL + "/delete-many-entries", multipartForm);
            SendRequest(request, isSuccessful =>
            {
                if (!isSuccessful) return;
                Log.Show("Entries have been deleted successfully!", LogType.Message);
                ClearEntryDisplays();
                LoadEntries();
            });
        }
        
        public void BlacklistUser()
        {
            if (string.IsNullOrEmpty(_blacklistUsernameInputField.text) && string.IsNullOrEmpty(_blacklistUserIDInputField.text))
            {
                Log.Show("Please fill at least one of the fields!", LogType.Error);
                return;
            }
            
            var blacklistUser = new BlacklistUser(_blacklistUsernameInputField.text, _blacklistUserIDInputField.text);
            _currentLeaderboard.BlacklistUsers = _currentLeaderboard.BlacklistUsers.Append(blacklistUser).ToArray();
            
            _blacklistUsernameInputField.text = "";
            _blacklistUserIDInputField.text = "";
            
            var blacklistUserObject = Instantiate(_blacklistedUserPrefab, _blacklistedUserContent);
            blacklistUserObject.GetComponentInChildren<TextMeshProUGUI>().text = blacklistUser.username + " | " + blacklistUser.userGuid;
            blacklistUserObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                _currentLeaderboard.BlacklistUsers = _currentLeaderboard.BlacklistUsers.Where(x => x != blacklistUser).ToArray();
                Destroy(blacklistUserObject.gameObject);
            });
        }

        public void UpdateBlacklist()
        {
            var request = UnityWebRequest.Post(SERVER_URL + "/update-blacklist", Requests.Form(
                Requests.Field("key", _currentLeaderboard.Key),
                Requests.Field("blacklistUsers", JsonConvert.SerializeObject(_currentLeaderboard.BlacklistUsers))));
            
            _loader.Activate();
            SendRequest(request, isSuccessful =>
            {
                _loader.Deactivate();
                if (!isSuccessful) return;
                CloseMenu(_blacklistMenu);
                Log.Show("Blacklist has been updated successfully!", LogType.Message);
            });
        }
    }
}