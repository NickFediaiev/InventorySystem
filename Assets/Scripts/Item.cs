using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
}

public enum ItemState //used to check if the item is in the backpack
{
    Free,
    InBackpack
}

public class Item : MonoBehaviour
{
    [SerializeField] protected Guid _id;
    public Guid ID => _id;
    
    [SerializeField] protected string _name;
    public string Name => _name;

    [SerializeField] protected float _weight;
    public float Weight => _weight;

    [SerializeField] protected ItemType _type;
    public ItemType Type => _type;

    [SerializeField] protected Sprite _sprite;
    public Sprite Sprite => _sprite;

    [SerializeField] protected ItemState _state;
    public ItemState State => _state;

    //references
    [SerializeField] protected Rigidbody _rigidBody;

    //events used in item-inventory interactions
    private UnityEvent OnItemStashed;
    private UnityEvent OnItemRetrieved;

    void Start()
    {
        _id = new Guid();
        _state = ItemState.Free;

        //initialize item events
        if (OnItemStashed == null)
            OnItemStashed = new UnityEvent();
        
        OnItemStashed.AddListener(SendStashedMessage);

        if (OnItemRetrieved == null)
            OnItemRetrieved = new UnityEvent();

        OnItemRetrieved.AddListener(SendRetrievedMessage);
    }

    public void StashInBackpack(Transform socket)
    {
        _rigidBody.isKinematic = true;
        _state = ItemState.InBackpack;
        
        StartCoroutine(PutItemIntoBackpackSocket(socket));

        OnItemStashed?.Invoke();
    }

    public void RetrieveFromBackpack()
    {
        _rigidBody.isKinematic = false;
        _state = ItemState.Free;
        
        OnItemRetrieved?.Invoke();
    }

    void SendStashedMessage()
    {
        StartCoroutine(SendRequest(OnItemStashed.ToString()));
    }

    void SendRetrievedMessage()
    {
        StartCoroutine(SendRequest(OnItemRetrieved.ToString()));
    }
    
    IEnumerator SendRequest(string itemEvent)
    {
        WWWForm form = new WWWForm();
        form.AddField("id", _id.ToString());
        form.AddField("unityEvent", itemEvent);

        using (UnityWebRequest www = UnityWebRequest.Post("https://dev3r02.elysium.today/inventory/status", form))
        {
            www.SetRequestHeader("auth", "BMeHG5xqJeB4qCjpuJCTQLsqNGaqkfB6");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Request sent successfully");
            }
        }
    }

    /// <summary>
    /// Smooth transition to physical item socket on the inventory
    /// </summary>
    /// <param name="socket"></param>
    /// <returns></returns>
    protected IEnumerator PutItemIntoBackpackSocket(Transform socket)
    {
        var startPosition = transform.position;
        var startRotation = transform.rotation;
        
        var timePassed = 0f;
        
        while ((transform.position - socket.position).magnitude >= 0.01f)
        {
            transform.position = Vector3.Lerp(startPosition, socket.position, timePassed);
            transform.rotation = Quaternion.Lerp(startRotation, socket.rotation, timePassed);

            timePassed += Time.deltaTime;
            
            yield return null;
        }
    }

    #region MyRegion //item movement in Scene
    
    protected Vector3 mouseOffset;
    protected float zCoordinate;

    void OnMouseDown() //Start drag item if it's not stashed
    {
        if (_state == ItemState.InBackpack)
        {
            return;
        }
        zCoordinate = Camera.main.WorldToScreenPoint(transform.position).z;
        mouseOffset = gameObject.transform.position - GetMouseWorldPosition();
    }

    protected Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;

        mousePoint.z = zCoordinate;

        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
    
    void OnMouseDrag()
    {
        if (_state == ItemState.InBackpack)
        {
            return;
        }
        transform.position = GetMouseWorldPosition() + mouseOffset;
    }

    void OnMouseUp() //Try to put item into the backpack
    {
        if (_state == ItemState.InBackpack)
        {
            return;
        }
        
        _rigidBody.velocity = Vector3.zero;

        //Check if the mouse is over the Inventory
        int stashLayerMask = 1 << 8;

        RaycastHit hit;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 100f, stashLayerMask))
        {
            var backPack = hit.collider.gameObject.GetComponent<Inventory>();

            if (backPack != null)
            {
                if (!backPack.TryStashItem(this, out Transform socketTransform))
                {
                    Debug.LogError("Backpack is full");
                }
                else
                {
                    StashInBackpack(socketTransform);
                }
            }
            
            Debug.Log(hit.collider.gameObject.name);
        }
    }
    #endregion
}
