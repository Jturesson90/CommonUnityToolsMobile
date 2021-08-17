using Drolegames.SocialService;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Drolegames.Achievements
{
    public class AchievementButton : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private Image image;
        [SerializeField] private Button button;
        [Header("Settings")]
        public Sprite googlePlaySprite;
        public Sprite iTunesSprite;
        public Sprite mockSprite;

        private void Awake()
        {
            if (!image || !button)
            {
                Debug.LogError(gameObject.name + " is missing scene references");
            }
        }
        private void Start()
        {
            var isAlreadyLoggedIn = SocialManager.IsInitialized && SocialManager.Current.IsLoggedIn;
            button.gameObject.SetActive(isAlreadyLoggedIn);
            if (isAlreadyLoggedIn)
            {
                UpdateImage(SocialManager.Current.Platform);
            }
        }
        private void OnEnable()
        {
            SocialManager.LoggedInChanged += SocialManager_LoggedInChanged;
            button.onClick.AddListener(OnButtonClicked);
        }
        private void OnDisable()
        {
            SocialManager.LoggedInChanged -= SocialManager_LoggedInChanged;
            button.onClick.RemoveListener(OnButtonClicked);
        }
        private void OnButtonClicked()
        {
            if (SocialManager.IsInitialized)
            {
                SocialManager.Current.ShowAchievementsUI();
            }
        }
        private void SocialManager_LoggedInChanged(object sender, SocialManagerArgs e)
        {
            button.gameObject.SetActive(e.IsLoggedIn);
            UpdateImage(e.Platform);
        }
        private void UpdateImage(RuntimePlatform platform)
        {

            if (platform == RuntimePlatform.Android)
            {
                image.sprite = googlePlaySprite;
            }
            else if (platform == RuntimePlatform.Android)
            {
                image.sprite = iTunesSprite;
            }
            else
            {
                image.sprite = mockSprite;
            }
        }
    }
}