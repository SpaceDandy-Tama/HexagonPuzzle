using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexagonPuzzle
{
    public class Bomb : Piece
    {
        public static List<Bomb> All = new List<Bomb>();

        public TextMesh TextMesh;
        public string Text
        #region Property
        {
            get => TextMesh.text;
            set => TextMesh.text = value;
        }
        #endregion
        [System.NonSerialized]
        public int CreatedAtMove;
        [System.NonSerialized]
        public int Countdown;
        public int RemainingMoves => Countdown - (Menu.Instance.NumMoves - CreatedAtMove);

        private static bool exploded;
        public static bool Exploded
        #region Property
        {
            get
            {
                bool returnVal = exploded;
                exploded = false;
                return returnVal;
            }
            set => exploded = value;
        }
        #endregion

        #region Helper Functions
        public static Bomb CreateNew(GridPoint gridPoint)
        {
            Bomb bomb = Piece.CreateNewSprite().AddComponent<Bomb>();
            bomb.gameObject.name = "bomb";
            bomb.transform.localScale = Vector3.one;
            bomb.gameObject.SetActive(true);
            Bomb.All.Add(bomb);

            bomb.SpriteRenderer.sprite = Grid.Instance.BombSprite;
            bomb.SpriteRenderer.sortingOrder = 1;

            //Maybe TextMesh is not such a good choice as it looks like shit.
            GameObject temp = new GameObject("textMesh");
            temp.transform.parent = bomb.transform;
            bomb.TextMesh = temp.AddComponent<TextMesh>();
            bomb.TextMesh.alignment = TextAlignment.Center;
            bomb.TextMesh.anchor = TextAnchor.MiddleCenter;
            bomb.TextMesh.color = Color.white;
            bomb.TextMesh.characterSize = 0.5f;
            bomb.TextMesh.gameObject.GetComponent<MeshRenderer>().sortingOrder = 9;

            gridPoint.Piece = bomb;
            bomb.Color = Grid.Instance.PieceColors[Random.Range(0, Grid.Instance.PieceColors.Length)];
            bomb.TimeActivated = Time.time;
            bomb.transform.localPosition = bomb.GridPosStart;
            bomb.CreatedAtMove = Menu.Instance.NumMoves;
            bomb.Countdown = Random.Range(7, 10);
            bomb.Tick(); //Do this once to display text.

            return bomb;
        }
        #endregion

        public static void TickAllBombs()
        {
            foreach (Bomb bomb in Bomb.All)
            {
                if(bomb == null)
                {
                    //Todo: Fix this
                    Bomb.All.Remove(bomb);
                    continue;
                }

                bomb.Tick();
            }
        }

        private void Tick() => TextMesh.text = RemainingMoves.ToString();

        public static void CheckFuses()
        {
            foreach (Bomb bomb in Bomb.All)
            {
                if (bomb == null)
                {
                    //Todo: Fix this
                    Bomb.All.Remove(bomb);
                    continue;
                }

                if (bomb.RemainingMoves <= 0)
                {
                    Menu.Instance.Restart();
                    Bomb.Exploded = true;
                    return;
                }
            }
        }

    }
}