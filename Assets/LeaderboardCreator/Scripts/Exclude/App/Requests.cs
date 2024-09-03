using System;
using System.Collections;
using System.Collections.Generic;
using Dan.Enums;
using UnityEngine.Networking;

namespace Dan
{
    internal static partial class Requests
    {
        internal const string ITCH_TOKEN_SAVE_KEY = "ItchToken";
        private const string APP_NAME = "LCV3";
        
        /// <summary>
        /// Used for sending a web request. If there is an error, it will log out an error message to the screen.
        /// </summary>
        /// <param name="request">The UnityWebRequest to process.</param>
        /// <param name="onComplete">The callback which is called if the request is completed successfully.</param>
        /// <param name="log">If true, it will log out a message upon the completion of the request.</param>
        internal static IEnumerator HandleRequest(UnityWebRequest request, Action<bool> onComplete, bool log = true)
        {
            request.SetRequestHeader("X-Request-Verification-Token", APP_NAME);
            yield return request.SendWebRequest();

            if ((StatusCode) request.responseCode != StatusCode.Ok)
            {
                var message = Enum.GetName(typeof(StatusCode), (StatusCode) request.responseCode).SplitByUppercase();
                
                var downloadHandler = request.downloadHandler;
                var text = downloadHandler.text;
                if (!string.IsNullOrEmpty(text))
                    message = $"{message}: {text}";
                
                if (log) Log.Show(message, LogType.Error);
                onComplete?.Invoke(false);
                request.downloadHandler.Dispose();
                request.Dispose();
                yield break;
            }

            onComplete?.Invoke(true);
            request.downloadHandler.Dispose();
            request.Dispose();
        }
    }
}