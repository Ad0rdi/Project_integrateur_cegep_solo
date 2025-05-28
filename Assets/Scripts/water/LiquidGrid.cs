using UnityEngine;

public class LiquidGrid : MonoBehaviour
{
    bool[,] _isMesh;

    private MapGenerator _mapGen;
    private int _mapWidth;
    private int _mapHeight;

    [SerializeField] [Range(0.1f, 1f)] float cellSize = 1;
    [SerializeField] [Range(0f, 0.1f)] float lineWidth = 0;
    [SerializeField] Color lineColor = Color.black;
    [SerializeField] bool showFlow = true;
    [SerializeField] bool renderDownFlowingLiquid = false;
    [SerializeField] bool renderFloatingLiquid = false;

    [SerializeField] private GameObject gridLine;
    [SerializeField] private GameObject gridCell;

    bool _ready = false;

    Cell[,] _cells;
    GridLine[] _horizontalLines;
    GridLine[] _verticalLines;

    Liquid _liquidSimulator;
    [SerializeField] private Sprite[] liquidFlowSprites;

    GameObject _view;
    bool _fill;
    
    [SerializeField] private MapGenerator mapGen;
    [SerializeField] private MeshGenerator meshGen;


    void Awake()
    {
        // Invoke(nameof(Generate), 2f);
    }

    public void Generate()
    {
        _mapHeight = mapGen.height;
        _mapWidth = mapGen.width;
        _isMesh = new bool[_mapWidth, _mapHeight];
        transform.position = new Vector3(-_mapWidth / 2, _mapHeight / 2, 0);

        _ready = true;

        // Camera view
        // _view = GameObject.Find("View").gameObject;

        // Load some sprites to show the liquid flow directions
        // _liquidFlowSprites = Resources.LoadAll<Sprite>("LiquidFlowSprites");
        CreateMeshMask();
        // Generate our viewable grid GameObjects
        CreateGrid();
        
        var tempPos = transform.position;
        tempPos.z = 0.01f;
        transform.position = tempPos;

        // Initialize the liquid simulator
        _liquidSimulator = new Liquid();
        _liquidSimulator.Initialize(_cells);
    }

    void CreateMeshMask()
    {
        var colliders = meshGen.GetComponentsInChildren<EdgeCollider2D>();
        MeshFilter meshFilter = meshGen.cave;
        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        
        _isMesh = new bool[_mapWidth, _mapHeight];

        // foreach (var colid in colliders)
        // {
        //     foreach (var point in colid.points)
        //     {
        //         int[] pxy = TransformWorldToCell(point.x, point.y);
        //         int xp = pxy[0];
        //         int yp = pxy[1];
        //         float[] center = TransformCellsintoWorld(xp, yp);
        //         if (xp - center[0] > 0.1)
        //         {
        //             if (xp > 0 && xp < _mapWidth && yp > 0 && yp < _mapHeight)
        //             {
        //                 _isMesh[xp, yp] = true;
        //             }
        //         }
        //     }
        // }
    }

    float[] TransformCellsintoWorld(int x, int y)
    {
        float newx = (x * (cellSize + lineWidth)) - (cellSize / 2);
        float newy = y * -(cellSize + lineWidth) - (cellSize / 2);

        return new float[] { newx, newy };
    }

    public int[] TransformWorldToCell(float x, float y)
    {
        int xp = (int)((x - transform.position.x) / (cellSize + lineWidth));
        int yp = -(int)((y - transform.position.y) / (cellSize + lineWidth));
        return new int[] { xp, yp };
    }

    void CreateGrid()
    {
        _cells = new Cell[_mapWidth, _mapHeight];
        Vector2 offset = this.transform.position;

        // Organize the grid objects
        GameObject gridLineContainer = new GameObject("GridLines");
        GameObject cellContainer = new GameObject("Cells");
        gridLineContainer.transform.parent = this.transform;
        cellContainer.transform.parent = this.transform;

        // vertical grid lines
        _verticalLines = new GridLine[_mapWidth + 1];
        for (int x = 0; x < _mapWidth + 1; x++)
        {
            GridLine line = Instantiate(gridLine).GetComponent<GridLine>();
            float xpos = offset.x + (cellSize * x) + (lineWidth * x);
            line.Set(lineColor, new Vector2(xpos, offset.y),
                new Vector2(lineWidth, (_mapHeight * cellSize) + lineWidth * _mapHeight + lineWidth));
            line.transform.parent = gridLineContainer.transform;
            _verticalLines[x] = line;
        }

        // horizontal grid lines
        _horizontalLines = new GridLine[_mapHeight + 1];
        for (int y = 0; y < _mapHeight + 1; y++)
        {
            GridLine line = Instantiate(gridLine).GetComponent<GridLine>();
            float ypos = offset.y - (cellSize * y) - (lineWidth * y);
            line.Set(lineColor, new Vector2(offset.x, ypos),
                new Vector2((_mapWidth * cellSize) + lineWidth * _mapWidth + lineWidth, lineWidth));
            line.transform.parent = gridLineContainer.transform;
            _horizontalLines[y] = line;
        }

        // Cells
        for (int x = 0; x < _mapWidth; x++)
        {
            for (int y = 0; y < _mapHeight; y++)
            {
                Cell cell = Instantiate(gridCell).GetComponent<Cell>();
                float xpos = offset.x + (x * cellSize) + (lineWidth * x) + lineWidth;
                float ypos = offset.y - (y * cellSize) - (lineWidth * y) - lineWidth;
                cell.Set(x, y, new Vector2(xpos, ypos), cellSize, liquidFlowSprites, showFlow, renderDownFlowingLiquid,
                    renderFloatingLiquid);

                // add a border
                if (x == 0 || y == 0 || x == _mapWidth - 1 || y == _mapHeight - 1)
                {
                    cell.SetType(CellType.Solid);
                }

                if (_isMesh[x, y] == true)
                {
                    cell.SetType(CellType.Solid);
                }

                cell.transform.parent = cellContainer.transform;
                _cells[x, y] = cell;
            }
        }

        UpdateNeighbors();
        // SetupCameraView();
    }

