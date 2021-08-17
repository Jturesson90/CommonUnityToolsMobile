using System;

namespace Drolegames.Achievements
{
    [Serializable]
    public struct DroleAchievement
    {
        public double increment;
        public string id;
        public bool hasIncrement;
        public DroleAchievement(string id, double increment = -1)
        {
            this.id = id;
            this.increment = increment;
            hasIncrement = increment > 0;
        }
    }
}
