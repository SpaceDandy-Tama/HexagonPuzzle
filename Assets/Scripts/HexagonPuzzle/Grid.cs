#define DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexagonPuzzle
{
    public class Grid : MonoBehaviour
    {
        public static Grid Instance;

        [Header("Grid Settings")]
        public Vector2Int Size = new Vector2Int(8, 9);
        public Color[] PieceColors = new Color[5] { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta };
        public Vector3 PieceScale = new Vector3(1, 1, 1); //Todo: Make this automatic based on Grid Size
        public int BombAppearanceScore = 1000;
        public float PieceActivationInterval = 0.016666f;

        [Header("Sprite Settings")]
        public Sprite PieceSprite;
        public Sprite BombSprite;

        public int PieceCount => PieceColors.Length;

        [HideInInspector]
        public GridPoint[] GridPoints;
        [HideInInspector]
        public Piece[] Pieces;
        [System.NonSerialized]
        public GridJunction[,] GridJunctions;
        [System.NonSerialized]
        public Selection Selection;
        [System.NonSerialized]
        private Vector3 LastClickPosition;
        [System.NonSerialized]
        public bool GameReady = false;

        public void RemoveGrid()
        {
            if (Pieces != null)
            {
                for (int i = 0; i < Pieces.Length; i++)
                {
                    if (Pieces[i] != null)
                        DestroyImmediate(Pieces[i].gameObject);
                }
                Pieces = null;
            }
            GridPoints = null;
        }

        public void GenerateGrid()
        {
            //Clean up old generated grid if it exists
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                RemoveGrid();
                Grid.Instance = this;
            }
#endif

            //Set up Grid origin based on Grid Size
            transform.position = new Vector3((-((Size.x - 1) * PieceScale.x * 0.725f) / 2), (-((Size.y) * PieceScale.y) / 2) + 0.75f);

            //Initialize Piece array;
            Pieces = new Piece[Size.x * Size.y];

            //Populate Piece Object Pool
            for (int i = 0; i < Pieces.Length; i++)
                Piece.CreateNew(i);

            //Initialize GridPoint array;
            GridPoints = new GridPoint[Size.x * Size.y];

            //Instantiate GridPoints based on Grid Size
            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    GridPoints[x + (y * Size.x)] = new GridPoint(this, x, y);
                    if(!Application.isPlaying)
                        Pieces[x + (y * Size.x)].Activate(GridPoints[x + (y * Size.x)], true);
                }
            }
        }

        public bool CheckForExplosion(GridJunction gridJunction = null)
        {
            //Assign values for default loop
            int xStart = 0;
            int yStart = 0;
            int xLength = GridJunctions.GetLength(0);
            int yLength = GridJunctions.GetLength(1);

            //Modify loop values to only check for the given junction and its neighbors to avoid unnecessary calculations
            if (gridJunction != null)
            {
                xStart = gridJunction.X > 1 ? gridJunction.X - 2 : xStart;
                yStart = gridJunction.Y > 1 ? gridJunction.Y - 2 : yStart;
                xLength = gridJunction.X < xLength - 2 ? gridJunction.X + 3 : xLength;
                yLength = gridJunction.Y < yLength - 2 ? gridJunction.Y + 3 : yLength;
            }

            for (int x = xStart; x < xLength; x++)
            {
                for (int y = yStart; y < yLength; y++)
                {
                    if (GridJunctions[x, y].GridPoints[0].Piece.ColorIndex == GridJunctions[x, y].GridPoints[1].Piece.ColorIndex
                        && GridJunctions[x, y].GridPoints[0].Piece.ColorIndex == GridJunctions[x, y].GridPoints[2].Piece.ColorIndex)
                    {
                        GameReady = false;
                        //Todo: Check for the same color on nearby GridJunctions
                        #region DEBUG
#if DEBUG
                        Debug.Log("Explosion found at " + x + ":" + y);

#endif
                        #endregion
                        int[] numRemoved = new int[GridPoint.All.GetLength(0)];
                        int[] lastRemoved = new int[GridPoint.All.GetLength(0)];
                        for (int i = 0; i < GridJunctions[x, y].GridPoints.Length; i++)
                        {
                            numRemoved[GridJunctions[x, y].GridPoints[i].X]++;
                            lastRemoved[GridJunctions[x, y].GridPoints[i].X] = GridJunctions[x, y].GridPoints[i].Y;
                            GridJunctions[x, y].GridPoints[i].Piece.Deactivate();
                        }
                        ShiftGridPoints(ref numRemoved, ref lastRemoved);

                        return true;
                    }
                }
            }
            return false;
        }

        private void ShiftGridPoints(ref int[] numRemoved, ref int[] lastRemoved)
        {
            
            for (int x = 0; x < GridPoint.All.GetLength(0); x++)
            {
                if (numRemoved[x] < 1)
                    continue;

                for (int y = 1 + lastRemoved[x] - numRemoved[x]; y < GridPoint.All.GetLength(1); y++)
                {
                    if (y < GridPoint.All.GetLength(1) - numRemoved[x])
                    {
                        GridPoint.All[x, y].Piece = GridPoint.All[x, y + numRemoved[x]].Piece;
                        GridPoint.All[x, y].Piece.Activated = false;
                        GridPoint.All[x, y].Piece.TimeActivated = Time.time;
                    }
                    else
                        Piece.ActivatePooled(GridPoint.All[x, y]);
                }
            }
        }

        private void Awake()
        {
            Grid.Instance = this;
        }

        void Start()
        {
            //Prepare two dimentional GridPoint Array for easy access
            GridPoint.All = new GridPoint[Size.x, Size.y];
            for (int i = 0; i < GridPoints.Length; i++)
                GridPoint.All[GridPoints[i].X, GridPoints[i].Y] = GridPoints[i];

            //Initialize GridJunctions
            GridJunctions = new GridJunction[(Size.x - 1) * 2, (Size.y - 1)];
            for (int x = 0; x < GridJunctions.GetLength(0); x++)
                for (int y = 0; y < GridJunctions.GetLength(1); y++)
                    GridJunctions[x, y] = new GridJunction(this, x, y);

            //Deactivate All Pieces but preserve their GridPoints and invoke activation.
            for (int i = 0; i < Pieces.Length; i++)
            {
                Pieces[i].Deactivate(true);
                Pieces[i].ActivateInSeconds(PieceActivationInterval * (i+1));
            }
        }

        private void Update()
        {
            //This prevents player interaction when we don't need it.
            //Example: before all pieces falls into place at the start.
            if (!GameReady)
            {
                bool allActivated = true;
                for (int i = 0; i < Pieces.Length; i++)
                {
                    if (!Pieces[i].Activated)
                    {
                        allActivated = false;
                        break;
                    }
                }
                if (allActivated)
                    GameReady = true;
                return;
            }

#if UNITY_ANDROID || UNITY_IOS
#endif
            if (Input.GetMouseButtonDown(0))
            {
                if (Selection.SelectedGridJunction == null)
                    Selection.Activate(Input.mousePosition);
                LastClickPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if(Mathf.Abs(Input.mousePosition.x - LastClickPosition.x) > 100.0f)
                {
#region DEBUG
#if DEBUG
                    if (Selection.gameObject.activeInHierarchy)
                    {
                        Debug.Log(Input.mousePosition.x + (Input.mousePosition.x > LastClickPosition.x ? " > " : " < ") + LastClickPosition.x);
                        Debug.Log(Input.mousePosition.x > LastClickPosition.x ? "Will Rotate Clockwise" : "Will Rotate Counter Clockwise");
                    }
#endif
#endregion
                    if (Selection.gameObject.activeInHierarchy)
                    {
                        if (Input.mousePosition.x > LastClickPosition.x)
                            Selection.RotateClockwise();
                        else
                            Selection.RotateCounterClockwise();
                    }
                }
                else
                {
                    Selection.Activate(Input.mousePosition);
                    LastClickPosition = Input.mousePosition;
                }
            }
        }
    }
}