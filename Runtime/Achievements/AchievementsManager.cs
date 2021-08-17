//#define DEBUG_ACHIEVEMENTS
namespace Drolegames.Achievements
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;
    using Drolegames.SocialService;

    public class AchievementsManager : MonoBehaviour
    {

        private PendingAchievements _pendingAchievements;
        private Dictionary<string, IAchievement> unlockedAchievements = new Dictionary<string, IAchievement>();
        private Dictionary<string, IAchievement> allAchievements = new Dictionary<string, IAchievement>();

        private void FlushAchievements()
        {
            if (SocialManager.IsInitialized && SocialManager.Current.IsLoggedIn)
            {
                foreach (var pending in _pendingAchievements.pending)
                {
                    if (pending.hasIncrement)
                    {
                        IncrementAchievement(pending.id, pending.increment);
                    }
                    else
                    {
                        UnlockAchievement(pending.id);
                    }
                }
            }
        }
        public void IncrementAchievement(string id, double steps)
        {

#if DEBUG_ACHIEVEMENTS
            Debug.Log($"IncrementAchievement {id} {steps} SocialManager.IsInitialized: {SocialManager.IsInitialized } SocialManager.Current.IsLoggedIn{SocialManager.Current.IsLoggedIn}");
#endif
            if (!allAchievements.ContainsKey(id) || unlockedAchievements.ContainsKey(id)) return;

            var s = new DroleAchievement() { id = id, hasIncrement = true, increment = steps };
            if (!SocialManager.IsInitialized || !SocialManager.Current.IsLoggedIn)
            {
                AddPendingAchievement(s);
                return;
            }

            if (SocialManager.IsInitialized && SocialManager.Current.IsLoggedIn)
            {
                SocialManager.Current.IncrementAchievement(id, steps, (bool success) =>
                {
#if DEBUG_ACHIEVEMENTS
                    Debug.Log($"IncrementAchievement success ? {success}");
#endif
                    if (success)
                    {
                        RemovePendingAchievement(s);
                    }
                    else
                    {
                        AddPendingAchievement(s);
                    }
                });
            }
        }

        private void RemovePendingAchievement(DroleAchievement pendingAchievement)
        {
            int index = _pendingAchievements.pending.FindIndex(c => c.id.Equals(pendingAchievement.id) && c.hasIncrement == pendingAchievement.hasIncrement && c.increment.Equals(pendingAchievement.increment));
            if (index >= 0)
                _pendingAchievements.pending.RemoveAt(index);
        }
        private void AddPendingAchievement(DroleAchievement pendingAchievement)
        {
            if (!_pendingAchievements.pending.Any(c => c.id.Equals(pendingAchievement.id) && c.hasIncrement == pendingAchievement.hasIncrement && c.increment.Equals(pendingAchievement.increment)))
            {
                _pendingAchievements.pending.Add(pendingAchievement);
            }
        }
        public void UnlockAchievement(string id)
        {
#if DEBUG_ACHIEVEMENTS
            Debug.Log($"UnlockAchievement {id}");
#endif
            if (!allAchievements.ContainsKey(id) || unlockedAchievements.ContainsKey(id)) return;
            var s = new DroleAchievement() { id = id, hasIncrement = false };
            if (!SocialManager.IsInitialized || !SocialManager.Current.IsLoggedIn)
            {
                AddPendingAchievement(s);
                return;
            }

            SocialManager.Current.UnlockAchievement(id, (bool success) =>
            {
#if DEBUG_ACHIEVEMENTS
                Debug.Log($"UnlockAchievement success ? {success}");
#endif
                if (success)
                {
                    unlockedAchievements.Add(id, allAchievements[id]);
                    RemovePendingAchievement(s);
                }
                else
                {
                    AddPendingAchievement(s);
                }
            });


        }
        private void Start()
        {
            if (SocialManager.IsInitialized && SocialManager.Current.IsLoggedIn)
            {
                LoadAchievements();
            }
        }
        private void OnEnable()
        {
            SocialManager.LoggedInChanged += SocialManager_LoggedInChanged;
            LoadFromDisk();
        }
        private void OnDisable()
        {
            SocialManager.LoggedInChanged -= SocialManager_LoggedInChanged;
            SaveToDisk();
        }
        private void LoadAchievements()
        {
#if DEBUG_ACHIEVEMENTS
            Debug.Log($"LoadAchievements");
#endif
            unlockedAchievements.Clear();
            allAchievements.Clear();
            SocialManager.Current.LoadAchievements(achievements =>
            {
                allAchievements = achievements.ToDictionary(a => a.id);
                unlockedAchievements = achievements
                .Where(a => a.completed)
                .ToDictionary(a => a.id);
#if DEBUG_ACHIEVEMENTS
                Debug.Log($"LoadAchievements allAchievements count {allAchievements.Count}");
                Debug.Log($"LoadAchievements unlockedAchievements count {unlockedAchievements.Count}");
#endif
                for (int i = _pendingAchievements.pending.Count - 1; i >= 0; i--)
                {
                    if (unlockedAchievements.ContainsKey(_pendingAchievements.pending[i].id))
                    {
                        _pendingAchievements.pending.RemoveAt(i);
                    }
                }

                FlushAchievements();
            });

        }
        private void SocialManager_LoggedInChanged(object sender, SocialManagerArgs e)
        {
            if (e.IsLoggedIn)
            {
                LoadAchievements();

            }
        }
        const string saveKey = "pend";
        private void SaveToDisk()
        {
            var json = _pendingAchievements.ToString();
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.Save();
        }
        private void LoadFromDisk()
        {
            var s = PlayerPrefs.GetString(saveKey, string.Empty);
            if (s == null || s.Trim().Length == 0)
            {
                _pendingAchievements = new PendingAchievements();
            }
            else
            {
                _pendingAchievements = PendingAchievements.FromString(s);
            }
        }
    }
    [Serializable]
    public class PendingAchievements
    {
        public List<DroleAchievement> pending;
        public static PendingAchievements FromString(string s) => JsonUtility.FromJson<PendingAchievements>(s);
        public override string ToString() => JsonUtility.ToJson(this, false);
        public PendingAchievements()
        {
            pending = new List<DroleAchievement>();
        }
    }
}
