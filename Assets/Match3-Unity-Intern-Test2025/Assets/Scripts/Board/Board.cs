using System;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    private int boardSizeX;
    private int boardSizeY;
    private Cell[,] m_cells;
    private Transform m_root;
    private Dictionary<NormalItem.eNormalType, int> typeCounts;
    private const int MaxPerType = 6;

    public Board(Transform transform, GameSettings gameSettings)
    {
        m_root = transform;
        this.boardSizeX = gameSettings.BoardSizeX;
        this.boardSizeY = gameSettings.BoardSizeY;
        m_cells = new Cell[boardSizeX, boardSizeY];
        CreateBoard();
        InitTypeCounts();
    }

    private void InitTypeCounts()
    {
        typeCounts = new Dictionary<NormalItem.eNormalType, int>();
        foreach (NormalItem.eNormalType type in Enum.GetValues(typeof(NormalItem.eNormalType)))
            typeCounts[type] = 0;
    }

    private void CreateBoard()
    {
        Vector3 origin = new Vector3(-boardSizeX * 0.5f + 0.5f, -boardSizeY * 0.5f + 0.5f, 0f);
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                GameObject go = GameObject.Instantiate(prefabBG);
                go.transform.position = origin + new Vector3(x, y, 0f);
                go.transform.SetParent(m_root);

                Cell cell = go.GetComponent<Cell>();
                cell.Setup(x, y);

                m_cells[x, y] = cell;
            }
        }
    }

    internal void Fill()
    {
        InitTypeCounts();
        // Tạo pool các loại cá, mỗi loại 6 con
        List<NormalItem.eNormalType> fishPool = new List<NormalItem.eNormalType>();
        foreach (NormalItem.eNormalType type in Enum.GetValues(typeof(NormalItem.eNormalType)))
        {
            for (int i = 0; i < MaxPerType; i++)
                fishPool.Add(type);
        }

        // Xáo trộn pool
        System.Random rng = new System.Random();
        int n = fishPool.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = fishPool[k];
            fishPool[k] = fishPool[n];
            fishPool[n] = value;
        }

        int poolIdx = 0;
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                if (poolIdx < fishPool.Count)
                {
                    var type = fishPool[poolIdx++];
                    NormalItem item = new NormalItem(type);
                    item.SetType(type);
                    item.SetView();
                    item.SetViewRoot(m_root);

                    cell.Assign(item);
                    cell.ApplyItemPosition(false);
                    typeCounts[type]++;
                }
                else
                {
                    cell.Clear();
                }
            }
        }
    }

    public void OnCellExploded(Cell cell)
    {
        // Giảm count cho loại cá cũ nếu muốn (không bắt buộc vì tổng sẽ tăng lại bên dưới nếu có spawn tiếp)
        if (cell.Item is NormalItem normalItem)
        {
            var explodedType = normalItem.ItemType;
            if (typeCounts.ContainsKey(explodedType) && typeCounts[explodedType] > 0)
                typeCounts[explodedType]--;
        }
        // Spawn cá mới nếu còn loại nào < MaxPerType
        NormalItem.eNormalType type = GetRandomAvailableType();
        if (type == (NormalItem.eNormalType)(-1))
        {
            cell.Clear();
            return;
        }
        NormalItem item = new NormalItem(type);
        item.SetType(type);
        item.SetView();
        item.SetViewRoot(m_root);

        cell.Assign(item);
        cell.ApplyItemPosition(false);

        typeCounts[type]++;
    }

    private NormalItem.eNormalType GetRandomAvailableType()
    {
        List<NormalItem.eNormalType> available = new List<NormalItem.eNormalType>();
        foreach (var kv in typeCounts)
        {
            if (kv.Value < MaxPerType)
                available.Add(kv.Key);
        }
        if (available.Count == 0)
            return (NormalItem.eNormalType)(-1);

        int idx = UnityEngine.Random.Range(0, available.Count);
        return available[idx];
    }

    public void Clear()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                cell.Clear();
                GameObject.Destroy(cell.gameObject);
                m_cells[x, y] = null;
            }
        }
        typeCounts = null;
    }
}