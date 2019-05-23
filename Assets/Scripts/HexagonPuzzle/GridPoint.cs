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
        [SerializeField]
        private Piece piece;
        public Piece Piece
        #region Property
        {
            get => piece;
            set
            {
                if(value != null)
                    value.GridPoint = this;
                piece = value;
            }
        }
        #endregion

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
            {
                localPosition = new Vector3(x * Grid.PieceScale.x * 0.725f, y * Grid.PieceScale.y);
                localStartPosition = new Vector3(x * Grid.PieceScale.x * 0.725f, Grid.Size.y + LocalPosition.y);
            }
            else
            {
                localPosition = new Vector3(x * Grid.PieceScale.x * 0.725f, (y * Grid.PieceScale.y) - (Grid.PieceScale.y / 2));
                localStartPosition = new Vector3(x * Grid.PieceScale.x * 0.725f, Grid.Size.y + LocalPosition.y - (Grid.PieceScale.y / 2));
            }
        }

        public static bool GetCommonNeighbor(GridPoint a, GridPoint b, GridPoint excluding, out GridPoint neighbor)
        {
            neighbor = null;

            //If a and b isn't neighbors this is futile!
            if ((Mathf.Abs(a.X - b.X) != 1 && Mathf.Abs(a.Y - b.Y) > 1) || (Mathf.Abs(a.Y - b.Y) != 1 && Mathf.Abs(a.X - b.X) > 1))
                return false;

            GridPoint temp = null;
            if (a.Y == b.Y)
            {
                if (a.IsOdd)
                {
                    
                    if (a.Y > 0)
                    {
                        temp = All[a.X, a.Y - 1];
                        if (temp != excluding)
                        {
                            neighbor = temp;
                            return true;
                        }
                    }

                    if (b.Y < Grid.Instance.Size.y - 1)
                    {
                        temp = All[b.X, b.Y + 1];
                        if (temp != excluding)
                        {
                            neighbor = temp;
                            return true;
                        }
                    }
                }
                else
                {
                    if (a.Y < Grid.Instance.Size.y - 1)
                    {
                        temp = All[a.X, a.Y + 1];
                        if (temp != excluding)
                        {
                            neighbor = temp;
                            return true;
                        }
                    }

                    if (b.Y > 0)
                    {
                        temp = All[b.X, b.Y - 1];
                        if (temp != excluding)
                        {
                            neighbor = temp;
                            return true;
                        }
                    }
                }
            }
            else if(a.X == b.X)
            {
                int lowestY = a.Y < b.Y ? a.Y : b.Y;
                if (a.x > 0)
                {
                    temp = All[a.X - 1, lowestY];
                    if (temp != excluding)
                    {
                        neighbor = temp;
                        return true;
                    }
                }

                if (b.X < Grid.Instance.Size.x - 1)
                {
                    temp = All[b.X + 1, lowestY];
                    if (temp != excluding)
                    {
                        neighbor = temp;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}