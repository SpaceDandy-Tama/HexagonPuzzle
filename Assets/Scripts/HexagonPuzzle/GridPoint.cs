using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexagonPuzzle
{
    [System.Serializable]
    public class GridPoint
    {
        //Multi Dimension Arrays are not serializable so this only works during runtime. For editor use see GridPoint.GetAt()
        public static GridPoint[,] All;

        public Grid Grid;
        public Piece Piece;

        [SerializeField]
        private int x;
        public int X => x;
        [SerializeField]
        private int y;
        public int Y => y;

        //Bottom left corner is (0, 0)
        [SerializeField]
        private Vector3 localPosition;
        public Vector3 LocalPosition => localPosition; //I did this because readonly fields are not serialized
        public Vector3 WorldPosition => Grid.Instance.transform.TransformPoint(localPosition);
        [SerializeField]
        private Vector3 localStartPosition;
        public Vector3 LocalStartPosition => localStartPosition;

        public bool IsOdd => X % 2 > 0 ? true : false;

        public GridPoint(Grid grid, int x, int y)
        {
            this.Grid = grid;
            this.x = x;
            this.y = y;

            if (IsOdd)
                localPosition = new Vector3(x * Grid.PieceScale.x * 0.725f, y * Grid.PieceScale.y);
            else
                localPosition = new Vector3(x * Grid.PieceScale.x * 0.725f, (y * Grid.PieceScale.y) - (Grid.PieceScale.y / 2));

            localStartPosition = new Vector3(x * Grid.PieceScale.x * 0.725f, Grid.Size.y + LocalPosition.y); //BUG at y
        }
    }
}