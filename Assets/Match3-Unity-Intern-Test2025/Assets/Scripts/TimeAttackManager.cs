using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TimeAttackManager: Thêm chức năng Time Attack mode mà KHÔNG sửa code cũ.
/// - Nút "Time Attack" (gọi StartTimeAttack)
/// - Không thua khi bar đầy, chỉ thua khi hết giờ.
/// - Cho phép trả lại item về ô cũ bằng cách chạm vào slot bar.
/// </summary>
public class TimeAttackManager : MonoBehaviour
{
    [Header("Tham chiếu đối tượng")]
    public Button btnTimeAttack; // Kéo từ Canvas vào (nút mới)
    public Text txtTimer;        // UI Text để hiển thị thời gian (nên tạo mới)
    public GameManager gameManager; // Kéo GameManager vào

    [Header("Cài đặt")]
    public float totalTime = 60f; // 1 phút

    private float timer = 60f;
    private bool isRunning = false;
    private bool isLose = false;
    private BoardController boardController;
    private BottomBarManager bottomBarManager;

    // Gọi ở Start hoặc Awake
    void Start()
    {
        btnTimeAttack.onClick.AddListener(() =>
        {
            // Chơi Time Attack Mode
            gameManager.LoadLevel(GameManager.eLevelMode.TIMER);
            StartCoroutine(WaitAndStartTimeAttack());
        });
    }

    private IEnumerator WaitAndStartTimeAttack()
    {
        // Đợi cho BoardController sinh ra (nếu cần)
        yield return new WaitForSeconds(0.1f);
        boardController = FindObjectOfType<BoardController>();
        bottomBarManager = FindObjectOfType<BottomBarManager>();

        // Cho phép trả lại item về ô cũ (hook extension)
        AddReturnItemToSlot(bottomBarManager);

        StartTimeAttack();
    }

    public void StartTimeAttack()
    {
        timer = totalTime;
        isRunning = true;
        isLose = false;
        txtTimer.gameObject.SetActive(true);
        StartCoroutine(TimeAttackCoroutine());
    }

    private IEnumerator TimeAttackCoroutine()
    {
        while (timer > 0 && isRunning && !isLose)
        {
            timer -= Time.deltaTime;
            UpdateTimerUI();
            if (IsBoardCleared())
            {
                OnWin();
                yield break;
            }
            yield return null;
        }

        if (!IsBoardCleared())
        {
            OnLose();
        }
    }

    private void UpdateTimerUI()
    {
        int t = Mathf.Max(0, Mathf.FloorToInt(timer));
        int min = t / 60;
        int sec = t % 60;
        txtTimer.text = string.Format("TIME:\n{0:00}:{1:00}", min, sec);
    }

    private bool IsBoardCleared()
    {
        foreach (Cell cell in FindObjectsOfType<Cell>())
            if (!cell.IsEmpty) return false;
        return true;
    }

    private void OnWin()
    {
        isRunning = false;
        txtTimer.gameObject.SetActive(false);
        if (bottomBarManager != null) bottomBarManager.OnWin?.Invoke();
    }

    private void OnLose()
    {
        isRunning = false;
        isLose = true;
        txtTimer.gameObject.SetActive(false);
        if (bottomBarManager != null) bottomBarManager.OnLose?.Invoke();
    }

    /// <summary>
    /// Thêm extension: cho phép trả lại item về ô cũ bằng cách chạm vào slot bar.
    /// </summary>
    private void AddReturnItemToSlot(BottomBarManager bar)
    {
        // Đảm bảo chỉ add 1 lần (tránh add nhiều lần nếu reload)
        foreach (var btn in bar.slotButtons)
        {
            btn.onClick.RemoveListener(() => { }); // Clean up dummy
        }

        for (int i = 0; i < bar.slotButtons.Length; i++)
        {
            int idx = i;
            bar.slotButtons[i].onClick.AddListener(() => ReturnItemToBoard(bar, idx));
        }
    }

    private void ReturnItemToBoard(BottomBarManager bar, int idx)
    {
        // Truy cập slotOriginCells qua reflection vì code cũ không public
        var slotOriginCellsField = bar.GetType().GetField("slotOriginCells", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var slotsField = bar.GetType().GetField("slots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var slotSpritesField = bar.GetType().GetField("slotSprites", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (slotOriginCellsField == null || slotsField == null || slotSpritesField == null) return;

        var slotOriginCells = slotOriginCellsField.GetValue(bar) as List<Cell>;
        var slots = slotsField.GetValue(bar) as List<NormalItem.eNormalType>;
        var slotSprites = slotSpritesField.GetValue(bar) as List<Sprite>;

        if (slotOriginCells == null || slots == null || slotSprites == null) return;
        if (idx >= slotOriginCells.Count) return;

        var cell = slotOriginCells[idx];
        if (cell != null && cell.IsEmpty)
        {
            cell.RespawnItem(slots[idx], slotSprites[idx]);
            slots.RemoveAt(idx);
            slotSprites.RemoveAt(idx);
            slotOriginCells.RemoveAt(idx);

            // Gọi lại UpdateBarUI (qua reflection)
            var updateBarUIMethod = bar.GetType().GetMethod("UpdateBarUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (updateBarUIMethod != null)
                updateBarUIMethod.Invoke(bar, null);
        }
    }
}