using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

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
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        camMinGrid = grid.WorldToCell(cam.ViewportToWorldPoint(new Vector2(0, 0))) + new Vector3Int(1, 0, 0);
        camMaxGrid = grid.WorldToCell(cam.ViewportToWorldPoint(new Vector2(1, 1))) - new Vector3Int(1, 1, 0);
        //print(camMinGrid + "    " + camMaxGrid);

        dimensions.x = camMaxGrid.x - camMinGrid.x + 1;
        dimensions.y = camMaxGrid.y - camMinGrid.y + 1;
        /*
        for (int i = camMinGrid.x; i <= camMaxGrid.x; i++)
        {
            for (int j = camMinGrid.y; j <= camMaxGrid.y; j++)
            {
                Instantiate(background, backgroundParent.TransformPoint(new Vector3(i, j, 0)), Quaternion.identity, backgroundParent);
            }
        }
        */
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

    }

    // Update is called once per frame
    void Update()
    {
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.x = Mathf.Clamp(mousePos.x, camMinGrid.x, camMaxGrid.x);
        mousePos.y = Mathf.Clamp(mousePos.y, camMinGrid.y, camMaxGrid.y);
        mousePosGrid = grid.WorldToCell(mousePos);

        //tile.position = mousePosGrid;

        if (Input.GetMouseButtonDown(0))
        {
            print("start");
            Vector2Int arrayPos = (Vector2Int)(mousePosGrid - camMinGrid);
            if (tiles[arrayPos.x, arrayPos.y] != null)
            {
                //print(tiles[arrayPos.x, arrayPos.y].GetComponent<TileData>().colorIndex);
            }
            else
            {
                EraseTiles(arrayPos);
            }
        }
    }

    struct TileColorPair
    {
        public int color;
        public GameObject tile;

        public TileColorPair(int color, GameObject tile)
        {
            this.color = color;
            this.tile = tile;
        }
    }

    private TileColorPair CheckClick(Vector2Int pos, Vector2Int direction)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x > dimensions.x || pos.y > dimensions.y)
        {
            return new TileColorPair(-1, null);
        }
        if (tiles[pos.x, pos.y] != null)
        {
            return new TileColorPair(tiles[pos.x, pos.y].GetComponent<TileData>().colorIndex, tiles[pos.x, pos.y]);
        }
        else
        {
            return CheckClick(pos + direction, direction);
        }
    }

    private void EraseTiles(Vector2Int arrayPos)
    {
        List<TileColorPair> cubes = new List<TileColorPair>() { CheckClick(arrayPos + new Vector2Int(-1, 0), new Vector2Int(-1, 0)),
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
        foreach (KeyValuePair<int, int> col in colors.ToList())
        {
            if (col.Value <= 1)
            {
                colors.Remove(col.Key);
            }
        }
        foreach (var pair in cubes)
        {
            if (colors.ContainsKey(pair.color))
            {
                pair.tile.GetComponent<SpriteRenderer>().sortingOrder = 1;

                pair.tile.GetComponent<Animator>().enabled = true;
                //if (Random.value > 0.5)
                //pair.tile.GetComponent<Animator>().SetBool("flip", true);

                Destroy(pair.tile, 1.25f);
                print("done");
            }
        }
    }
}
