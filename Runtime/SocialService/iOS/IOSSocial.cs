namespace Drolegames.SocialService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    public class IOSSocial : ISocialService
    {
        public RuntimePlatform Platform => RuntimePlatform.IPhonePlayer;

        public bool UserCanSign => false;

        public bool IsLoggedIn => Social.Active.localUser.authenticated;

        public string Name => Social.Active.localUser.userName;

        public string StoreName => "iCloud";

        public string Greeting => $"Welcome {Name}";

        public byte[] CloudData { get; private set; }

        public bool CloudSaveEnabled => false;

        public void Initialize()
        {
            // Dont need any initialization 
        }
        Dictionary<string, IAchievement> achievementById;
        public void LoadAchievements(Action<IAchievement[]> callback)
        {
            Social.Active.LoadAchievements(a =>
            {
                achievementById = a.ToDictionary(b => b.id);
                callback(a);
            });
        }

        public void LoadFromCloud(Action<bool> callback)
        {
            callback(false);
        }

        public void Login(Action<bool> callback)
        {
            Debug.LogWarning("Trying to login to IOS!");
            Debug.LogWarning("Am I already logged in? " + IsLoggedIn);
            if (IsLoggedIn)
            {
                callback?.Invoke(false);
                return;
            }
            Social.Active.localUser.Authenticate((bool success) =>
            {
                Debug.LogWarning("Authenticate success:" + success);
                callback?.Invoke(success);
            });
        }

        public void Logout(Action<bool> callback)
        {
            /* Can't logout*/
            callback(false);
        }

        public void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback)
        {
            callback(false);
        }

        public void ShowAchievementsUI()
        {
            Social.Active.ShowAchievementsUI();
        }

        public void IncrementAchievement(string achievementId, double steps, Action<bool> callback)
        {
            if (achievementById.ContainsKey(achievementId))
            {
                var achievement = achievementById[achievementId];
                achievement.percentCompleted += steps;
                Social.Active.ReportProgress(achievementId, achievement.percentCompleted, callback);
            }
        }
        public void UnlockAchievement(string achievementId, Action<bool> callback)
        {
            Social.Active.ReportProgress(achievementId, 100d, callback);
        }
    }
}
