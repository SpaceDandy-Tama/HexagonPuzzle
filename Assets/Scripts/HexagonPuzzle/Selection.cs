using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexagonPuzzle
{
    public class Selection : MonoBehaviour
    {
        public GameObject BackgroundObject;
        public GameObject ForegroundObject;
        public GameObject Piece0;
        public GameObject Piece1;
        public GameObject Piece2;

        private SpriteRenderer Piece0SpriteRenderer;
        public Color Piece0Color
        #region Property
        {
            get => Piece0SpriteRenderer.color;
            set => Piece0SpriteRenderer.color = value;
        }
        #endregion

        private SpriteRenderer Piece1SpriteRenderer;
        public Color Piece1Color
        #region Property
        {
            get => Piece1SpriteRenderer.color;
            set => Piece1SpriteRenderer.color = value;
        }
        #endregion

        private SpriteRenderer Piece2SpriteRenderer;
        public Color Piece2Color
        #region Property
        {
            get => Piece2SpriteRenderer.color;
            set => Piece2SpriteRenderer.color = value;
        }
        #endregion

        [System.NonSerialized]
        public GridJunction SelectedGridJunction;

        private void Start()
        {
            Grid.Instance.Selection = this;

            Piece0SpriteRenderer = Piece0.GetComponent<SpriteRenderer>();
            Piece1SpriteRenderer = Piece1.GetComponent<SpriteRenderer>();
            Piece2SpriteRenderer = Piece2.GetComponent<SpriteRenderer>();

            ForegroundObject.transform.parent = null; //We don't want the circle to get squished
            ForegroundObject.transform.localScale = Vector3.one * Grid.Instance.PieceScale.x;
            transform.localScale = Grid.Instance.PieceScale;

            Deactivate();
        }

        //This Overload is added because mousePosition returns Vector3 while Touch.position returns Vector2
        public void Activate(Vector2 screenPoint)
        {
            Activate(new Vector3(screenPoint.x, screenPoint.y, 0));
        }
        public void Activate(Vector3 screenPoint)
        {
            if (Grid.Instance == null)
                return;

            Vector3 worldPoint = Camera.allCameras[0].ScreenToWorldPoint(screenPoint);

            //Find the closest GridJunction
            GridJunction closest = null;
            float closestDist = float.MaxValue;
            for (int x = 0; x < Grid.Instance.GridJunctions.GetLength(0); x++)
            {
                for (int y = 0; y < Grid.Instance.GridJunctions.GetLength(1); y++)
                {
                    float dist = Vector3.Distance(Grid.Instance.GridJunctions[x, y].WorldPosition, worldPoint);

                    if (dist > closestDist)
                        continue;

                    closest = Grid.Instance.GridJunctions[x, y];
                    closestDist = dist;
                }
            }

            SelectedGridJunction = closest;

            //Set gameobject active
            gameObject.SetActive(true);
            ForegroundObject.SetActive(true);

            //Set position to GridJunction
            ForegroundObject.transform.position = transform.position = SelectedGridJunction.WorldPosition;

            //Rotate Bg and reposition pieces based on oddness
            BackgroundObject.transform.localRotation = SelectedGridJunction.IsOdd ? Quaternion.Euler(Vector3.zero) : Quaternion.Euler(Vector3.up * 180);
            Piece0.transform.localPosition = SelectedGridJunction.IsOdd ? new Vector3(-0.375f, 0) : new Vector3(0.375f, 0);
            Piece1.transform.localPosition = SelectedGridJunction.IsOdd ? new Vector3(0.375f, -0.5f) : new Vector3(-0.375f, -0.5f);
            Piece2.transform.localPosition = SelectedGridJunction.IsOdd ? new Vector3(0.375f, 0.5f) : new Vector3(-0.375f, 0.5f);

            //Assign Colors
            Piece0Color = SelectedGridJunction.GridPoints[0].Piece.Color;
            Piece1Color = SelectedGridJunction.GridPoints[1].Piece.Color;
            Piece2Color = SelectedGridJunction.GridPoints[2].Piece.Color;
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
            ForegroundObject.SetActive(false);
        }
    }
}