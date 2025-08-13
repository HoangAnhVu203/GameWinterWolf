using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public BottomBarManager bottomBarManager;
    private Board m_board;
    private GameManager m_gameManager;
    private Camera m_cam;
    private bool m_gameOver;

    private bool isAutoplay = false;
    private UIPanelGameOver panelGameOver;

    private Coroutine timeAttackTimer;
    private float timeAttackDuration = 60f;

    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        m_gameManager = gameManager;
        m_cam = Camera.main;
        m_board = new Board(this.transform, gameSettings);
        m_board.Fill();

        bottomBarManager = m_gameManager.bottomBarManager;
        bottomBarManager.OnWin = ShowWin;
        bottomBarManager.OnLose = ShowLose;

        foreach (var cell in FindObjectsOfType<Cell>())
            cell.bottomBarManager = bottomBarManager;

        panelGameOver = FindObjectOfType<UIPanelGameOver>(true);
        if (panelGameOver == null)
        {
            Debug.LogError("Không tìm thấy UIPanelGameOver trên scene!");
        }
    }

    public void Update()
    {
        if (m_gameOver || isAutoplay) return;

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                Cell cell = hit.collider.GetComponent<Cell>();
                if (cell == null)
                {
                    cell = hit.collider.GetComponentInParent<Cell>();
                }

                var clickedItem = cell != null ? cell.Item : null;
                if (cell != null && clickedItem != null && !bottomBarManager.IsFull())
                {
                    NormalItem normal = clickedItem as NormalItem;
                    if (normal != null && normal.View != null)
                    {
                        SpriteRenderer sr = normal.View.GetComponentInChildren<SpriteRenderer>();
                        if (sr == null || sr.sprite == null)
                        {
                            return;
                        }

                        Vector3 fromWorldPos = cell.transform.position;
                        bottomBarManager.AddItem(normal.ItemType, sr.sprite, fromWorldPos);

                        cell.ExplodeItem();
                        StartCoroutine(DelaySpawnFish(cell, 0.5f));

                        if (IsAllItemsCleared())
                            bottomBarManager.OnWin?.Invoke();
                    }
                }
            }
        }
    }

    private IEnumerator DelaySpawnFish(Cell cell, float delay)
    {
        yield return new WaitForSeconds(delay);
        m_board.OnCellExploded(cell);
    }

    private bool IsAllItemsCleared()
    {
        foreach (Cell cell in FindObjectsOfType<Cell>())
            if (!cell.IsEmpty) return false;
        return true;
    }

    private void ShowWin()
    {
        m_gameOver = true;
        if (panelGameOver != null)
        {
            panelGameOver.gameObject.SetActive(true);
            panelGameOver.Show(true);
        }
        bottomBarManager.Hide();
        Debug.Log("You Win!");
    }

    private void ShowLose()
    {
        m_gameOver = true;
        if (panelGameOver != null)
        {
            panelGameOver.gameObject.SetActive(true);
            panelGameOver.Show(false);
        }
        if (bottomBarManager != null)
            bottomBarManager.Hide();
        Debug.Log("You Lose!");
    }

    internal void Clear()
    {
        m_board.Clear();
    }

    public void StartAutoplay()
    {
        isAutoplay = true;
        StartCoroutine(AutoplayCoroutine());
    }

    private int CountInBar(NormalItem.eNormalType type)
    {
        return bottomBarManager.CountType(type);
    }

    private IEnumerator AutoplayCoroutine()
    {
        while (!m_gameOver)
        {
            Cell[] cells = FindObjectsOfType<Cell>().OrderBy(x => UnityEngine.Random.value).ToArray();
            bool found = false;

            // 1. Ưu tiên triplet (2 con trên bar)
            found = AutoSelectAndPick(cells, typeCount: 2);
            if (!found)
            {
                // 2. Ưu tiên loại đã có 1 con trên bar
                found = AutoSelectAndPick(cells, typeCount: 1);
            }
            if (!found)
            {
                // 3. Ưu tiên loại chưa có trên bar
                found = AutoSelectAndPickNewType(cells);
            }
            if (!found)
            {
                // 4. Chọn bất kỳ cell hợp lệ
                found = AutoSelectAndPick(cells, typeCount: -1);
            }

            yield return new WaitForSeconds(0.5f);

            if (IsAllItemsCleared())
            {
                bottomBarManager.OnWin?.Invoke();
                break;
            }
        }
    }

    // typeCount = 2: chọn loại đã có 2 trên bar; typeCount = 1: đã có 1; typeCount = -1: bất kỳ hợp lệ
    private bool AutoSelectAndPick(Cell[] cells, int typeCount)
    {
        foreach (Cell cell in cells)
        {
            if (cell != null && cell.Item != null && !bottomBarManager.IsFull())
            {
                NormalItem normal = cell.Item as NormalItem;
                if (normal != null && normal.View != null)
                {
                    int count = CountInBar(normal.ItemType);
                    if (typeCount == -1 || count == typeCount)
                    {
                        SpriteRenderer sr = normal.View.GetComponentInChildren<SpriteRenderer>();
                        if (sr != null && sr.sprite != null)
                        {
                            Vector3 fromWorldPos = cell.transform.position;
                            bottomBarManager.AddItem(normal.ItemType, sr.sprite, fromWorldPos);
                            cell.ExplodeItem();
                            StartCoroutine(DelaySpawnFish(cell, 0.5f));
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    // Chọn loại cá chưa có trên bar
    private bool AutoSelectAndPickNewType(Cell[] cells)
    {
        var currentTypes = bottomBarManager.GetCurrentTypes();
        foreach (Cell cell in cells)
        {
            if (cell != null && cell.Item != null && !bottomBarManager.IsFull())
            {
                NormalItem normal = cell.Item as NormalItem;
                if (normal != null && normal.View != null && !currentTypes.Contains(normal.ItemType))
                {
                    SpriteRenderer sr = normal.View.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null && sr.sprite != null)
                    {
                        Vector3 fromWorldPos = cell.transform.position;
                        bottomBarManager.AddItem(normal.ItemType, sr.sprite, fromWorldPos);
                        cell.ExplodeItem();
                        StartCoroutine(DelaySpawnFish(cell, 0.5f));
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void StartAutoLose()
    {
        isAutoplay = true;
        StartCoroutine(AutoLoseCoroutine());
    }

    private IEnumerator AutoLoseCoroutine()
    {
        while (!m_gameOver)
        {
            Cell[] cells = FindObjectsOfType<Cell>().OrderBy(x => UnityEngine.Random.value).ToArray();
            var currentTypes = bottomBarManager.GetCurrentTypes();

            bool found = false;

            // 1. Ưu tiên chọn loại chưa có trên bar
            foreach (Cell cell in cells)
            {
                if (cell != null && cell.Item != null && !bottomBarManager.IsFull())
                {
                    NormalItem normal = cell.Item as NormalItem;
                    if (normal != null && normal.View != null && !currentTypes.Contains(normal.ItemType))
                    {
                        SpriteRenderer sr = normal.View.GetComponentInChildren<SpriteRenderer>();
                        if (sr != null && sr.sprite != null)
                        {
                            Vector3 fromWorldPos = cell.transform.position;
                            bottomBarManager.AddItem(normal.ItemType, sr.sprite, fromWorldPos);
                            cell.ExplodeItem();
                            StartCoroutine(DelaySpawnFish(cell, 0.5f));
                            found = true;
                            break;
                        }
                    }
                }
            }

            // 2. Nếu không còn loại mới, chọn bất kỳ cell còn lại
            if (!found)
            {
                foreach (Cell cell in cells)
                {
                    if (cell != null && cell.Item != null && !bottomBarManager.IsFull())
                    {
                        NormalItem normal = cell.Item as NormalItem;
                        if (normal != null && normal.View != null)
                        {
                            SpriteRenderer sr = normal.View.GetComponentInChildren<SpriteRenderer>();
                            if (sr != null && sr.sprite != null)
                            {
                                Vector3 fromWorldPos = cell.transform.position;
                                bottomBarManager.AddItem(normal.ItemType, sr.sprite, fromWorldPos);
                                cell.ExplodeItem();
                                StartCoroutine(DelaySpawnFish(cell, 0.5f));
                                break;
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);

            if (IsAllItemsCleared())
            {
                bottomBarManager.OnWin?.Invoke();
                break;
            }
        }
    }

    public void StartTimeAttack()
    {
        isAutoplay = false;
        m_gameOver = false;
        if (timeAttackTimer != null) StopCoroutine(timeAttackTimer);
        timeAttackTimer = StartCoroutine(TimeAttackCoroutine());
    }

    private IEnumerator TimeAttackCoroutine()
    {
        float timer = timeAttackDuration;
        while (timer > 0 && !m_gameOver)
        {
            timer -= Time.deltaTime;
            yield return null;
            if (IsAllItemsCleared())
            {
                bottomBarManager.OnWin?.Invoke();
                yield break;
            }
        }
        if (!IsAllItemsCleared())
        {
            // Hết giờ mà chưa xong thì thua
            bottomBarManager.OnLose?.Invoke();
        }
    }
}