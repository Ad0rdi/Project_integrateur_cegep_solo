using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine.Tilemaps;

public class MapGenerator : NetworkBehaviour
{
    //Pour sync les paramètres de la map sur les clients
    private NetworkList<float> _mapParaNetworkList;

    public int width;
    public int height;

    public float seed;
    public bool useRandomSeed;


    [Range(0, 100)] public int randomFillPercent;

    int[,] map;

    //Tileset
    [SerializeField] private Tilemap tilemap; // Référence à la Tilemap de Unity
    public Tile wallTile; // Tile utilisée pour les murs
    public Tile floorTile; // Tile utilisée pour le sol

    //Monstres
    public GameObject monsterPrefab;
    public GameObject bossPrefab;
    public int numberOfMonsters = 10;
    public int numberOfBoss = 1;
    private List<Vector2Int> _monsterSpawnPositions=new();

    [SerializeField] private LiquidGrid liquidGrid;
    [SerializeField] private int flowFrameRate = 25;
    [SerializeField] private int flowLength = 2;
    [SerializeField] private float flowSize = 3f;

    public void InitialiseParaNetworkList()
    {
        _mapParaNetworkList = new NetworkList<float>();
    }

    private void SetParaNetworkList()
    {
        if (IsHost) return; //Ne pas mettre les valeurs de la map si c'est le host.
        width = (int)_mapParaNetworkList[0];
        height = (int)_mapParaNetworkList[1];
        seed = _mapParaNetworkList[2];
        randomFillPercent = (int)_mapParaNetworkList[3];

        for (int i = 4; i < _mapParaNetworkList.Count; i += 2)
        {
            _monsterSpawnPositions.Add(new Vector2Int((int)_mapParaNetworkList[i], (int)_mapParaNetworkList[i + 1]));
        }

        useRandomSeed = false;
        GenerateMap();
    }

    public new void OnDestroy()
    {
        if (_mapParaNetworkList == null) return;

        _mapParaNetworkList.Dispose(); //Nécessaire pour ne pas gaspiller de mémoire
        _mapParaNetworkList = null;
    }

    public override void OnNetworkSpawn()
    {
        Invoke(nameof(SetParaNetworkList), 0.2f); //Attendre un peu qu'il soit vraiment connecté
    }

    void Start()
    {
        GenerateMap();
        liquidGrid.Generate();
    }

    public void SaveSettingsForNetwork()
    {
        //Pour pouvoir utiliser les paramètre dans les autres instances
        _mapParaNetworkList.Add(width);
        _mapParaNetworkList.Add(height);
        _mapParaNetworkList.Add(seed);
        _mapParaNetworkList.Add(randomFillPercent);

        foreach (var position in _monsterSpawnPositions)
        {
            _mapParaNetworkList.Add(position[0]);
            _mapParaNetworkList.Add(position[1]);
        }
    }


    void Update()
    {
        if (Time.frameCount % flowFrameRate < flowLength)
            for (var i = 0; i < width; i += 2)
            {
                liquidGrid.AddLiquid(i, height - 2, flowSize);
            }
    }


    private void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();

        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }

        ProcessMap();

        // Ajouter le puit après la génération de la carte
        AddWell();

        int borderSize = 1;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];


        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
            }
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(borderedMap, 1);
        //Tileset
        // Remplacer la génération de mesh par la mise à jour de la Tilemap
        CreateTilemap(borderedMap);
        SpawnMonsters();
    }

    //Monstres
    void SpawnMonsters()
    {
        if (_mapParaNetworkList?.Count > 3 && !(IsHost || IsServer))
        {
            foreach (var position in _monsterSpawnPositions)
            {
                Vector3 worldPos = new Vector3(position.x + 0.5f, position.y + 0.5f, -1);
                Instantiate(monsterPrefab, worldPos, Quaternion.identity);
            }

            return;
        }

        List<Vector2Int> floorPositions = new List<Vector2Int>();
        _monsterSpawnPositions?.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 0) // 0 = sol
                {
                    floorPositions.Add(new Vector2Int(x - width / 2, y - height / 2));
                }
            }
        }

        for (int i = 0; i < numberOfMonsters && floorPositions.Count > 0; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, floorPositions.Count);
            Vector2Int position = floorPositions[randomIndex];
            _monsterSpawnPositions?.Add(position);
            floorPositions.RemoveAt(randomIndex); // Pour éviter un doublon

            Vector3 worldPos = new Vector3(position.x + 0.5f, position.y + 0.5f, -1);
            Instantiate(monsterPrefab, worldPos, Quaternion.identity);
        }
    }


    //Tileset
    void CreateTilemap(int[,] map)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                if (map[x, y] == 1)
                {
                    tilemap.SetTile(tilePosition, wallTile); // Placer le mur
                }
                else
                {
                    tilemap.SetTile(tilePosition, floorTile); // Placer le sol
                }
            }
        }

        var mapOffset = new Vector3(-0.75f, -0.94f, 0);
        tilemap.transform.position = new Vector3(-width / 2, (-height / 2), 0) + mapOffset;
    }

    private void AddWell()
    {
        int wellWidth = 5;
        int wellCenterX = width / 2;
        int wellStart = height - 1; // Commence tout en haut
        int wellEnd = height / 4; // Descend aux 3/4 de la carte

        for (int x = wellCenterX - wellWidth / 2; x <= wellCenterX + wellWidth / 2; x++)
        {
            for (int y = wellStart; y >= wellEnd; y--)
            {
                if (x > 0 && x < width - 1) // évite de toucher aux bords latéraux
                {
                    map[x, y] = 0; // Crée un vrai trou
                }
            }
        }
    }

    private void ProcessMap()
    {
        List<List<Coord>> wallRegions = GetRegions(1);
        int wallThresholdSize = 50;

        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0);
        int roomThresholdSize = 50;
        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }

        survivingRooms.Sort();
        survivingRooms[0].isMainRoom = true;
        survivingRooms[0].isAccessibleFromMainRoom = true;

        ConnectClosestRooms(survivingRooms);
    }

    private void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) +
                                                         Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    private void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        //Debug.DrawLine (CoordToWorldPoint (tileA), CoordToWorldPoint (tileB), Color.green, 100);

        List<Coord> line = GetLine(tileA, tileB);
        foreach (Coord c in line)
        {
            DrawCircle(c, 1);
        }
    }

    private void DrawCircle(Coord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (IsInMapRange(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    private List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }

                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    private Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
    }

    private List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    private List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    private bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = DateTime.Now.Millisecond;
        }

        System.Random pseudoRandom = new System.Random(seed.ToString().GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    private void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                    map[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    map[x, y] = 0;
            }
        }
    }

    private int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    private struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    private class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        public Room()
        {
        }

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edgeTiles = new List<Coord>();
            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (map[x, y] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMainRoom()
        {
            if (!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.isAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }

            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }
}