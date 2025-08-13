using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BottomBarManager : MonoBehaviour
{
    public Button[] slotButtons;
    private List<NormalItem.eNormalType> slots = new List<NormalItem.eNormalType>();
    private List<Sprite> slotSprites = new List<Sprite>();
    private List<Cell> slotOriginCells = new List<Cell>();

    public System.Action OnWin;
    public System.Action OnLose;

    private bool isClearing = false;

    public bool IsFull() => slots.Count >= slotButtons.Length || isClearing;

    public void Show()
    {
        this.gameObject.SetActive(true);
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// fromWorldPos: Vị trí thế giới của cell (cell.transform.position), sẽ tự động chuyển sang canvas pos cho hiệu ứng.
    /// </summary>
    public void AddItem(NormalItem.eNormalType type, Sprite sprite, Vector3 fromWorldPos, Cell originCell)
    {
        if (IsFull()) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas cha!");
            return;
        }

        Vector3 fromScreenPos = Camera.main.WorldToScreenPoint(fromWorldPos);

        
        GameObject flyingIcon = new GameObject("FlyingIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
        var img = flyingIcon.GetComponent<UnityEngine.UI.Image>();
        img.sprite = sprite;
        img.raycastTarget = false;

        flyingIcon.transform.SetParent(canvas.transform, false);

        RectTransform flyingRect = flyingIcon.GetComponent<RectTransform>();
        flyingRect.sizeDelta = slotButtons[0].GetComponent<RectTransform>().sizeDelta;
        flyingRect.position = fromScreenPos;

        Vector3 toScreenPos = slotButtons[slots.Count].transform.position;

        flyingRect.DOMove(toScreenPos, 0.35f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            Destroy(flyingIcon);

            // Sau khi bay xong - add vào bar
            slots.Add(type);
            slotSprites.Add(sprite);
            slotOriginCells.Add(originCell); // LƯU cell gốc

            var barImg = slotButtons[slots.Count - 1].GetComponent<UnityEngine.UI.Image>();
            barImg.sprite = sprite;
            barImg.enabled = true;

            CheckAndClearTriplets();
            if (IsFull() && !HasTriplet())
                OnLose?.Invoke();
        });
    }

    // Nếu các nơi khác gọi AddItem (cũ), overload hàm cũ để tránh lỗi:
    public void AddItem(NormalItem.eNormalType type, Sprite sprite, Vector3 fromWorldPos)
    {
        AddItem(type, sprite, fromWorldPos, null);
    }
    private void ReturnItemToBoard(int idx)
    {
        if (idx >= slots.Count) return;
        var cell = slotOriginCells[idx];
        if (cell != null && cell.IsEmpty)
        {
            cell.RespawnItem(slots[idx], slotSprites[idx]);
            slots.RemoveAt(idx);
            slotSprites.RemoveAt(idx);
            slotOriginCells.RemoveAt(idx);
            UpdateBarUI();
        }
    }

    // Gắn listener cho các slot button (chỉ cần gọi 1 lần, ví dụ trong Awake)
    void Awake()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int idx = i;
            slotButtons[i].onClick.AddListener(() => ReturnItemToBoard(idx));
        }
    }

    private void CheckAndClearTriplets()
    {
        var group = slots.GroupBy(i => i).Where(g => g.Count() >= 3).FirstOrDefault();
        if (group != null)
        {
            if (!isClearing)
                StartCoroutine(ClearTripletsWithDelay(group.Key));
        }
    }

    private IEnumerator ClearTripletsWithDelay(NormalItem.eNormalType type)
    {
        isClearing = true;
        yield return new WaitForSeconds(0.5f);

        int count = 0;
        List<int> idxToClear = new List<int>();
        for (int i = slots.Count - 1; i >= 0 && count < 3; i--)
        {
            if (slots[i].Equals(type))
            {
                idxToClear.Add(i);
                count++;
            }
        }

        // Animate scale về 0
        foreach (int i in idxToClear)
        {
            var img = slotButtons[i].GetComponent<Image>();
            img.transform.DOScale(Vector3.zero, 0.3f);
        }
        yield return new WaitForSeconds(0.3f);

        // Xóa thực sự
        foreach (int i in idxToClear.OrderByDescending(x => x))
        {
            var img = slotButtons[i].GetComponent<Image>();
            img.sprite = null;
            img.enabled = false;
            img.transform.localScale = Vector3.one;

            slots.RemoveAt(i);
            slotSprites.RemoveAt(i);
        }

        UpdateBarUI();
        isClearing = false;

        CheckAndClearTriplets();
    }

    private bool HasTriplet()
    {
        return slots.GroupBy(i => i).Any(g => g.Count() >= 3);
    }

    private void UpdateBarUI()
    {
        // Dồn các hình còn lại về slot đầu tiên, đồng bộ cả type và sprite
        for (int i = 0; i < slotButtons.Length; i++)
        {
            var img = slotButtons[i].GetComponent<Image>();
            if (i < slotSprites.Count)
            {
                img.sprite = slotSprites[i];
                img.enabled = true;
                img.transform.localScale = Vector3.one;
            }
            else
            {
                img.sprite = null;
                img.enabled = false;
                img.transform.localScale = Vector3.one;
            }
        }
    }

    public void ResetBar()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            var img = slotButtons[i].GetComponent<Image>();
            img.sprite = null;
            img.enabled = false;
            slotButtons[i].interactable = true;
            img.transform.localScale = Vector3.one;
        }
        slots.Clear();
        slotSprites.Clear();
        isClearing = false;
    }

    // Đếm số lượng item type đang có trên bar
    public int CountType(NormalItem.eNormalType type)
    {
        int count = 0;
        foreach (var t in slots)
        {
            if (t == type) count++;
        }
        return count;
    }

    public List<NormalItem.eNormalType> GetCurrentTypes()
    {
        return new List<NormalItem.eNormalType>(slots);
    }


}