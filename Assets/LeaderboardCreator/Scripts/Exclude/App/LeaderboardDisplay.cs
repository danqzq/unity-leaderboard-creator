using Dan.App.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dan.App
{
    public class LeaderboardDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _nameText;
        [SerializeField] private Button _manageButton;
        [SerializeField] private Button _resetButton;

        [SerializeField] private Outline _ommOutline;
        
        private Leaderboard _leaderboard;
        
        public static event System.Action OnNameChanged;

        public void Init(Leaderboard leaderboard, System.Action onLeaderboardOpen, System.Action onLeaderboardReset)
        {
            _leaderboard = leaderboard;
            _nameText.text = leaderboard.Name;
            _ommOutline.enabled = leaderboard.IsSuperLeaderboard;
            _manageButton.onClick.AddListener(onLeaderboardOpen.Invoke);
            _resetButton.onClick.AddListener(onLeaderboardReset.Invoke);
        }
        
        public void SetLeaderboardName(string newName)
        {
            _leaderboard.Name = newName;
            OnNameChanged?.Invoke();
        }

        public void OnKeyButtonClicked()
        {
            Clipboard.SetText(_leaderboard.Key);
            Log.Show($"Key of leaderboard {_leaderboard.Name} revealed. Copy it from above.", LogType.Warning);
        }
    }
}