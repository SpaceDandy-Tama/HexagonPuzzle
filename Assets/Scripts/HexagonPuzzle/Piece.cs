using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexagonPuzzle
{
    public class Piece : MonoBehaviour
    {
        [System.NonSerialized]
        public int Index = -1;

        public GridPoint GridPoint;

        public Vector3 GridPos => GridPoint.LocalPosition;
        public Vector3 GridPosWorld => GridPoint.Grid.transform.TransformPoint(GridPoint.LocalPosition);
        public Vector3 GridPosStart => GridPoint.LocalStartPosition;
        public Vector3 GridPosStartWorld => GridPoint.Grid.transform.TransformPoint(GridPoint.LocalStartPosition);

        private SpriteRenderer spriteRenderer;
        public SpriteRenderer SpriteRenderer => spriteRenderer == null ? spriteRenderer = GetComponent<SpriteRenderer>() : spriteRenderer;
        public Color Color
        #region Property
        {
            get => SpriteRenderer.color;
            set
            {
                for (int i = 0; i < GridPoint.Grid.PieceColors.Length; i++)
                {
                    if (GridPoint.Grid.PieceColors[i].Equals(value))
                    {
                        colorIndex = i;
                        SpriteRenderer.color = GridPoint.Grid.PieceColors[i];
                        break;
                    }
                }
            }
        }
        #endregion
        private int colorIndex;
        public int ColorIndex => colorIndex;

        private float TimeActivated = -1;

        public void ActivateInSeconds(float time)
        {
            Invoke("Activate", time);
        }

        public bool Activate()
        {
            return Activate(null);
        }
        public bool Activate(GridPoint gridPoint, bool randomizeColor = false)
        {
            if (GridPoint == null && (gridPoint == null || gridPoint.Piece != null))
                return false;

            gameObject.SetActive(true);
            TimeActivated = Time.time;
            if (GridPoint == null)
            {
                gridPoint.Piece = this;
                GridPoint = gridPoint;
            }
            transform.localPosition = GridPosStart;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                TimeActivated = -1;
                transform.localPosition = GridPos;
            }
#endif
            if (randomizeColor)
                Color = Grid.Instance.PieceColors[Random.Range(0, Grid.Instance.PieceColors.Length)];

            return true;
        }

        public void Deactivate(bool preserveGridPoint = false)
        {
            gameObject.SetActive(false);
            TimeActivated = -1f;

            if (!preserveGridPoint)
            {
                GridPoint.Piece = null;
                GridPoint = null;
            }
        }

        #region Helper Functions
        protected static GameObject CreateNewSprite(int i = -1)
        {
            GameObject go = new GameObject("Piece" + (i < 0 ? "" : i.ToString()));
            go.SetActive(false);
            go.transform.parent = Grid.Instance.transform;
            go.transform.localScale = Grid.Instance.PieceScale;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();

            return go;
        }

        public static Piece CreateNew(int i)
        {
            Piece piece = CreateNewSprite(i).AddComponent<Piece>();
            piece.SpriteRenderer.sprite = Grid.Instance.PieceSprite;
            Grid.Instance.Pieces[i] = piece;
            piece.Index = i;

            return piece;
        }
        #endregion

        protected virtual void Update()
        {
            if (GridPoint == null)
                return;

            float deltaActivation = Time.time - TimeActivated;
            if (deltaActivation < 1.0f)
                transform.localPosition = Vector3.Lerp(GridPosStart, GridPos, deltaActivation);
            else
                transform.localPosition = GridPos;
        }
    }
}