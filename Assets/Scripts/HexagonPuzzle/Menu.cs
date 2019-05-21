using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace HexagonPuzzle
{
    public class Menu : MonoBehaviour
    {
        public static Menu Instance;

        public Text TextScore;
        public Text TextNumMoves;

        private int score = 0;
        public int Score
        #region Property
        {
            get => score;
            set => TextScore.text = (score = value).ToString();
        }
        #endregion
        private int numMoves = 0;
        public int NumMoves
        #region Property
        {
            get => numMoves;
            set
            {
                TextNumMoves.text = (numMoves = value).ToString();
                Bomb.TickAllBombs();
            }
        }
        #endregion

        private void Awake() => Menu.Instance = this;

        //Todo: Implement reseting static variables just in case
        public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }
}