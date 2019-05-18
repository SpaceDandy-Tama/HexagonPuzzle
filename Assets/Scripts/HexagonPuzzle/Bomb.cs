using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexagonPuzzle
{
    public class Bomb : Piece
    {
        public static List<Bomb> BombList = new List<Bomb>();

        #region Helper Functions
        public static Bomb CreateNew(GridPoint gridPoint)
        {
            Bomb bomb = Piece.CreateNewSprite().AddComponent<Bomb>();
            bomb.SpriteRenderer.sprite = Grid.Instance.BombSprite;
            bomb.GridPoint = gridPoint;

            return bomb;
        }
        #endregion
    }
}