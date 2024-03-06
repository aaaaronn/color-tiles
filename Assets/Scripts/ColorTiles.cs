using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorTiles : MonoBehaviour
{
    [Header("General References")]
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject tile;
    [SerializeField] private GameObject background;
    [SerializeField] private Transform backgroundParent;
    [SerializeField] private Transform tilesParent;
    [SerializeField] private GameObject playButton;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text gameplayScoreText;


    private Vector2 mousePos;
    private Vector3Int mousePosGrid;

    private Camera cam;
    private Vector3Int camMinGrid;
    private Vector3Int camMaxGrid;
    private Vector2Int dimensions;

    [Header("Tile Data")]
    [SerializeField] private int tileCount = 200;
    public GameObject[,] tiles;
    [SerializeField] private Color[] colors;

    [Header("Timer stuff")]
    [SerializeField] private Image progressBar;
    [SerializeField] private float startingTime = 100f;
    [SerializeField] private float misclickTimeLoss = 5f;
    [SerializeField] private float currentTime;
    private bool doTimer;


    private int score = 0;



    // Start is called before the first frame update
    void Start()
    {
        // Dimensions of game area
        cam = Camera.main;

        camMinGrid = grid.WorldToCell(cam.ViewportToWorldPoint(new Vector2(0, 0))) + new Vector3Int(2, 1, 0);
        camMaxGrid = grid.WorldToCell(cam.ViewportToWorldPoint(new Vector2(1, 1))) - new Vector3Int(2, 2, 0);
        //print(camMinGrid + "    " + camMaxGrid);

        dimensions.x = camMaxGrid.x - camMinGrid.x + 1;
        dimensions.y = camMaxGrid.y - camMinGrid.y + 1;
        //print(dimensions);

        // checkerboard background pattern
        for (int x = -2; x <= dimensions.x + 1; x++)
        {
            for (int y = -1; y <= dimensions.y + 1; y++)
            {
                if ((x + y) % 2 == 0)
                {
                    Instantiate(background,
                                backgroundParent.TransformPoint(grid.CellToWorld(new Vector3Int(x + camMinGrid.x, y + camMinGrid.y, 0))),
                                Quaternion.identity,
                                backgroundParent);
                }
            }
        }

        currentTime = startingTime;
        gameplayScoreText.text = score.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        #region clickHandler

        if (Input.GetMouseButtonDown(0) && doTimer)
        {
            // Convert mousePos to grid coordinates
            mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.x = Mathf.Clamp(mousePos.x, camMinGrid.x, camMaxGrid.x);
            mousePos.y = Mathf.Clamp(mousePos.y, camMinGrid.y, camMaxGrid.y);
            mousePosGrid = grid.WorldToCell(mousePos);

            //tile.position = mousePosGrid;

            doTimer = true;
            //print("start");
            Vector2Int arrayPos = (Vector2Int)(mousePosGrid - camMinGrid);
            arrayPos.x = Mathf.Clamp(arrayPos.x, 0, dimensions.x + 1);
            arrayPos.y = Mathf.Clamp(arrayPos.y, 0, dimensions.y + 1);
            if (tiles[arrayPos.x, arrayPos.y] != null)
            {
                //print($"clicked tile color: {tiles[arrayPos.x, arrayPos.y].GetComponent<TileData>().colorIndex}");
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
            progressBar.fillAmount = currentTime / startingTime;
        }

        if (currentTime <= 0)
        {
            doTimer = false;
            finalScoreText.gameObject.SetActive(true);
            finalScoreText.text = $"Score\n{score}";
            currentTime = startingTime;
        }
    }


    public void StartNewGame()
    {
        Destroy(tilesParent.gameObject);
        tilesParent = new GameObject("tilesParent").transform;
        tilesParent.transform.position = Vector2.zero;

        tiles = new GameObject[dimensions.x + 1, dimensions.y + 1];
        //print(dimensions.x + " " + dimensions.y);

        int emptyCount = dimensions.x * dimensions.y - tileCount;
        int currentTileCount = tileCount;


        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                int roll = Random.Range(0, currentTileCount + emptyCount);

                if (roll < currentTileCount)
                {
                    currentTileCount--;
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
                else
                {
                    emptyCount--;
                }
            }
        }

        //print($"Current tile count: {currentTileCount} targ: {tileCount}");

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
                score++;
                gameplayScoreText.text = score.ToString();
            }
        }
        if (!hasClearedTiles)
        {
            //print("u stink!!!");
            currentTime -= misclickTimeLoss;
        }
    }
}
