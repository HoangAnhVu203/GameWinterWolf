using System;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public BottomBarManager bottomBarManager;

    public int BoardX { get; private set; }
    public int BoardY { get; private set; }
    public Item Item { get; private set; }
    public Cell NeighbourUp { get; set; }
    public Cell NeighbourRight { get; set; }
    public Cell NeighbourBottom { get; set; }
    public Cell NeighbourLeft { get; set; }

    public bool IsEmpty => Item == null;

    public void Setup(int cellX, int cellY)
    {
        this.BoardX = cellX;
        this.BoardY = cellY;
    }

    public bool IsNeighbour(Cell other)
    {
        return BoardX == other.BoardX && Mathf.Abs(BoardY - other.BoardY) == 1 ||
            BoardY == other.BoardY && Mathf.Abs(BoardX - other.BoardX) == 1;
    }

    public void Free()
    {
        Item = null;
    }

    public void Assign(Item item)
    {
        Item = item;
        Item.SetCell(this);
    }

    public void ApplyItemPosition(bool withAppearAnimation)
    {
        Item.SetViewPosition(this.transform.position);

        if (withAppearAnimation)
        {
            Item.ShowAppearAnimation();
        }
    }

    internal void Clear()
    {
        if (Item != null)
        {
            Item.Clear();
            Item = null;
        }
    }

    internal bool IsSameType(Cell other)
    {
        return Item != null && other.Item != null && Item.IsSameType(other.Item);
    }

    internal void ExplodeItem()
    {
        if (Item == null) return;

        Item.ExplodeView();
        Item = null;
    }

    internal void AnimateItemForHint()
    {
        Item.AnimateForHint();
    }

    internal void StopHintAnimation()
    {
        Item.StopAnimateForHint();
    }

    internal void ApplyItemMoveToPosition()
    {
        Item.AnimationMoveToPosition();
    }

    public void RespawnItem(NormalItem.eNormalType type, Sprite sprite)
    {
        // Xóa item cũ nếu có
        if (Item != null && Item.View != null)
        {
            // Nếu View là GameObject:
            GameObject go = Item.Vieww as GameObject;
            if (go != null)
                GameObject.Destroy(go);
            else
            {
                // Nếu View là Transform:
                Transform tf = Item.View as Transform;
                if (tf != null)
                    GameObject.Destroy(tf.gameObject);
            }
        }
        NormalItem normalItem = new NormalItem(type);

        GameObject goNew = new GameObject("NormalItemView", typeof(SpriteRenderer));
        goNew.transform.SetParent(this.transform, false);
        goNew.transform.position = this.transform.position;
        var sr = goNew.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;

        // Gán View nếu có public set
        // Nếu không public, dùng hàm SetView nếu có
        normalItem.Viewn = goNew;

        this.Item = normalItem;
        this.Item.SetCell(this);
    }
}