using System.Globalization;
using Dan.Models;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Dan.App
{
    public class EntryDisplay : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private TextMeshProUGUI _rankText, _dateText;
        [SerializeField] private TMP_InputField _usernameText, _scoreText, _extraText;
        [SerializeField] private Button _editButton, _completeEditButton, _deleteButton;

        [SerializeField] private TextMeshProUGUI _guidText; 
        
        private Image _background;
        private Color _originalColor;
        
        private bool _guidVisible, _isHeld;
        private float _showTimer;

        private long _originalScore;
        
        public void SetEntry(Entry entry, System.Action<Entry> onEdit, System.Action<string, string> onDelete,
            bool guidVisible, float scoreMultiplier)
        {
            _background = GetComponent<Image>();
            _originalColor = _background.color;
            
            _originalScore = entry.Score;
            
            _guidVisible = guidVisible;
            _rankText.text = entry.RankSuffix();
            _usernameText.text = entry.Username;
            _scoreText.text = (entry.Score * scoreMultiplier).ToString(CultureInfo.CurrentCulture);
            var dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(entry.Date);
            _dateText.text = $"{dateTime.Hour:00}:{dateTime.Minute:00}:{dateTime.Second:00} (UTC)\n{dateTime:dd/MM/yyyy}";
            _extraText.text = entry.Extra;
            _guidText.text = entry.UserGuid;
            
            _usernameText.interactable = false;
            _scoreText.interactable = false;
            _extraText.interactable = false;
            _editButton.gameObject.SetActive(true);
            _completeEditButton.gameObject.SetActive(false);
            _deleteButton.gameObject.SetActive(true);
            
            _usernameText.onEndEdit.RemoveAllListeners();
            _scoreText.onEndEdit.RemoveAllListeners();
            _extraText.onEndEdit.RemoveAllListeners();
            
            _usernameText.onEndEdit.AddListener(text => entry.username = text);
            
            _scoreText.onEndEdit.AddListener(text => 
                entry.score = int.TryParse(text, out var score) ? score : entry.Score);
            
            _extraText.onEndEdit.AddListener(text => entry.extra = text);
            
            _editButton.onClick.RemoveAllListeners();

            _editButton.onClick.AddListener(() =>
            {
                _scoreText.text = _originalScore.ToString(CultureInfo.CurrentCulture);
                _usernameText.interactable = true;
                _scoreText.interactable = true;
                _extraText.interactable = true;
                _editButton.gameObject.SetActive(false);
                _completeEditButton.gameObject.SetActive(true);
                _deleteButton.gameObject.SetActive(false);
            });
            
            _completeEditButton.onClick.RemoveAllListeners();
            
            _completeEditButton.onClick.AddListener(() => onEdit(entry));
            
            _deleteButton.onClick.RemoveAllListeners();
            
            _deleteButton.onClick.AddListener(() => onDelete(entry.Username, entry.UserGuid));

            AppManager.OnScoreMultiplierChanged += OnScoreMultiplierChanged;
        }

        private void OnDestroy() => AppManager.OnScoreMultiplierChanged -= OnScoreMultiplierChanged;

        private void OnScoreMultiplierChanged(float multiplier)
        {
            if (_scoreText.interactable) return;
            _scoreText.text = (_originalScore * multiplier).ToString(CultureInfo.CurrentCulture);
        }

        public void HighlightUsername()
        {
            GetComponent<Outline>().enabled = true;
        }
        
        public void CopyGuid()
        {
            _guidText.transform.parent.gameObject.SetActive(false);
            if (!_guidVisible) return;
            
            var guid = _guidText.text;
            if (string.IsNullOrEmpty(guid))
            {
                Log.Show("GUID is empty.", LogType.Message);
                return;
            }
            
            Clipboard.SetText(guid);
            Log.Show("GUID revealed. Copy it from above.", LogType.Message);
        }

        public void OnPointerDown(PointerEventData eventData) => _isHeld = true;
        

        public void OnPointerUp(PointerEventData eventData) => _isHeld = false;

        private void Update()
        {
            if (!_guidVisible) return;
            
            if (_isHeld && _showTimer < 1f)
            {
                _showTimer += Time.deltaTime;
                if (_showTimer > 1f)
                {
                    _guidText.transform.parent.gameObject.SetActive(true);
                }
                _background.color = Color.Lerp(_originalColor, _originalColor + Color.gray * 0.1f, _showTimer);
            }
            else if (_showTimer > 0)
            {
                _showTimer = 0;
                _background.color = _originalColor;
            }
        }
    }
}