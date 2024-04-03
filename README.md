# Official Documentation for Leaderboard Creator

Current latest version of **Leaderboard Creator** is v2.8

## Table of Contents

- [Integration Video Tutorial](#integration-video-tutorial)
- [Importing Leaderboard Creator into your project](#importing-leaderboard-creator-into-your-project)
- [Include this "using" statement in your script where you want to handle leaderboards](#include-this-using-statement-in-your-script-where-you-want-to-handle-leaderboards)
- [Adding your leaderboard to your project](#adding-your-leaderboard-to-your-project)
- [Getting your leaderboard](#getting-your-leaderboard)
- [Uploading and modifying leaderboard entries](#uploading-and-modifying-leaderboard-entries)
- [Display entries](#display-entries)
  - [Displaying entries in a list](#displaying-entries-in-a-list)
  - [Identifying the player's entry](#identifying-the-players-entry)
  - [Rank suffixes](#rank-suffixes)
- [More functions](#more-functions)
  - [Get the player's personal entry](#get-the-players-personal-entry)
  - [Get the total count of entries in the leaderboard](#get-the-total-count-of-entries-in-the-leaderboard)
  - [Reset the player's entry](#reset-the-players-entry)
  - [Toggle Logging](#toggle-logging)
- [Understanding the behaviour of the leaderboard entries](#understanding-the-behaviour-of-the-leaderboard-entries)
  - [Authorization](#authorization)
  - [The Purpose of Unique Identifiers](#the-purpose-of-unique-identifiers)
- [FAQ](#faq)
  - [Where is the data of the leaderboards stored?](#where-is-the-data-of-the-leaderboards-stored)
  - [What if I lost my leaderboard or its secret key?](#what-if-i-lost-my-leaderboard-or-its-secret-key)
  - [My leaderboard entry keeps getting overridden / I can only upload one entry. Why is that?](#my-leaderboard-entry-keeps-getting-overridden--i-can-only-upload-one-entry-why-is-that)
  - [Does the score field in the entry store integers only?](#does-the-score-field-in-the-entry-store-integers-only)
  - [Why is my leaderboard "frozen"?](#why-is-my-leaderboard-frozen)


  ---

## Integration Video Tutorial

If you prefer to watch a video tutorial on how to integrate your leaderboard into your project, click the image below to watch it on YouTube:

[![Watch the video](https://img.youtube.com/vi/v0aWwSkC-4o/0.jpg)](https://www.youtube.com/watch?v=v0aWwSkC-4o)

  ---

## Importing Leaderboard Creator into your project

Visit the [itch.io page](https://danqzq.itch.io/leaderboard-creator) to download the Unity package containing Leaderboard Creator.

  ---

## Include this "using" statement in your script where you want to handle leaderboards
```c#
using Dan.Main;
```
  ---

## Adding your leaderboard to your project

To add your leaderboard to your project, follow these steps:

1. Once you import the Unity package, click the **Leaderboard Creator** button on the toolbar
2. Press **Enter New Leaderboard** and enter the credentials for your leaderboard
3. Click **Add Leaderboard** then **Save to C# Script**
4. You will now have a new C# script in your project that contains a reference to your leaderboard by its name

> [!NOTE]
> For the rest of this documentation, we will refer to your leaderboard as **YourLeaderboard**. Replace this with the name of your actual leaderboard.

  ---

## Getting your leaderboard
• To **get** the entries in your leaderboard, call this function:
```c#
Leaderboards.YourLeaderboard.GetEntries(string publicKey, Action<Entry[]> callback, Action<string> errorCallback[optional])
```

• If the function is called and the request is successful, the callback is called, an array of `Entry` objects is returned.

An `Entry` object contains:
```c#
[System.Serializable]
public struct Entry
{
    public string Username { get; set; }
    public int Score { get; set; }
    public int Rank { get; set; }
    public ulong Date { get; set; }
    public string Extra { get; set; } 
    ...
}
```
> [!NOTE]
> - **Date** - Unix timestamp of the last time the entry was uploaded/modified
> - **Extra** - A string value for metadata, you can use this however you like depending on your game. But make sure that the length of the string does not go over 100, otherwise it will get truncated and not all your data will be saved.

  ---

• This function also has other overloads that can give you more control over the querying of your leaderboard's entries:
```c#
Leaderboards.YourLeaderboard.GetEntries(string publicKey, bool isInAscendingOrder, Action<Entry[]> callback, Action<string> errorCallback[optional])
Leaderboards.YourLeaderboard.GetEntries(string publicKey, LeaderboardSearchQuery searchQuery, Action<Entry[]> callback, Action<string> errorCallback[optional])
Leaderboards.YourLeaderboard.GetEntries(string publicKey, bool isInAscendingOrder, LeaderboardSearchQuery searchQuery, Action<Entry[]> callback, Action<string> errorCallback[optional])
```

> [!NOTE]
> - **isInAscendingOrder** - If true, the entries are sorted in ascending order, otherwise they are sorted in descending order.
> - **searchQuery** - A `LeaderboardSearchQuery` object that can be used to filter the entries you want to get.
> - **callback** - A function that is called when the request is successful, returning an array of Entry objects.
> - **errorCallback** - A function that is called when the request is unsuccessful.

  ---

• A `LeaderboardSearchQuery` object consists of:
```c#
public struct LeaderboardSearchQuery
{
    public int Skip { get; set; } //amount of entries to skip
    public int Take { get; set; } //amount of entries to take
    public string Username { get; set; }
    public TimePeriodType TimePeriod { get; set; }
}
```
> [!IMPORTANT]
> - You do not have to fill out all of these fields in the query.
> - There are several functions that can generate a `LeaderboardSearchQuery` for your exact needs:
> ```c#
> LeaderboardSearchQuery.Paginated(int skip, int take)
>
> LeaderboardSearchQuery.ByUsername(string username)
>         
> LeaderboardSearchQuery.ByUsernamePaginated(string username, int prev, int next)
>         
> LeaderboardSearchQuery.ByTimePeriod(TimePeriodType timePeriod)
>         
> LeaderboardSearchQuery.ByTimePeriodPaginated(TimePeriodType timePeriod, int skip, int take)
> 
> LeaderboardSearchQuery.ByUsernameAndTime(string username, TimePeriodType timePeriod)
> ```

> [!NOTE]
> **TimePeriodType** is an enum that consists of the following values:
> - **AllTime** - All entries
> - **Today** - Entries from today
> - **ThisWeek** - Entries from this week
> - **ThisMonth** - Entries from this month
> - **ThisYear** - Entries from this year
> 
> This enum comes from a namespace called `Dan.Enums`, so you will need to include it in your script.

  ---

## Uploading and modifying leaderboard entries
• To **upload** a new entry into your leaderboard, call this function:
```c#
Leaderboards.YourLeaderboard.UploadNewEntry(string username, int score, Action<bool> callback, Action<string> errorCallback[optional])
```
• To **upload** a new entry, with an extra field, into your leaderboard, call this function:
```c#
Leaderboards.YourLeaderboard.UploadNewEntry(string username, int score, string extra, Action<bool> callback, Action<string> errorCallback[optional])
```
• To **delete** the player's entry, call this function:
```c#
Leaderboards.YourLeaderboard.DeleteEntry(Action<bool> callback[optional], Action<string> errorCallback[optional])
```

> [!NOTE]
> Here are some of the parameter descriptions:
> - callback - A function that is called when the request is successful, returning a boolean value.
> - errorCallback - An optional function that is called when the request is unsuccessful.

> [!IMPORTANT]
> - If the player previously uploaded an entry, the new uploaded entry should override the old one **if it has a higher score**. This is intended behaviour, as it prevents players from spamming the leaderboard with lower scores. Read more about this in the FAQ section.

  ---

## Display entries

### Displaying entries in a list

Here is an example of how you can display the entries in a list using Unity's UI system:

```c#
...

// A prefab containing a TextMeshPro component:
[SerializeField] private GameObject entryPrefab;

// This transform will be the parent of the instantiated prefabs
// Typically, this would be the "Content" object of a ScrollView
// You may want to add a VerticalLayoutGroup component to this object for easy and automatic alignment
[SerializeField] private Transform entryParent;

private void Start()
{
    LoadEntries();
}

private void LoadEntries()
{
    Leaderboards.YourLeaderboard.GetEntries(OnEntriesLoaded, OnError);
}

private void OnEntriesLoaded(Entry[] entries)
{
    foreach (Entry entry in entries)
    {
        GameObject entryObject = Instantiate(entryPrefab, entryParent);
        TextMeshProUGUI textObject = entryObject.GetComponent<TextMeshProUGUI>();
        textObject.text = $"{entry.Rank}. {entry.Username} - {entry.Score}";
    }
}

private void OnError(string error)
{
    Debug.LogError(error);
}

...
```

### Identifying the player's entry

The `Entry` object contains a built-in method that can be used to check if the entry belongs to the player. We will build on the previous example to show how you can identify the player's entry:

```c#
private void OnEntriesLoaded(Entry[] entries)
{
    foreach (Entry entry in entries)
    {
        GameObject entryObject = Instantiate(entryPrefab, entryParent);
        TextMeshProUGUI textObject = entryObject.GetComponent<TextMeshProUGUI>();
        textObject.text = $"{entry.Rank}. {entry.Username} - {entry.Score}";
        
        // Returns true if the entry belongs to the player
        bool isMine = entry.IsMine();
        
        if (isMine)
        {
            textObject.color = Color.green;
        }
    }
}
```


### Rank suffixes

If you want to add suffixes to the rank numbers (e.g. 1st, 2nd, 3rd, 4th, etc.), the `Entry` object has a built-in utility method that can be used to get the rank suffix. Here is an example of how you can use it:


```c#
private void OnEntriesLoaded(Entry[] entries)
{
    foreach (Entry entry in entries)
    {
        GameObject entryObject = Instantiate(entryPrefab, entryParent);
        TextMeshProUGUI textObject = entryObject.GetComponent<TextMeshProUGUI>();
        
        string rankSuffix = entry.GetRankSuffix();
        textObject.text = $"{rankSuffix}. {entry.Username} - {entry.Score}";
        
        // Returns true if the entry belongs to the player
        bool isMine = entry.IsMine();
        
        if (isMine)
        {
            textObject.color = Color.green;
        }
    }
}
```

  ---

## More functions

### Get the player's personal entry

```c#
Leaderboards.YourLeaderboard.GetPersonalEntry(Action<Entry> callback, Action<string> errorCallback[optional])
```

> [!NOTE]
> Here are the parameter descriptions:
> - callback - A function that is called when the request is successful, returning an Entry object.
> - errorCallback - A function that is called when the request is unsuccessful.

> [!IMPORTANT]
> - If the player has not uploaded their entry, the callback will return an Entry object with a rank and score of 0.

### Get the total count of entries in the leaderboard

```c#
Leaderboards.YourLeaderboard.GetEntryCount(Action<int> callback, Action<string> errorCallback[optional])
```

> [!NOTE]
> Here are the parameter descriptions:
> - callback - A function that is called when the request is successful, returning the total count of entries in the leaderboard.
> - errorCallback - A function that is called when the request is unsuccessful.

### Reset the player's entry

Want players to be able to submit more than one entry? Use this function to reset the player's entry, so they can submit a new one:

```c#
Leaderboards.YourLeaderboard.ResetPlayer(Action<bool> callback, Action<string> errorCallback[optional])
```

> [!NOTE]
> Here are the parameter descriptions:
> - callback - A function that is called when the request is successful, returning the total count of entries in the leaderboard.
> - errorCallback - A function that is called when the request is unsuccessful.

> [!IMPORTANT]
> - This function will only work correctly if the leaderboard's "Unique Usernames" setting is disabled.
> - After this function is called, a player's previous entry cannot be edited nor deleted by the player themselves.

### Toggle Logging
Want to disable logging for your leaderboard? Use the following line of code:
```c#
LeaderboardCreator.LoggingEnabled = false;
```

  ---

## Understanding the behaviour of the leaderboard entries

### Authorization
Each entry in the leaderboard is identified by a unique identifier, which is assigned to the player upon authorization. By default, authorization occurs automatically upon the initialization of your game.

### The Purpose of Unique Identifiers

Using this identifier, the player can upload, edit, and delete their entry in the leaderboard. Without this identifier, the player will not be able to perform any of these actions. For security reasons, the player only has access to their own unique identifier, so they can only modify their own entry in the leaderboard.

  ---

<details>
<summary>Alternative way to use Leaderboard Creator functions</summary>

## Getting your leaderboard
• To **get** the entries in your leaderboard, call this function:
```c#
LeaderboardCreator.GetLeaderboard(string publicKey, Action<Entry[]> callback)
```

  ---

• This function also has other overloads that can give you more control over the querying of your leaderboard's entries:
```c#
LeaderboardCreator.GetLeaderboard(string publicKey, bool isInAscendingOrder, Action<Entry[]> callback, Action<string> errorCallback[optional])
LeaderboardCreator.GetLeaderboard(string publicKey, LeaderboardSearchQuery searchQuery, Action<Entry[]> callback, Action<string> errorCallback[optional])
LeaderboardCreator.GetLeaderboard(string publicKey, bool isInAscendingOrder, LeaderboardSearchQuery searchQuery, Action<Entry[]> callback, Action<string> errorCallback[optional])
```

  ---

## Uploading and modifying leaderboard entries
• To **upload** a new entry into your leaderboard, call this function:
```c#
LeaderboardCreator.UploadNewEntry(string publicKey, string username, int score, Action<bool> callback, Action<string> errorCallback[optional])
```
• To **upload** a new entry, with an extra field, into your leaderboard, call this function:
```c#
LeaderboardCreator.UploadNewEntry(string publicKey, string username, int score, string extra, Action<bool> callback, Action<string> errorCallback[optional])
```
• To **delete** the player's entry, call this function:
```c#
LeaderboardCreator.DeleteEntry(string publicKey, Action<bool> optionalCallback)
```

## More functions

### Get the player's personal entry

```c#
LeaderboardCreator.GetPersonalEntry(string publicKey, Action<Entry> callback, Action<string> errorCallback[optional])
```

### Get the total count of entries in the leaderboard

```c#
LeaderboardCreator.GetEntryCount(string publicKey, Action<int> callback, Action<string> errorCallback[optional])
```

### Reset the player's entry

```c#
LeaderboardCreator.ResetPlayer(string publicKey, Action<bool> callback, Action<string> errorCallback[optional])
```

  ---

</details>

## FAQ

### Where is the data of the leaderboards stored?
All the data is stored in a cloud-based database.

### What if I lost my leaderboard or its secret key?
If you lose your leaderboard's secret key by accident but you still have your public key, contact me on Discord: @danqzq

Alternatively, you can email me at: dan@danqzq.games

### My leaderboard entry keeps getting overridden / I can only upload one entry. Why is that?
There may only be one entry per one player. This is done to support features like name editing and deleting an entry in a safe way, such that other people don't mess around with other people's entries. If you do not want this, then disable "Unique Usernames" for your leaderboard and make use of the `Leaderboards.YourLeaderboard.ResetPlayer()` function during the callback from uploading an entry. Note that after this, a player's previous entry cannot be edited nor deleted by the player themselves.

### Does the score field in the entry store integers only?
Yes. If you want to store decimal numbers as the scores, you can either:

A. Before submitting an entry, multiply the score of the entry by 100, 1000 or etc. depending on amount of precision you want. When receiving entry data, divide each score by the same amount you multiplied the scores, and only then display the score for each entry.

B. Make use of the Extra field, for storing extra data, and perform manual sorting of the entries appropriately to your liking.

### Why is my leaderboard "frozen"?
If an error is shown saying that your leaderboard is frozen, it is likely due to a high number of requests at the same time. You will be able to fetch the leaderboard entries, but new entries will not be submitted until the leaderboard will unfreeze, which should happen within 24 hours from the moment it entered a frozen state.
