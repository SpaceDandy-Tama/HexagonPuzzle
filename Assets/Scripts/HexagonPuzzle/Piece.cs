using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexagonPuzzle
{
    public class Piece : MonoBehaviour
    {
        public static Queue<Piece> Unused = new Queue<Piece>();

        [HideInInspector]
        public int Index = -1;

        [SerializeField]
        private GridPoint gridPoint;
        public GridPoint GridPoint
        #region Property
        {
            get => gridPoint;
            set
            {
                LastGridPoint = gridPoint;
                gridPoint = value;
            }
        }
        #endregion
        [System.NonSerialized]
        private GridPoint LastGridPoint = null;

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
        [SerializeField]
        private int colorIndex;
        public int ColorIndex => colorIndex;

        [System.NonSerialized]
        public float TimeActivated = -1;
        private float DeltaActivation => Time.time - TimeActivated;

        [System.NonSerialized]
        public bool Activated;

        public void ActivateInSeconds(float time)
        {
            Invoke("Activate", time);
        }

        public static bool ActivatePooled(GridPoint gridPoint)
        {
            if (Unused.Count < 1)
                return false;

            return Unused.Dequeue().Activate(gridPoint, true);
        }
        public bool Activate()
        {
            return Activate(null);
        }
        public bool Activate(GridPoint gridPoint, bool randomizeColor = false)
        {
            gameObject.SetActive(true);
            TimeActivated = Time.time;
            if (GridPoint == null)
            {
                gridPoint.Piece = this;
                //GridPoint = gridPoint; //Propert takes care of this
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
            Activated = false;

            if (!preserveGridPoint)
            {
                GridPoint.Piece = null;
                GridPoint = null;
                LastGridPoint = null;
                Piece.Unused.Enqueue(this);
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
            if (Activated)
            {
                transform.localPosition = GridPos;
                return;
            }

            if (LastGridPoint == null)
            {
                if (DeltaActivation < 1.0f)
                    transform.localPosition = Vector3.Lerp(GridPosStart, GridPos, DeltaActivation);
                else
                {
                    transform.localPosition = GridPos;
                    Activated = true;
                }
            }
            else
            {
                float t = (1.0f / Grid.Instance.Size.y) * (LastGridPoint.Y - GridPoint.Y);
                if (DeltaActivation < t)
                    transform.localPosition = Vector3.Lerp(LastGridPoint.LocalPosition, GridPos, (1.0f / t) * DeltaActivation);
                else
                {
                    transform.localPosition = GridPos;
                    Activated = true;
                }
            }
        }
    }
}