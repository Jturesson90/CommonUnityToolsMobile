
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Drolegames.SocialService
{
    public interface ISocialService
    {
        void Initialize();
        void Login(Action<bool> callback);
        void Logout(Action<bool> callback);
        void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback);
        void LoadFromCloud(Action<bool> callback);
        void LoadAchievements(Action<IAchievement[]> callback);
        void UnlockAchievement(string achievementId, Action<bool> callback);
        void IncrementAchievement(string achievementId, double steps, Action<bool> callback);
        void ShowAchievementsUI();
        RuntimePlatform Platform { get; }
        bool UserCanSign { get; }
        bool CloudSaveEnabled { get; }
        bool IsLoggedIn { get; }
        string Name { get; }
        string StoreName { get; }
        string Greeting { get; }
        byte[] CloudData { get; }
    }
}
