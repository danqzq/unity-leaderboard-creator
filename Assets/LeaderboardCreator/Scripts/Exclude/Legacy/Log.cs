using TMPro;
using UnityEngine;

namespace Dan
{
    public enum LogType
    {
        Message,
        Warning,
        Error
    }
    
    /// <summary>
    /// Custom logger for the app which just displays visual messages to the screen.
    /// </summary>
    public class Log : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _logObject;

        private static TextMeshProUGUI _log;
        private static Transform _canvas;
        
        private void Start()
        {
            _log = _logObject;
            _canvas = FindFirstObjectByType<Canvas>().transform;
        }

        private static Color LogTypeToColor(LogType logType) =>
            logType switch
            {
                LogType.Message => new Color(0.0f, 0.6f, 0.0f),
                LogType.Warning => Color.yellow,
                LogType.Error => Color.red,
                _ => Color.white
            };

        /// <summary>
        /// Displays a message to the screen for a short time.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="logType">The log type of the message.</param>
        public static void Show(string message, LogType logType)
        {
            var tempLog = Instantiate(_log, _canvas);
            tempLog.text = message;
            tempLog.color = LogTypeToColor(logType);
            Destroy(tempLog, 2f);
        }
    }
}