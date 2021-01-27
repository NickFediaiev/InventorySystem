using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    //caching references ti the Item slots on the UI
    [SerializeField] private List<UISlotRef> UIItemSlots;

    //caching components needed for Raycasting UI
    [SerializeField] private GraphicRaycaster _raycaster;
    [SerializeField] private PointerEventData _pointerEventData;
    [SerializeField] private EventSystem _eventSystem;

    void OnEnable()
    {
        InitializeUI();
    }

    void InitializeUI() //display non-empty item slots
    {
        ResetSprites();

        if (UIItemSlots != null && UIItemSlots.Any())
        {
            foreach (var uiSlot in UIItemSlots)
            {
                if (uiSlot.ItemSocket.Item != null)
                {
                    SetItemImage(uiSlot);
                }
            }
        }
    }

    void ResetSprites() //clear old images
    {
        foreach (var slot in UIItemSlots)
        {
            slot.ItemImage.sprite = null;
            slot.ItemImage.gameObject.SetActive(false);
        }
    }

    void SetItemImage(UISlotRef slot)
    {
        slot.ItemImage.sprite = slot.ItemSocket.Item.Sprite;
        slot.ItemImage.gameObject.SetActive(true);
        slot.ItemImage.preserveAspect = true;
    }

    /// <summary>
    /// Get item socket by mouse position on the UI
    /// </summary>
    /// <returns></returns>
    public Socket GetItemSocket()
    {
        //Set up the new Pointer Event position to the mouse position
        _pointerEventData = new PointerEventData(_eventSystem);
        _pointerEventData.position = Input.mousePosition;
        
        List<RaycastResult> results = new List<RaycastResult>();

        //Raycast using the Graphics Raycaster and mouse click position
        _raycaster.Raycast(_pointerEventData, results);

        foreach (var result in results)
        {
            var slot = result.gameObject.GetComponent<UISlotRef>();
            if (slot != null)
            {
                return slot.ItemSocket;
            }
        }

        return null;
    }
}
