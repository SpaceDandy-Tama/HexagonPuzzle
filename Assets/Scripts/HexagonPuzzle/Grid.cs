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
        public bool StartByCheckingExplosions = false;

        [Header("Sprite Settings")]
        public Sprite PieceSprite;
        public Sprite BombSprite;

        [Header("Audio Settings")]
        public AudioClip AC_PieceSelect;
        public AudioClip AC_PieceClockwise;
        public AudioClip AC_PieceCounterClockwise;
        public AudioClip AC_PieceExplosion;
        public AudioClip AC_BombExplosion;

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
        [System.NonSerialized]
        public bool ExplosionOccurred = false;
        [System.NonSerialized]
        public int BombCounter;
        [System.NonSerialized]
        public AudioSource AudioSource;

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
            RemoveGrid();
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Grid.Instance = this;
            }
#endif

            //Set up Grid origin based on Grid Size
            transform.position = new Vector3((-((Size.x - 1) * PieceScale.x * 0.725f) / 2), (-((Size.y) * PieceScale.y) / 2));

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

            //This was disabled because it created a bug where sometimes explosions wasn't detected correctly
            /*
            //Modify loop values to only check for the given junction and its neighbors to avoid unnecessary calculations
            if (gridJunction != null)
            {
                xStart = gridJunction.X > 1 ? gridJunction.X - 2 : xStart;
                yStart = gridJunction.Y > 1 ? gridJunction.Y - 2 : yStart;
                xLength = gridJunction.X < xLength - 2 ? gridJunction.X + 3 : xLength;
                yLength = gridJunction.Y < yLength - 2 ? gridJunction.Y + 3 : yLength;
            }
            */

            for (int y = yStart; y < yLength; y++)
            {
                for (int x = xStart; x < xLength; x++)
                    {
                    if (GridJunctions[x, y].GridPoints[0].Piece.ColorIndex == GridJunctions[x, y].GridPoints[1].Piece.ColorIndex
                        && GridJunctions[x, y].GridPoints[0].Piece.ColorIndex == GridJunctions[x, y].GridPoints[2].Piece.ColorIndex)
                    {
                        GameReady = false;
                        int colorIndex = GridJunctions[x, y].GridPoints[0].Piece.ColorIndex;

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

                        //Also need to check surrounding pieces to detect more than 3
                        GridPoint neighbor;
                        GridPoint gridPointA = GridJunctions[x, y].GridPoints[0];
                        GridPoint gridPointB = GridJunctions[x, y].GridPoints[1];
                        GridPoint gridPointC = GridJunctions[x, y].GridPoints[2];

                        if (GridPoint.GetCommonNeighbor(gridPointA, gridPointB, gridPointC, out neighbor))
                        {
                            if (neighbor != null && neighbor.Piece != null && neighbor.Piece.Activated && neighbor.Piece.ColorIndex == colorIndex)
                            {
                                numRemoved[neighbor.X]++;
                                lastRemoved[neighbor.X] = neighbor.Y > lastRemoved[neighbor.X] ? neighbor.Y : lastRemoved[neighbor.X];
                                neighbor.Piece.Deactivate();
                            }
                        }

                        gridPointA = GridJunctions[x, y].GridPoints[1];
                        gridPointB = GridJunctions[x, y].GridPoints[2];
                        gridPointC = GridJunctions[x, y].GridPoints[0];

                        if (GridPoint.GetCommonNeighbor(gridPointA, gridPointB, gridPointC, out neighbor))
                        {
                            if (neighbor != null && neighbor.Piece != null && neighbor.Piece.Activated && neighbor.Piece.ColorIndex == colorIndex)
                            {
                                numRemoved[neighbor.X]++;
                                lastRemoved[neighbor.X] = neighbor.Y > lastRemoved[neighbor.X] ? neighbor.Y : lastRemoved[neighbor.X];
                                neighbor.Piece.Deactivate();
                            }
                        }

                        gridPointA = GridJunctions[x, y].GridPoints[2];
                        gridPointB = GridJunctions[x, y].GridPoints[0];
                        gridPointC = GridJunctions[x, y].GridPoints[1];

                        if (GridPoint.GetCommonNeighbor(gridPointA, gridPointB, gridPointC, out neighbor))
                        {
                            if (neighbor != null && neighbor.Piece != null && neighbor.Piece.Activated && neighbor.Piece.ColorIndex == colorIndex)
                            {
                                numRemoved[neighbor.X]++;
                                lastRemoved[neighbor.X] = neighbor.Y > lastRemoved[neighbor.X] ? neighbor.Y : lastRemoved[neighbor.X];
                                neighbor.Piece.Deactivate();
                            }
                        }

                        ShiftGridPoints(ref numRemoved, ref lastRemoved);

                        //Add the score
                        int totalRemoved = 0;
                        for (int i = 0; i < numRemoved.Length; i++)
                            totalRemoved += numRemoved[i];
                        Menu.Instance.Score += totalRemoved * 5;

                        //Play Audio
                        AudioSource.PlayOneShot(AC_PieceExplosion);

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
                        //this is set to false until piece falls to its new gridpoint. It will because true once it does automaticly
                        GridPoint.All[x, y].Piece.Activated = false;
                        //this is set to current time so that fall to place can be lerped.
                        GridPoint.All[x, y].Piece.TimeActivated = Time.time;
                    }
                    else
                    {
                        if (BombCounter < Menu.Instance.Score / BombAppearanceScore)
                        {
                            Bomb.CreateNew(GridPoint.All[x, y]);
                            BombCounter++;
                        }
                        else
                            Piece.ActivatePooled(GridPoint.All[x, y]);
                    }
                }
            }
        }

        private void Awake()
        {
            Grid.Instance = this;
            AudioSource = gameObject.GetComponent<AudioSource>();

            if (Bomb.Exploded)
                AudioSource.PlayOneShot(AC_BombExplosion);
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

            if (StartByCheckingExplosions)
                ExplosionOccurred = true;
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
                    if (Pieces[i].gameObject.activeInHierarchy && !Pieces[i].Activated)
                    {
                        allActivated = false;
                        break;
                    }
                }
                foreach (Bomb bomb in Bomb.All)
                {
                    if (bomb == null)
                        continue;

                    if (bomb.gameObject.activeInHierarchy && !bomb.Activated)
                    {
                        allActivated = false;
                        break;
                    }
                }
                if (allActivated)
                {
                    //If there was an explosion earlier, do not enable the game immedietly, check for repeating explosions due to new pieces being added in and shifts.
                    if (ExplosionOccurred)
                        ExplosionOccurred = CheckForExplosion();
                    else
                        GameReady = true;
                }
                return;
            }

            Bomb.CheckFuses();

            //Ignore Input if mouse is hovering on top of the header.
            if (Input.mousePosition.y < Screen.height - 100)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (Selection.SelectedGridJunction == null)
                        Selection.Activate(Input.mousePosition);
                    LastClickPosition = Input.mousePosition;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    Vector3 delta = Input.mousePosition - LastClickPosition;
                    if (delta.magnitude > 100.0f)
                    {
            #region DEBUG
#if UNITY_EDITOR && DEBUG
                        if (Selection.gameObject.activeInHierarchy)
                            Debug.Log(Input.mousePosition.x + " | " + LastClickPosition.x);
#endif
            #endregion
                        if (Selection.gameObject.activeInHierarchy)
                        {
                            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                            {
                                if (Input.mousePosition.x > LastClickPosition.x)
                                    Selection.RotateClockwise();
                                else
                                    Selection.RotateCounterClockwise();
                            }
                            else
                            {
                                if (Input.mousePosition.y > LastClickPosition.y)
                                    Selection.RotateClockwise();
                                else
                                    Selection.RotateCounterClockwise();
                            }
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
}