using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexagonPuzzle
{
    public class GridJunction
    {
        public Grid Grid;

        public GridPoint[] GridPoints = new GridPoint[3];

        [SerializeField]
        private int x;
        public int X => x;
        [SerializeField]
        private int y;
        public int Y => y;

        [SerializeField]
        private Vector3 localPosition;
        public Vector3 LocalPosition => localPosition;
        public Vector3 WorldPosition => Grid.Instance.transform.TransformPoint(localPosition);

        public bool IsOdd => X % 2 > 0 ? true : false;

        public GridJunction(Grid grid, int x, int y)
        {
            Grid = grid;
            this.x = x;
            this.y = y;

            int gridX = x / 2;
            if (IsOdd)
            {
                GridPoints[0] = GridPoint.All[gridX, gridX % 2 > 0 ? y : y + 1];
                GridPoints[1] = GridPoint.All[gridX + 1, y];
                GridPoints[2] = GridPoint.All[gridX + 1, y + 1];
            }
            else
            {
                GridPoints[0] = GridPoint.All[gridX + 1, gridX % 2 > 0 ? y + 1 : y];
                GridPoints[1] = GridPoint.All[gridX, y];
                GridPoints[2] = GridPoint.All[gridX, y + 1];
            }

            localPosition = new Vector3((GridPoints[0].LocalPosition.x + GridPoints[1].LocalPosition.x) / 2, GridPoints[0].LocalPosition.y);
        }
    }
}