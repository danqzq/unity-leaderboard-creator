using Dan.Enums;

namespace Dan
{
    public static class Constants
    {
        public static string GetServerURL(Routes route = Routes.None, string extra = "")
        {
            return ServerURL + route switch
            {
                Routes.Activate => "/activate",
                Routes.Get => "/get",
                Routes.Upload => "/upload",
                _ => "/"
            } + extra;
        }

        private const string ServerURL = "https://leaderboard-creator.danqzq.repl.co";

        public const string ExampleCode = @"[<color=#ff59d0>SerializeField</color>] <color=#2389c7>private</color> <color=#ff59d0>Text</color>[] highscoreFields;

<color=#2389c7>public void</color> <color=#41d27c>DownloadHighscores</color>() => <color=#41d27c>StartCoroutine</color>(<color=#41d27c>RequestHighscores</color>());

<color=#2389c7>private</color> <color=#ff59d0>IEnumerator</color> <color=#41d27c>RequestHighscores</color>()
{
    <color=#2389c7>var</color> request = <color=#ff59d0>UnityWebRequest</color>.<color=#41d27c>Post</color>(<color=#ed9b56>""https://leaderboard-creator.danqzq.repl.co/get""</color>, <color=#2389c7>new</color> <color=#ff59d0>List</color><<color=#ff59d0>IMultipartFormSection</color>>
    {
        <color=#41d27c>Form</color>(<color=#ed9b56>""publicKey""</color>, <color=#ed9b56>~</color>)
    });

    <color=#2389c7>yield return</color> request.<color=#41d27c>SendWebRequest</color>();

    <color=#2389c7>if</color> (!<color=#2389c7>string</color>.<color=#41d27c>IsNullOrEmpty</color>(request.downloadHandler.error))
    {
        <color=#ff59d0>Debug</color>.<color=#41d27c>Log</color>(request.downloadHandler.error);
        <color=#2389c7>yield break</color>;
    }

    <color=#2389c7>var</color> leaderboard = <color=#ff59d0>JsonUtility</color>.<color=#41d27c>FromJson</color><<color=#ff59d0>Leaderboard</color>>(request.downloadHandler.text);
    <color=#2389c7>var</color> highscores = leaderboard.lb;
    
    <color=#41d27c>EmptyHighscoreFields</color>();

    <color=#2389c7>for</color> (<color=#2389c7>int</color> i = 0; i < highscores.Length; i++)
    {
        <color=#2389c7>if</color> (i >= highscoreFields.Length) <color=#2389c7>yield break</color>;
        highscoreFields[i].text = (i + 1) + <color=#ed9b56>"") ""</color> + highscores[i].name + <color=#ed9b56>"" | ""</color> + highscores[i].highscore;
    }
}

<color=#2389c7>public void</color> <color=#41d27c>NewHighscore</color>(<color=#2389c7>string</color> username, <color=#2389c7>int</color> highscore) => <color=#41d27c>StartCoroutine</color>(<color=#41d27c>UploadHighscores</color>(username, highscore));

<color=#2389c7>private</color> <color=#ff59d0>IEnumerator</color> <color=#41d27c>UploadHighscores</color>(<color=#2389c7>string</color> username, <color=#2389c7>int</color> highscore)
{
    <color=#2389c7>var</color> request = <color=#ff59d0>UnityWebRequest</color>.<color=#41d27c>Post</color>(<color=#ed9b56>""https://leaderboard-creator.danqzq.repl.co/upload""</color>, <color=#2389c7>new</color> <color=#ff59d0>List</color><<color=#ff59d0>IMultipartFormSection</color>>
    {
        <color=#41d27c>Form</color>(<color=#ed9b56>""secretKey""</color>, <color=#ed9b56>`</color>),
        <color=#41d27c>Form</color>(<color=#ed9b56>""name""</color>, username),
        <color=#41d27c>Form</color>(<color=#ed9b56>""highscore""</color>, highscore.<color=#41d27c>ToString</color>())
    });

    <color=#2389c7>yield return</color> request.<color=#41d27c>SendWebRequest</color>();
    
    <color=#2389c7>if</color> (!<color=#2389c7>string</color>.<color=#41d27c>IsNullOrEmpty</color>(request.downloadHandler.error))
        <color=#ff59d0>Debug</color>.<color=#41d27c>Log</color>(request.downloadHandler.error);
}

<color=#2389c7>private static</color> <color=#ff59d0>IMultipartFormSection</color> Form(<color=#2389c7>string</color> formName, <color=#2389c7>string</color> data) => <color=#2389c7>new</color> <color=#ff59d0>MultipartFormDataSection</color>(formName, data);

<color=#2389c7>private void</color> <color=#41d27c>EmptyHighscoreFields</color>()
{
    <color=#2389c7>foreach</color> (<color=#2389c7>var</color> t <color=#2389c7>in</color> highscoreFields) t.text = <color=#ed9b56>""""</color>;
}";
        
        public const string ExampleCodeRaw = @"[SerializeField] private Text[] highscoreFields;

public void DownloadHighscores() => StartCoroutine(RequestHighscores());

private IEnumerator RequestHighscores()
{
    var request = UnityWebRequest.Post(""https://leaderboard-creator.danqzq.repl.co/get"", new List<IMultipartFormSection>
    {
        Form(""publicKey"", ~)
    });

    yield return request.SendWebRequest();

    if (!string.IsNullOrEmpty(request.downloadHandler.error))
    {
        Debug.Log(request.downloadHandler.error);
        yield break;
    }

    var leaderboard = JsonUtility.FromJson<Leaderboard>(request.downloadHandler.text);
    var highscores = leaderboard.lb;
    
    EmptyHighscoreFields();

    for (int i = 0; i < highscores.Length; i++)
    {
        if (i >= highscoreFields.Length) yield break;
        highscoreFields[i].text = (i + 1) + "") "" + highscores[i].name + "" | "" + highscores[i].highscore;
    }
}

public void NewHighscore(string username, int highscore) => StartCoroutine(UploadHighscores(username, highscore));

private IEnumerator UploadHighscores(string username, int highscore)
{
    var request = UnityWebRequest.Post(""https://leaderboard-creator.danqzq.repl.co/upload"", new List<IMultipartFormSection>
    {
        Form(""secretKey"", `),
        Form(""name"", username),
        Form(""highscore"", highscore.ToString())
    });

    yield return request.SendWebRequest();
    
    if (!string.IsNullOrEmpty(request.downloadHandler.error))
        Debug.Log(request.downloadHandler.error);
}

private static IMultipartFormSection Form(string formName, string data) => new MultipartFormDataSection(formName, data);

private void EmptyHighscoreFields()
{
    foreach (var t in highscoreFields) t.text = "";
}";
    }
}