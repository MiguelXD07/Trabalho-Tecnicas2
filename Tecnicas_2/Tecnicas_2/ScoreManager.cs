using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Tecnicas_2
{
    public static class ScoreManager
    {
        private static int _score = 0;
        private static int _highScore = 0;
        private static readonly string ScoreFile = "highscore.txt";

        static ScoreManager()
        {
            LoadHighScore();
        }

        public static int Score => _score;
        public static int HighScore => _highScore;

        public static void AddScore(int amount)
        {
            _score += amount;
            if (_score > _highScore)
            {
                _highScore = _score;
                SaveHighScore();
            }
        }

        public static void ResetScore()
        {
            _score = 0;
        }

        private static void LoadHighScore()
        {
            if (File.Exists(ScoreFile))
            {
                int.TryParse(File.ReadAllText(ScoreFile), out _highScore);
            }
        }

        private static void SaveHighScore()
        {
            File.WriteAllText(ScoreFile, _highScore.ToString());
        }
    }
}
