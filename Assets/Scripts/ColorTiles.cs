using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorTiles : MonoBehaviour
{
    [SerializeField] private Vector2 mousePos;
    private Camera cam;
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject tile;
    [SerializeField] private GameObject background;
    [SerializeField] private Transform backgroundParent;

    private Vector3Int camMinGrid;
    private Vector3Int camMaxGrid;
    [SerializeField] private Vector3Int mousePosGrid;

    private Vector2Int dimensions;

    public GameObject[,] tiles;
    [SerializeField] private Transform tilesParent;

    [SerializeField] private Color[] colors;

    [Header("Timer stuff")]
    [SerializeField] private Image bar;
    [SerializeField] private float startingTime = 100f;
    [SerializeField] private float misclickTimeLoss = 5f;
    private float currentTime;
    private bool doTimer;

    [SerializeField] private GameObject playButton;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        camMinGrid = grid.WorldToCell(cam.ViewportToWorldPoint(new Vector2(0, 0))) + new Vector3Int(1, 0, 0);
        camMaxGrid = grid.WorldToCell(cam.ViewportToWorldPoint(new Vector2(1, 1))) - new Vector3Int(1, 1, 0);
        //print(camMinGrid + "    " + camMaxGrid);

        dimensions.x = camMaxGrid.x - camMinGrid.x + 1;
        dimensions.y = camMaxGrid.y - camMinGrid.y + 1;

        bool didPlace = false;
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                if (!didPlace)
                {
                    print(x);
                    Instantiate(background,
                                backgroundParent.TransformPoint(grid.CellToWorld(new Vector3Int(x + camMinGrid.x, y + camMinGrid.y, 0))),
                                Quaternion.identity,
                                backgroundParent);
                    didPlace = true;
                }
                else didPlace = false;
            }
            if (didPlace)
                didPlace = false;
            else didPlace = true;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        #region clickHandler
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.x = Mathf.Clamp(mousePos.x, camMinGrid.x, camMaxGrid.x);
        mousePos.y = Mathf.Clamp(mousePos.y, camMinGrid.y, camMaxGrid.y);
        mousePosGrid = grid.WorldToCell(mousePos);

        //tile.position = mousePosGrid;

        if (Input.GetMouseButtonDown(0) && doTimer)
        {
            doTimer = true;
            //print("start");
            Vector2Int arrayPos = (Vector2Int)(mousePosGrid - camMinGrid);
            arrayPos.x = Mathf.Clamp(arrayPos.x, 0, dimensions.x + 1);
            arrayPos.y = Mathf.Clamp(arrayPos.y, 0, dimensions.y + 1);
            if (tiles[arrayPos.x, arrayPos.y] != null)
            {
                //print(tiles[arrayPos.x, arrayPos.y].GetComponent<TileData>().colorIndex);
            }
            else
            {
                EraseTiles(arrayPos);
            }
        }
        #endregion

        if (doTimer)
        {
            currentTime -= Time.deltaTime;
            bar.fillAmount = currentTime / startingTime;
        }

        if (currentTime <= 0)
        {
            doTimer = false;
            playButton.SetActive(false);
        }
    }


    public void StartNewGame()
    {
        while (tilesParent.childCount > 0)
        {
            Destroy(tilesParent.GetChild(0));
        }

        tiles = new GameObject[dimensions.x + 1, dimensions.y + 1];
        //print(dimensions.x + " " + dimensions.y);

        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                if (Random.value > 0.3f)
                {
                    GameObject instObj =
                        Instantiate(tile,
                                    grid.CellToWorld(new Vector3Int(x + camMinGrid.x, y + camMinGrid.y, 0)),
                                    Quaternion.identity,
                                    tilesParent);

                    int randColorIndex = Random.Range(0, colors.Length);
                    instObj.transform.localScale = new Vector3(instObj.transform.localScale.x * grid.cellSize.x,
                                                               instObj.transform.localScale.y * grid.cellSize.x, 1);

                    instObj.GetComponent<SpriteRenderer>().color = colors[randColorIndex];
                    instObj.GetComponent<TileData>().colorIndex = randColorIndex;
                    //print(x + "  " + y);
                    tiles[x, y] = instObj;
                    //print("done");
                }
            }
        }

        currentTime = startingTime;
        doTimer = true;
        playButton.SetActive(false);
    }

    struct GameTile
    {
        public int color;
        public GameObject tile;
        public Vector2Int arrayPos;

        public GameTile(int color, GameObject tile, Vector2Int arrayPos)
        {
            this.color = color;
            this.tile = tile;
            this.arrayPos = arrayPos;
        }
    }

    private GameTile CheckClick(Vector2Int pos, Vector2Int direction)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x > dimensions.x || pos.y > dimensions.y)
        {
            return new GameTile(-1, null, Vector2Int.zero);
        }
        if (tiles[pos.x, pos.y] != null)
        {
            return new GameTile(tiles[pos.x, pos.y].GetComponent<TileData>().colorIndex, tiles[pos.x, pos.y], new Vector2Int(pos.x, pos.y));
        }
        else
        {
            return CheckClick(pos + direction, direction);
        }
    }

    private void EraseTiles(Vector2Int arrayPos)
    {
        List<GameTile> cubes = new List<GameTile>() { CheckClick(arrayPos + new Vector2Int(-1, 0), new Vector2Int(-1, 0)),
                                                                        CheckClick(arrayPos + new Vector2Int(1, 0), new Vector2Int(1, 0)),
                                                                        CheckClick(arrayPos + new Vector2Int(0, 1), new Vector2Int(0, 1)),
                                                                        CheckClick(arrayPos + new Vector2Int(0, -1), new Vector2Int(0, -1))};

        Dictionary<int, int> colors = new Dictionary<int, int>();

        for (int i = 0; i < cubes.Count; i++)
        {
            if (colors.ContainsKey(cubes[i].color))
            {
                colors[cubes[i].color]++;
            }
            else
            {
                colors.Add(cubes[i].color, 1);
            }
        }

        bool hasClearedTiles = false;
        foreach (var tile in cubes)
        {
            if (colors.ContainsKey(tile.color) && colors[tile.color] > 1)
            {
                tile.tile.GetComponent<SpriteRenderer>().sortingOrder = 1;

                tile.tile.GetComponent<Animator>().enabled = true;

                tiles[tile.arrayPos.x, tile.arrayPos.y] = null;
                Destroy(tile.tile, 1.25f);
                hasClearedTiles = true;
            }
        }
        if (!hasClearedTiles)
        {
            print("u stink!!!");
            currentTime -= misclickTimeLoss;
        }
    }
}
