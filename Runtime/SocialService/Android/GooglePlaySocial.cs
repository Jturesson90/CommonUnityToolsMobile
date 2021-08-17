#if UNITY_ANDROID

namespace Drolegames.SocialService
{
    using GooglePlayGames;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.SavedGame;
    using System;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    public class GooglePlaySocial : ISocialService
    {
        public RuntimePlatform Platform => RuntimePlatform.Android;

        public bool UserCanSign => true;

        public bool IsLoggedIn => PlayGamesActive.localUser.authenticated;

        public string Name => PlayGamesActive.localUser.userName;
        public string Greeting => $"Welcome {Name}";
        public string StoreName => "Google Play Games";

        public byte[] CloudData { get; private set; }

        private const string CloudFileName = "SeaOfBombs_save1";
        private PlayGamesPlatform PlayGamesActive => (PlayGamesPlatform)Social.Active;

        public bool CloudSaveEnabled => true;

        public void Initialize()
        {
            Debug.LogWarning("GooglePlaySocial Initialize");
            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration
                .Builder()
                .EnableSavedGames()
                .Build();

            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.Activate();
        }
        public void LoadFromCloud(Action<bool> callback)
        {
            Debug.LogWarning("GooglePlaySocial LoadFromCloud");
            if (loadingFromCloud || !IsLoggedIn)
            {
                Debug.LogWarning("GooglePlaySocial loading or is not LoggedIn");
                LoadComplete(false);
                return;
            }
            loadingFromCloud = true;
            loadCallback = callback;
            Debug.LogWarning("GooglePlaySocial ShowSelectSavedGameUI begin");

            PlayGamesActive.SavedGame.OpenWithAutomaticConflictResolution(CloudFileName,
                            DataSource.ReadCacheOrNetwork,
                            ConflictResolutionStrategy.UseLongestPlaytime,
                            SavedGameOpened);
        }


        public void Login(Action<bool> callback)
        {
            Debug.LogWarning("Trying to login to Google!");
            Debug.LogWarning("Am I already logged in? " + IsLoggedIn);
            if (IsLoggedIn)
            {
                callback?.Invoke(false);
                return;
            }
            Social.localUser.Authenticate((bool success) =>
            {
                Debug.LogWarning("Authenticate success:" + success);
                callback?.Invoke(success);
            });
        }

        public void Logout(Action<bool> callback)
        {
            ((PlayGamesPlatform)Social.Active).SignOut();
            callback?.Invoke(true);
        }
        Texture2D screenImage;
        public void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback)
        {
            Debug.LogWarning("GooglePlaySocial SaveGame playedTime " + playedTime.TotalMinutes);
            if (savingToCloud)
            {
                SaveComplete(false);
            };
            saveCallback = callback;
            savingToCloud = true;
            Debug.LogWarning("GooglePlaySocial SaveGame filename " + CloudFileName);

            PlayGamesActive.SavedGame.OpenWithAutomaticConflictResolution(CloudFileName, DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime,
              (SavedGameRequestStatus status, ISavedGameMetadata game) =>
              {
                  if (status != SavedGameRequestStatus.Success)
                  {
                      Debug.LogWarning("GooglePlaySocial OpenWithAutomaticConflictResolution Failed");
                      SaveComplete(false);
                      return;
                  }
                  SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder()
                       .WithUpdatedPlayedTime(playedTime)
                       .WithUpdatedDescription("Saved Game at " + DateTime.Now);


                  Debug.LogWarning("GooglePlaySocial OpenWithAutomaticConflictResolution builder done");
                  Debug.LogWarning("GooglePlaySocial DATA we send " + data);
                  Debug.LogWarning("GooglePlaySocial DATA we send json" + GameSaveData.FromBytes(data).ToString());

                  SavedGameMetadataUpdate updatedMetadata = builder.Build();

                  PlayGamesActive.SavedGame
                    .CommitUpdate(game, updatedMetadata, data,
                         (SavedGameRequestStatus committedStatus, ISavedGameMetadata committedGame) =>
                         {
                             Debug.LogWarning("GooglePlaySocial SavedGameRequestStatus " + committedStatus.ToString() + " " + committedGame.Description + " commitedGane: " + committedGame.Filename);
                             SaveComplete(committedStatus == SavedGameRequestStatus.Success);
                         }
                    );
              }
              );


        }

        Action<bool> saveCallback;
        bool savingToCloud = false;
        private void SaveComplete(bool success)
        {
            savingToCloud = false;
            if (saveCallback != null)
            {
                saveCallback.Invoke(success);
            }
            saveCallback = null;
        }

        Action<bool> loadCallback;
        bool loadingFromCloud = false;
        private void LoadComplete(bool success)
        {
            loadingFromCloud = false;
            if (loadCallback != null)
            {
                loadCallback.Invoke(success);
            }
            loadCallback = null;
        }


        private void SavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata game)
        {
            Debug.LogWarning($"GooglePlaySocial SavedGameOpened {status} {game.Description}");
            if (status == SavedGameRequestStatus.Success)
            {
                PlayGamesActive.SavedGame.ReadBinaryData(game, SavedGameLoaded);
            }
            else
            {
                Debug.LogWarning($"GooglePlaySocial SavedGameOpened Fail");
                LoadComplete(false);
            }
        }

        private void SavedGameLoaded(SavedGameRequestStatus status, byte[] data)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                CloudData = data;
                LoadComplete(true);
            }
            else
            {
                LoadComplete(false);
            }
        }

        public void UnlockAchievement(string achievementId, Action<bool> callback)
        {
            PlayGamesActive.UnlockAchievement(achievementId, callback);
        }

        public void IncrementAchievement(string achievementId, double steps, Action<bool> callback)
        {
            PlayGamesActive.IncrementAchievement(achievementId, (int)steps, callback);
        }

        public void LoadAchievements(Action<IAchievement[]> callback)
        {
            PlayGamesActive.LoadAchievements(callback);
        }

        public void ShowAchievementsUI()
        {
            PlayGamesActive.ShowAchievementsUI();
        }
    }
}
#endif