using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
    //cashing references
    [SerializeField] private List<Socket> Sockets;
    [SerializeField] private InventoryUI InventoryUI;
    
    void Start()
    {
        InitializeInventory();
    }

    private void InitializeInventory()
    {
        if (Sockets == null || !Sockets.Any())
        {//item sockets must be set for the inventory
            Debug.LogError("Create sockets");
        }
    }

    #region MouseInteractions
    void OnMouseDown() //turn on Inventory UI on mouse press
    {
        if (InventoryUI != null)
        {
            InventoryUI.gameObject.SetActive(true);
        }
    }

    void OnMouseUp() // try to retrieve item and close inventory
    {
        TryRetrieveItem();

        if (InventoryUI != null)
        {
            InventoryUI.gameObject.SetActive(false);
        }
    }
    #endregion

    void TryRetrieveItem() //get selected socket and try to retrieve item
    {
        var socket = InventoryUI.GetItemSocket();
        
        if (socket != null && socket.Item != null)
        {
            socket.Item.RetrieveFromBackpack();
            socket.Item = null;
        }
    }

    public bool TryStashItem(Item item, out Transform socketTransform)//check if there is an empty socket and put item there
    {
        foreach (var socket in Sockets)
        {
            if (socket.Item != null || socket.ItemType != item.Type)
                continue;

            socket.Item = item;
            socketTransform = socket.transform;
            return true;
        }

        socketTransform = null;
        return false;
    }
}