    void SetupCameraView()
    {
        float totalWidth = (cellSize + lineWidth) * _mapWidth;
        float totalHeight = (cellSize + lineWidth) * _mapHeight;

        // Set view scale to match full grid
        _view.transform.localScale = new Vector3(totalWidth, totalHeight, 1f);

        // Position view at center of grid
        _view.transform.position = new Vector3(
            this.transform.position.x + totalWidth / 2f,
            this.transform.position.y - totalHeight / 2f,
            0f
        );

        // // Set camera to target this area
        // Camera2D cam = Camera.main.GetComponent<Camera2D>();
        // cam.Area = _view.transform;
        // cam.Set(); // Initial camera fit
    }


    // Live update the grid properties
    void RefreshGrid()
    {
        Vector2 offset = this.transform.position;

        // vertical grid lines
        for (int x = 0; x < _mapWidth + 1; x++)
        {
            float xpos = offset.x + (cellSize * x) + (lineWidth * x);
            _verticalLines[x].Set(lineColor, new Vector2(xpos, offset.y),
                new Vector2(lineWidth, (_mapHeight * cellSize) + lineWidth * _mapHeight + lineWidth));
        }

        // horizontal grid lines
        for (int y = 0; y < _mapHeight + 1; y++)
        {
            float ypos = offset.y - (cellSize * y) - (lineWidth * y);
            _horizontalLines[y].Set(lineColor, new Vector2(offset.x, ypos),
                new Vector2((_mapWidth * cellSize) + lineWidth * _mapWidth + lineWidth, lineWidth));
        }

        // Cells
        for (int x = 0; x < _mapWidth; x++)
        {
            for (int y = 0; y < _mapHeight; y++)
            {
                float xpos = offset.x + (x * cellSize) + (lineWidth * x) + lineWidth;
                float ypos = offset.y - (y * cellSize) - (lineWidth * y) - lineWidth;
                _cells[x, y].Set(x, y, new Vector2(xpos, ypos), cellSize, liquidFlowSprites, showFlow,
                    renderDownFlowingLiquid, renderFloatingLiquid);
            }
        }

        // // Fit camera to grid
        // _view.transform.position = this.transform.position + new Vector3(
        //     _horizontalLines[0].transform.localScale.x / 2f,
        //     -_verticalLines[0].transform.localScale.y / 2f);
        // _view.transform.localScale = new Vector2(_horizontalLines[0].transform.localScale.x,
        //     _verticalLines[0].transform.localScale.y);
        // Camera.main.GetComponent<Camera2D>().Set();
    }

    // Sets neighboring cell references
    void UpdateNeighbors()
    {
        for (int x = 0; x < _mapWidth; x++)
        {
            for (int y = 0; y < _mapHeight; y++)
            {
                if (x > 0)
                {
                    _cells[x, y].Left = _cells[x - 1, y];
                }

                if (x < _mapWidth - 1)
                {
                    _cells[x, y].Right = _cells[x + 1, y];
                }

                if (y > 0)
                {
                    _cells[x, y].Top = _cells[x, y - 1];
                }

                if (y < _mapHeight - 1)
                {
                    _cells[x, y].Bottom = _cells[x, y + 1];
                }
            }
        }
    }

    public void AddLiquid(int x, int y, float qte)
    {
        _cells[x, y].AddLiquid(qte);
    }

    void Update()
    {
        if (_ready)
        {

          
            
            // Convert mouse position to Grid Coordinates
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int x = (int)((pos.x - this.transform.position.x) / (cellSize + lineWidth));
            int y = -(int)((pos.y - this.transform.position.y) / (cellSize + lineWidth));

            // Check if we are filling or erasing walls
            if (Input.GetMouseButtonDown(0))
            {
                if ((x > 0 && x < _cells.GetLength(0)) && (y > 0 && y < _cells.GetLength(1)))
                {
                    if (_cells[x, y].Type == CellType.Blank)
                    {
                        _fill = true;
                    }
                    else
                    {
                        _fill = false;
                    }
                }
            }

            // Left click draws/erases walls
            if (Input.GetMouseButton(0))
            {
                if (x != 0 && y != 0 && x != _mapWidth - 1 && y != _mapHeight - 1)
                {
                    if ((x > 0 && x < _cells.GetLength(0)) && (y > 0 && y < _cells.GetLength(1)))
                    {
                        if (_fill)
                        {
                            _cells[x, y].SetType(CellType.Solid);
                        }
                        else
                        {
                            _cells[x, y].SetType(CellType.Blank);
                        }
                    }
                }
            }

            // Right click places liquid
            if (Input.GetMouseButton(1))
            {
                if ((x > 0 && x < _cells.GetLength(0)) && (y > 0 && y < _cells.GetLength(1)))
                {
                    _cells[x, y].AddLiquid(5);
                }
            }

            // Run our liquid simulation 
            _liquidSimulator.Simulate(ref _cells);
        }
    }
}