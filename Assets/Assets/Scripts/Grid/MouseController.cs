using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace finished3
{
    public class MouseController : MonoBehaviour
    {
        public float speed;
        public int steps;
        public GameObject characterPrefab;
        public CharacterInfo character;
        OverlayTile Active;

        private PathFinder pathFinder;
        private RangeFinder rangeFinder;
        private List<OverlayTile> path;
        private List<OverlayTile> rangeFinderTiles;
        private bool isMoving;

        private void Start()
        {
            pathFinder = new PathFinder();
            rangeFinder = new RangeFinder();

            path = new List<OverlayTile>();
            isMoving = false;
            rangeFinderTiles = new List<OverlayTile>();
            Debug.Log(character);
        }

        void Update()
        {
            RaycastHit2D? hit = GetFocusedOnTile();

            if (hit.HasValue)
            {
                OverlayTile tile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();
                transform.position = tile.transform.position;
                var active = ActiveTile();
                Active = active.Value.collider.GetComponent<OverlayTile>();
                PositionCharacterOnLine(Active);
                gameObject.GetComponent<SpriteRenderer>().sortingOrder = tile.transform.GetComponent<SpriteRenderer>().sortingOrder;

                if (rangeFinderTiles.Contains(tile) && !isMoving)
                {
                    path = pathFinder.FindPath(Active, tile, rangeFinderTiles);
                }
                if (character != null && Active != null)
                {
                    GetInRangeTiles(Active, steps);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    tile.ShowTile();

                    if (character == null)
                    {
                        character = Instantiate(characterPrefab).GetComponent<CharacterInfo>();
                    }
                    else
                    {
                        isMoving = true;
                        tile.gameObject.GetComponent<OverlayTile>().HideTile();
                    }
                }
            }

            if (path.Count > 0 && isMoving)
            {
                MoveAlongPath();
            }
        }

        private void MoveAlongPath()
        {
            var step = speed * Time.deltaTime;

            float zIndex = path[0].transform.position.z;
            character.transform.position = Vector2.MoveTowards(character.transform.position, path[0].transform.position, step);
            character.transform.position = new Vector3(character.transform.position.x, character.transform.position.y, zIndex);

            if (Vector2.Distance(character.transform.position, path[0].transform.position) < 0.00001f)
            {
                PositionCharacterOnLine(path[0]);
                path.RemoveAt(0);
            }

            if (path.Count == 0)
            {
                GetInRangeTiles(Active, steps);
                isMoving = false;
            }

        }

        public RaycastHit2D? ActiveTile()
        {
            Vector2 origin = new Vector2(character.transform.position.x, character.transform.position.y);

            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.zero);

            if (hits.Length > 0)
            {
                return hits.OrderByDescending(i => i.collider.transform.position.z).First();
            }
            return null;
        }

        private void PositionCharacterOnLine(OverlayTile tile)
        {
            character.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.0001f, tile.transform.position.z-1);
            character.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
            Active = tile;
        }

        private static RaycastHit2D? GetFocusedOnTile()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);

            if (hits.Length > 0)
            {
                return hits.OrderByDescending(i => i.collider.transform.position.z).First();
            }

            return null;
        }

        public void GetInRangeTiles(OverlayTile active, int movement)
        {
            rangeFinderTiles = rangeFinder.GetTilesInRange(active, movement);

            foreach (var item in rangeFinderTiles)
            {
                item.ShowTile();
            }
        }
    }
}