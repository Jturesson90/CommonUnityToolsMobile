namespace Drolegames.SocialService
{
    using Drolegames.IO;
    using System;
    using System.Collections;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    public class MockSocial : ISocialService
    {
        public RuntimePlatform Platform => RuntimePlatform.WindowsEditor;

        public bool UserCanSign => true;

        public bool IsLoggedIn { get; private set; }

        public string Name => IsLoggedIn ? "Jesper" : string.Empty;
        public string Greeting => $"Welcome {Name}";
        public string StoreName => "Mock";

        public byte[] CloudData { get; private set; }

        public bool CloudSaveEnabled => true;

        public void Initialize()
        {
            IsLoggedIn = false;
        }

        public void Login(Action<bool> callback)
        {
            UseDelay(1.5f, () => callback?.Invoke(IsLoggedIn = true));
        }

        public void Logout(Action<bool> callback)
        {
            IsLoggedIn = false;
            callback?.Invoke(true);
        }

        public void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback)
        {
            CloudData = data;
            bool success = FileManager.WriteToFile("mock.txt", System.Text.ASCIIEncoding.Default.GetString(data));
            UseDelay(1.5f, () => callback?.Invoke(success));
        }

        public void LoadFromCloud(Action<bool> callback)
        {
            bool success = FileManager.LoadFromFile("mock.txt", out string json);
            if (success)
            {
                CloudData = System.Text.ASCIIEncoding.Default.GetBytes(json);
            }

            Debug.Log("Mock Cloud Load Success? " + success);
            UseDelay(1.5f, () => callback?.Invoke(success));
        }


        void UseDelay(float time, Action callback)
        {
            Task.Run(async delegate
            {
                await Task.Delay(TimeSpan.FromSeconds(time));
                callback?.Invoke();
            });
        }

        public void UnlockAchievement(string achievementId, Action<bool> callback)
        {
            callback(true);
        }

        public void IncrementAchievement(string achievementId, double steps, Action<bool> callback)
        {
            callback(true);
        }

        public void LoadAchievements(Action<IAchievement[]> callback)
        {
            callback(new IAchievement[0]);
        }

        public void ShowAchievementsUI()
        {
            Debug.Log("MockSocial ShowAchievementsUI");

        }
    }
}
