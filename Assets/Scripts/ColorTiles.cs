using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

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
    private Vector2Int arrayPos;

    private Camera cam;
    private Vector3Int camMinGrid;
    private Vector3Int camMaxGrid;
    private Vector2Int dimensions;

    [Header("Tile Data")]
    [SerializeField] private int tileCount = 200;
    public GameObject[,] tiles;
    private int[,] tileColors;
    [SerializeField] private Color[] colors;

    [Header("Timer stuff")]
    [SerializeField] private Image progressBar;
    [SerializeField] private float startingTime = 100f;
    [SerializeField] private float misclickTimeLoss = 5f;
    [SerializeField] private float currentTime;
    private bool doTimer;


    private int score = 0;


    private List<GameObject> circles = new List<GameObject>();
    [SerializeField] private GameObject ghostPrefab;


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

            
            // Convert mouse grid coordinates to an array position
            doTimer = true;
            //print("start");
            arrayPos = (Vector2Int)(mousePosGrid - camMinGrid);
            arrayPos.x = Mathf.Clamp(arrayPos.x, 0, dimensions.x + 1);
            arrayPos.y = Mathf.Clamp(arrayPos.y, 0, dimensions.y + 1);
            if (tiles[arrayPos.x, arrayPos.y] != null)
            {
                //print($"clicked tile color: {tiles[arrayPos.x, arrayPos.y].GetComponent<TileData>().colorIndex}");
            }
            else
            {
                // If didnt click on tile, check to see if proper click
                EraseTiles(arrayPos);
            }
        }
        #endregion

        // Game timer
        if (doTimer)
        {
            currentTime -= Time.deltaTime;
            progressBar.fillAmount = currentTime / startingTime;
        }

        // Game end
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
        // Destroy all old tiles
        Destroy(tilesParent.gameObject);
        tilesParent = new GameObject("tilesParent").transform;
        tilesParent.transform.position = Vector2.zero;

        // Reset tiles array
        tiles = new GameObject[dimensions.x + 1, dimensions.y + 1];
        //print(dimensions.x + " " + dimensions.y);

        // Balanced random placement of tiles
        int emptyCount = dimensions.x * dimensions.y - tileCount;
        int currentTileCount = tileCount;


        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                int roll = Random.Range(0, currentTileCount + emptyCount);

                // place tile and get random color for tile
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
                    tileColors[x, y] = randColorIndex;
                    //instObj.GetComponent<TileData>().colorIndex = randColorIndex;
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
        // Start timer
        currentTime = startingTime;
        doTimer = true;
        playButton.SetActive(false);
    }

    // Struct for each tile storing the gameobject, position in array, and color
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

    private Vector2Int CheckClick(Vector2Int pos, Vector2Int direction)
    {
        // If checking outside array, return fake tile
        if (pos.x < 0 || pos.y < 0 || pos.x > dimensions.x || pos.y > dimensions.y)
        {
            return Vector2Int.left;
        }
        // If checking a tile in array, return it
        if (tiles[pos.x, pos.y] != null)
        {
            Vector2Int distTraveled = pos - arrayPos;
            int distance = (distTraveled.x == 0) ? distTraveled.y : distTraveled.x;



            for (int i = 0; i <  Mathf.Abs(distance); i++)
            {
                //print($"adding tiles for direction {direction}");
                circles.Add(Instantiate(ghostPrefab, grid.CellToWorld((Vector3Int)pos + camMinGrid - (Vector3Int)direction * i), Quaternion.identity));
            }
            //return new GameTile(tiles[pos.x, pos.y].GetComponent<TileData>().colorIndex, tiles[pos.x, pos.y], new Vector2Int(pos.x, pos.y));
            return new Vector2Int(pos.x, pos.y);
        }
        // If no tile found, continue searching in direction
        else
        {
            return CheckClick(pos + direction, direction);
        }
    }

    private void EraseTiles(Vector2Int arrayPos)
    {
        // Create a list of all horizontal/vertical checked tiles
        List<Vector2Int> cubes = new List<Vector2Int>() { CheckClick(arrayPos + new Vector2Int(-1, 0), Vector2Int.left),
                                                      CheckClick(arrayPos + new Vector2Int(1, 0), Vector2Int.right),
                                                      CheckClick(arrayPos + new Vector2Int(0, 1), Vector2Int.up),
                                                      CheckClick(arrayPos + new Vector2Int(0, -1), Vector2Int.down)};

        Dictionary<int, int> colors = new Dictionary<int, int>();

        cubes.RemoveAll(x => x.x == -1);

        // Add to dictonary color index key
        for (int i = 0; i < cubes.Count; i++)
        {           
            if (colors.ContainsKey(tileColors[cubes[i].x, cubes[i].y]))
            {
                colors[tileColors[cubes[i].x, cubes[i].y]]++;
            }
            else
            {
                colors.Add(tileColors[cubes[i].x, cubes[i].y], 1);
            }
        }

        bool hasClearedTiles = false;
        // Play anim and delete tiles
        foreach (var tile in cubes)
        {
            /*if (colors.ContainsKey(tile.color) && colors[tile.color] > 1)
            {
                tile.tile.GetComponent<SpriteRenderer>().sortingOrder = 1;

                tile.tile.GetComponent<Animator>().enabled = true;

                tiles[tile.arrayPos.x, tile.arrayPos.y] = null;
                Destroy(tile.tile, 1.25f);
                hasClearedTiles = true;
                score++;
                gameplayScoreText.text = score.ToString();
            }*/
        }
        if (!hasClearedTiles)
        {
            //print("u stink!!!");
            currentTime -= misclickTimeLoss;
        }
        else
        {
            circles.Add(Instantiate(ghostPrefab, grid.CellToWorld((Vector3Int)arrayPos + camMinGrid), Quaternion.identity));
        }
    }
}
