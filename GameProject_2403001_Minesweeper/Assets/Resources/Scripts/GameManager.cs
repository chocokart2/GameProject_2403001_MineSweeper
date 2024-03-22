using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour
{
    public enum TileType
    {
        Safe,
        Bomb
    }

    public class TileInfo
    {
        public TileType type;
        public bool isDiscovered;
        public bool isMarked;
        public GameObject gameObject;
        public int x;
        public int y;
    }

    public TileInfo[,] tileInfo;

    private Vector3 leftTopPosition;
    [SerializeField]
    private int sizeX = 25;
    [SerializeField]
    private int sizeY = 25;
    [SerializeField]
    private int bombCount = 156;
    private int safeTileLeftCount;
    private bool isGameOver;
    private bool isFirstExplore;

    private GameObject TilesParentGameObject;
    private GameObject UnexploredGameObject;
    private GameObject MarkedGameObject;
    private GameObject[] NumberGameObject;
    private GameObject EmptyGameObject;
    private GameObject BombGameObject;

    public void Init()
    {
        Assert.IsNotNull(TilesParentGameObject, "Tiles 게임오브젝트를 찾을 수 없습니다.");
        Assert.IsNotNull(UnexploredGameObject, "UnexploredGameObject 에셋을 찾을 수 없습니다.");
        // 맵 청소
        if (tileInfo != null)
        {
            foreach (TileInfo one in tileInfo)
            {
                if (one.gameObject == null) continue;
                Destroy(one.gameObject);
            }
        }

        // 시작하기
        tileInfo = new TileInfo[sizeX, sizeY];
        leftTopPosition = new Vector3(((float)sizeX - 1) / 2, 0, ((float)sizeY - 1) / 2);
        for (int yIndex = 0; yIndex < sizeY; yIndex++)
        {
            for (int xIndex = 0; xIndex < sizeX; xIndex++)
            {
                tileInfo[xIndex, yIndex] = new TileInfo()
                {
                    type = TileType.Safe,
                    isDiscovered = false,
                    isMarked = false,
                    gameObject = Instantiate(
                        UnexploredGameObject, 
                        leftTopPosition - new Vector3(xIndex, 0, yIndex),
                        UnexploredGameObject.transform.rotation),
                    x = xIndex,
                    y = yIndex
                };
                //tileInfo[xIndex, yIndex].gameObject.transform.parent
                //    = TilesParentGameObject.transform;
                TileButtonController component
                    = tileInfo[xIndex, yIndex].gameObject.
                    GetComponent<TileButtonController>();
                component.x = xIndex;
                component.y = yIndex;
            }
        }

        // 무작위 위치에 폭탄 매설
        for (int bomb = 0; bomb < bombCount;)
        {
            int xPosition = Random.Range(0, sizeX - 1);
            int yPosition = Random.Range(0, sizeY - 1);

            if (tileInfo[xPosition, yPosition].type == TileType.Bomb)
            {
                continue;
            }
            tileInfo[xPosition, yPosition].type = TileType.Bomb;
            bomb++;
        }

        isGameOver = false;
        isFirstExplore = true;
        safeTileLeftCount = sizeX * sizeY - bombCount;
    }
    public void Explore(int x, int y)
    {
        //Debug.Log($"Explore() [x:{x}, y:{y}]");

        // 얼리 리턴 패턴
        if (isGameOver) { return; }
        if (isFirstExplore) { M_MoveBombs(x, y); }
        if (tileInfo[x, y].isDiscovered) { return; }
        if (tileInfo[x, y].isMarked) { return; }
        if (tileInfo[x, y].type == TileType.Bomb)
        {
            M_Terminate();
            M_PlaceBlock(BombGameObject, x, y);
            return;
        }

        tileInfo[x, y].isDiscovered = true;
        int nearBombCount = GetNearBombCount(x, y);
        if (nearBombCount == 0)
        {
            for (int dX = -1; dX < 2; ++dX)
            {
                for (int dY = -1; dY < 2; ++dY)
                {
                    if (dX == 0 && dY == 0) continue;
                    if (dX + x < 0 || dX + x >= sizeX) continue;
                    if (dY + y < 0 || dY + y >= sizeY) continue;
                    Explore(x + dX, y + dY);
                }
            }
            M_PlaceBlock(EmptyGameObject, x, y);
        }
        else
        {
            M_PlaceBlock(NumberGameObject[nearBombCount - 1], x, y);
        }
        
        safeTileLeftCount--;

        if (safeTileLeftCount == 0)
        {
            M_GameWin();
        }
    }
    public void AutoExplore(int x, int y)
    {
        if (tileInfo[x, y].isDiscovered == false) return;
        List<TileInfo> tileInfos = M_GetNearTileList(x, y);
        int sum = 0;
        for (int i = 0; i < tileInfos.Count; ++i)
        {
            if (tileInfos[i].isMarked) sum--;
            if (tileInfos[i].type == TileType.Bomb) sum++;
        }
        if (sum != 0) return;

        for (int i = 0; i < tileInfos.Count; ++i)
        {
            Explore(tileInfos[i].x, tileInfos[i].y);
        }
    }
    public void SwichFlagStat(int x, int y)
    {
        if (tileInfo[x, y].isDiscovered) return;

        if (tileInfo[x, y].isMarked)
        {
            M_PlaceBlock(UnexploredGameObject, x, y);
            tileInfo[x, y].isMarked = false;
        }
        else
        {
            M_PlaceBlock(MarkedGameObject, x, y);
            tileInfo[x, y].isMarked = true;
        }
    }

    public int GetNearBombCount(int x, int y)
    {
        int result = 0;
        List<TileInfo> nearTiles = M_GetNearTileList(x, y);
        for (int index = 0; index < nearTiles.Count; ++index)
        {
            if (nearTiles[index].type == TileType.Bomb)
            {
                result++;
            }
        }
        return result;
    }


    private void M_MoveBombs(int x, int y)
    {
        List<TileInfo> nearTiles = M_GetNearTileList(x, y);
        nearTiles.Add(tileInfo[x, y]);
        isFirstExplore = false;

        for (int index = 0; index < nearTiles.Count;)
        {
            if (nearTiles[index].type == TileType.Safe)
            {
                ++index;
                continue;
            }

            int xPosition = Random.Range(0, sizeX - 1);
            int yPosition = Random.Range(0, sizeY - 1);

            if (Mathf.Abs(x - xPosition) < 2 && Mathf.Abs(y - yPosition) < 2) continue;
            if (tileInfo[xPosition, yPosition].type == TileType.Bomb) continue;

            //Debug.Log($"M_MoveBombs({x}, {y}) : 폭탄이 [{xPosition}, {yPosition}]에 배치됨");

            nearTiles[index].type = TileType.Safe;
            tileInfo[xPosition, yPosition].type = TileType.Bomb;
            ++index;
        }
    }
    private void M_PlaceBlock(GameObject blockPrefab, int x, int y)
    {
        GameObject temp = tileInfo[x, y].gameObject;

        tileInfo[x, y].gameObject = Instantiate(
            blockPrefab,
            leftTopPosition - new Vector3(x, 0, y),
            blockPrefab.transform.rotation
            );
        TileButtonController component
            = tileInfo[x, y].gameObject.GetComponent<TileButtonController>();
        if (component != null)
        {
            component.x = x; component.y = y;
        }
        //tileInfo[x, y].gameObject.transform.parent = TilesParentGameObject.transform;
        Destroy(temp);
    }
    private void M_Terminate()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("Game Over!");

        for (int xIndex = 0; xIndex < sizeX; ++xIndex)
        {
            for (int yIndex = 0; yIndex < sizeY; ++yIndex)
            {
                if (tileInfo[xIndex, yIndex].type == TileType.Safe) { continue; }
                M_PlaceBlock(BombGameObject, xIndex, yIndex);
            }
        }
    }
    private void M_GameWin()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("You Win!");
    }
    private List<TileInfo> M_GetNearTileList(int x, int y)
    {
        List<TileInfo> result = new List<TileInfo>();
        for (int dx = -1; dx < 2; ++dx)
        {
            for (int dy = -1; dy < 2; ++dy)
            {
                if (dx == 0 && dy == 0) continue;
                if (dx + x < 0 || dx + x >= sizeX) continue;
                if (dy + y < 0 || dy + y >= sizeY) continue;
                result.Add(tileInfo[x + dx, y + dy]);
            }
        }
        return result;
    }

    // Start is called before the first frame update
    void Start()
    {
        TilesParentGameObject = GameObject.Find("Tiles");

        string folderPath = "Prefabs/";
        UnexploredGameObject = Resources.Load<GameObject>($"{folderPath}TileUnexplored Variant");
        MarkedGameObject = Resources.Load<GameObject>($"{folderPath}TileSus Variant");
        NumberGameObject = new GameObject[8];
        NumberGameObject[0] = Resources.Load<GameObject>($"{folderPath}TileNum1 Variant");
        NumberGameObject[1] = Resources.Load<GameObject>($"{folderPath}TileNum2 Variant");
        NumberGameObject[2] = Resources.Load<GameObject>($"{folderPath}TileNum3 Variant");
        NumberGameObject[3] = Resources.Load<GameObject>($"{folderPath}TileNum4 Variant");
        NumberGameObject[4] = Resources.Load<GameObject>($"{folderPath}TileNum5 Variant");
        NumberGameObject[5] = Resources.Load<GameObject>($"{folderPath}TileNum6 Variant");
        NumberGameObject[6] = Resources.Load<GameObject>($"{folderPath}TileNum7 Variant");
        NumberGameObject[7] = Resources.Load<GameObject>($"{folderPath}TileNum8 Variant");
        EmptyGameObject = Resources.Load<GameObject>($"{folderPath}TileEmpty Variant");
        BombGameObject = Resources.Load<GameObject>($"{folderPath}TileBomb Variant");

        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
