using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator Instance { get; private set; }

    // Tilemapへの参照
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public TileBase floorTile;
    public TileBase wallTile;

    // ダンジョンのサイズ
    public int mapWidth = 50;
    public int mapHeight = 50;

    // 部屋のサイズ
    public int minRoomSize = 5;
    public int maxRoomSize = 10;

    // マップデータ
    public int[,] map;

    // 部屋のリストを公開
    public List<Room> rooms = new List<Room>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        map = new int[mapWidth, mapHeight];
        GenerateDungeon();
        DrawMap();
        PlacePlayer(); // プレイヤーの配置を呼び出す
    }

    void GenerateDungeon()
    {
        List<Rect> leafNodes = new List<Rect>();
        leafNodes.Add(new Rect(0, 0, mapWidth, mapHeight));

        bool split = true;
        int maxIterations = 10;
        int iteration = 0;

        // BSPを使ってマップを分割
        while (split && iteration < maxIterations)
        {
            split = false;
            List<Rect> newLeafs = new List<Rect>();

            foreach (var leaf in leafNodes)
            {
                if (leaf.width > maxRoomSize * 1.5f || leaf.height > maxRoomSize * 1.5f)
                {
                    // 水平方向か垂直方向に分割
                    bool splitHorizontally = UnityEngine.Random.value > 0.5f;
                    if (leaf.width / leaf.height >= 1.25f)
                        splitHorizontally = false;
                    else if (leaf.height / leaf.width >= 1.25f)
                        splitHorizontally = true;

                    if (splitHorizontally)
                    {
                        float splitPoint = UnityEngine.Random.Range(minRoomSize, leaf.height - minRoomSize);
                        Rect left = new Rect(leaf.x, leaf.y, leaf.width, splitPoint);
                        Rect right = new Rect(leaf.x, leaf.y + splitPoint, leaf.width, leaf.height - splitPoint);
                        newLeafs.Add(left);
                        newLeafs.Add(right);
                    }
                    else
                    {
                        float splitPoint = UnityEngine.Random.Range(minRoomSize, leaf.width - minRoomSize);
                        Rect left = new Rect(leaf.x, leaf.y, splitPoint, leaf.height);
                        Rect right = new Rect(leaf.x + splitPoint, leaf.y, leaf.width - splitPoint, leaf.height);
                        newLeafs.Add(left);
                        newLeafs.Add(right);
                    }

                    split = true;
                }
                else
                {
                    newLeafs.Add(leaf);
                }
            }

            leafNodes = newLeafs;
            iteration++;
        }

        // 各リーフに部屋を配置
        foreach (var leaf in leafNodes)
        {
            int roomWidth = UnityEngine.Random.Range(minRoomSize, Mathf.Min(maxRoomSize, (int)leaf.width - 2));
            int roomHeight = UnityEngine.Random.Range(minRoomSize, Mathf.Min(maxRoomSize, (int)leaf.height - 2));
            int roomX = UnityEngine.Random.Range((int)leaf.x + 1, (int)(leaf.x + leaf.width - roomWidth - 1));
            int roomY = UnityEngine.Random.Range((int)leaf.y + 1, (int)(leaf.y + leaf.height - roomHeight - 1));

            Rect roomRect = new Rect(roomX, roomY, roomWidth, roomHeight);
            Room room = new Room(roomRect);
            rooms.Add(room);

            // マップに部屋を描画
            for (int x = (int)roomRect.x; x < roomRect.x + roomRect.width; x++)
            {
                for (int y = (int)roomRect.y; y < roomRect.y + roomRect.height; y++)
                {
                    map[x, y] = 1; // 1は床を表す
                }
            }
        }

        // 部屋同士を通路で繋げる
        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2 prevCenter = rooms[i - 1].center;
            Vector2 currentCenter = rooms[i].center;

            if (UnityEngine.Random.value > 0.5f)
            {
                CreateHorizontalTunnel((int)prevCenter.x, (int)currentCenter.x, (int)prevCenter.y);
                CreateVerticalTunnel((int)prevCenter.y, (int)currentCenter.y, (int)currentCenter.x);
            }
            else
            {
                CreateVerticalTunnel((int)prevCenter.y, (int)currentCenter.y, (int)prevCenter.x);
                CreateHorizontalTunnel((int)prevCenter.x, (int)currentCenter.x, (int)currentCenter.y);
            }
        }

        // 壁を設定
        SetWalls();
    }

    void CreateHorizontalTunnel(int x1, int x2, int y)
    {
        for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
        {
            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                map[x, y] = 1;
        }
    }

    void CreateVerticalTunnel(int y1, int y2, int x)
    {
        for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
        {
            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                map[x, y] = 1;
        }
    }

    void SetWalls()
    {
        // Floorの周囲に壁を配置
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (map[x, y] == 1)
                {
                    // 8方向をチェック
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight)
                            {
                                if (map[nx, ny] == 0)
                                {
                                    map[nx, ny] = 2; // 2は壁を表す
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void DrawMap()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                if (map[x, y] == 1)
                {
                    floorTilemap.SetTile(tilePosition, floorTile);
                }
                else if (map[x, y] == 2)
                {
                    wallTilemap.SetTile(tilePosition, wallTile);
                }
            }
        }
    }

    // プレイヤーをランダムな部屋内のFloorタイルに配置
    void PlacePlayer()
    {
        if (PlayerController.Instance != null)
        {
            if (rooms.Count == 0)
            {
                Debug.LogError("部屋が一つも生成されていません。");
                return;
            }

            Room randomRoom = rooms[UnityEngine.Random.Range(0, rooms.Count)];
            Vector2 playerPos = randomRoom.GetRandomFloorPosition();

            // プレイヤーの位置を設定
            PlayerController.Instance.SetPosition(playerPos);
        }
        else
        {
            Debug.LogError("PlayerControllerのインスタンスが見つかりません。");
        }
    }
}

public class Room
{
    public Rect rect;
    public Vector2 center;

    public Room(Rect rect)
    {
        this.rect = rect;
        this.center = new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2);
    }

    // 部屋内のランダムなFloorタイルの位置を取得
    public Vector2 GetRandomFloorPosition()
    {
        int x = UnityEngine.Random.Range((int)rect.x + 1, (int)(rect.x + rect.width - 1));
        int y = UnityEngine.Random.Range((int)rect.y + 1, (int)(rect.y + rect.height - 1));
        return new Vector2(x, y);
    }

    // Floorタイルが存在するかを確認（ここでは常にtrueとしますが、必要に応じて条件を追加）
    public bool HasFloorTiles()
    {
        return rect.width > 0 && rect.height > 0;
    }
}
