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

        private void Awake()
        {
            Grid.Instance = this;
        }

        void Start()
        {
            for (int i = 0; i < Pieces.Length; i++)
            {
                Pieces[i].Deactivate(true);
                Pieces[i].ActivateInSeconds(PieceActivationInterval * (i+1));
            }
        }

        private void Update()
        {

        }
    }
}