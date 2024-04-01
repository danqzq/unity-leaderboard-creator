using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Dan.Models;

namespace Dan.Demo
{
    public class EntryDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _rankText, _usernameText, _scoreText, _timeText;
        
        public void SetEntry(Entry entry)
        {
            _rankText.text = entry.RankSuffix();
            _usernameText.text = entry.Username;
            _scoreText.text = entry.Score.ToString();
            var dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(entry.Date);
            _timeText.text = $"{dateTime.Hour:00}:{dateTime.Minute:00}:{dateTime.Second:00} (UTC)\n{dateTime:dd/MM/yyyy}";
            
            GetComponent<Image>().color = entry.IsMine() ? Color.yellow : Color.white;
        }
    }
}