using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<Item> inventory = new List<Item>();
    public Image[] itemSlots; // 物品槽图片（UI）
    public Text popupText;    // 弹窗提示文字
    public float appearTime;
    public float disappearTime;
    public float entireTime;
    private Coroutine popupCoroutine;

    public GameObject inventoryUI; // ✅ 拖入整个物品栏 UI 的根节点

    [Header("槽位样式")]
    public Sprite normalSlotSprite;
    public Sprite highlightSlotSprite;

    public int selectedIndex = -1;

    [Header("物品信息弹窗")]
    public GameObject infoPanel;
    public Image infoIcon;
    public Text infoText;
    public float infoFadeInTime = 0.3f;
    public float infoFadeOutTime = 0.3f;

    private CanvasGroup infoCanvasGroup;
    private Coroutine infoCoroutine;
    private bool infoVisible = false;

    public void SetInventoryVisible(bool visible)
    {
        if (inventoryUI != null)
            inventoryUI.SetActive(visible);
    }

    void Awake()
    {
        // 如果已有实例（比如 DontDestroyOnLoad 保持的），而且不是自己，就销毁它
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetSelectedIndex(0);
        if (infoPanel != null)
        {
            infoCanvasGroup = infoPanel.GetComponent<CanvasGroup>();
            if (infoCanvasGroup == null)
                infoCanvasGroup = infoPanel.AddComponent<CanvasGroup>();

            infoCanvasGroup.alpha = 0f;
            infoPanel.SetActive(false);
        }
    }


    // public void OnEnable()
    // {
    //     SceneManager.sceneLoaded += OnSceneLoaded;
    // }

    // public void OnDisable()
    // {
    //     SceneManager.sceneLoaded -= OnSceneLoaded;
    // }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 重新绑定弹窗文字
        GameObject popupObj = GameObject.Find("ItemPopupText");
        if (popupObj != null)
        {
            popupText = popupObj.GetComponent<Text>();
            Color color = popupText.color;
            color.a = 0f;
            popupText.color = color;
        }

        // 重新绑定物品槽
        GameObject[] slots = GameObject.FindGameObjectsWithTag("ItemSlot");
        itemSlots = new Image[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            itemSlots[i] = slots[i].GetComponent<Image>();
        }

        RefreshUI();
        HighlightSelectedSlot();
    }

    public void Update()
    {
        HandleInput();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (infoVisible)
                HideItemInfo();
            else
                ShowItemInfo();
        }
        
                        
    }

    public void HandleInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll < 0f)
        {
            selectedIndex = (selectedIndex + 1) % itemSlots.Length;
            HighlightSelectedSlot();
        }
        else if (scroll > 0f)
        {
            selectedIndex = (selectedIndex - 1 + itemSlots.Length) % itemSlots.Length;
            HighlightSelectedSlot();
        }

        for (int i = 0; i < Mathf.Min(9, itemSlots.Length); i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
            {
                selectedIndex = i;
                HighlightSelectedSlot();
            }
        }
    }

    public void HighlightSelectedSlot()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            // 获取背景 Image（假设 itemSlot 的父物体是背景）
            Image bg = itemSlots[i].transform.parent.GetComponent<Image>();
            if (bg != null)
            {
                if (i == selectedIndex)
                {
                    bg.sprite = highlightSlotSprite;
                    bg.color = Color.white;
                }
                else
                {
                    bg.sprite = normalSlotSprite;
                    bg.color = Color.white;
                }
            }

            // 更新图标显示
            if (i < inventory.Count && inventory[i] != null)
            {
                itemSlots[i].sprite = inventory[i].icon;
                itemSlots[i].color = Color.white;
            }
            else
            {
                itemSlots[i].sprite = null;
                itemSlots[i].color = new Color(1, 1, 1, 0); // 图标透明
            }
        }

        if (selectedIndex >= 0 && selectedIndex < inventory.Count && inventory[selectedIndex] != null)
        {
            // popupText.text = inventory[selectedIndex].itemName;
            // Color textColor = popupText.color;
            // textColor.a = 1f;
            // popupText.color = textColor;
            
            ShowPopup(inventory[selectedIndex].itemName);
            if(inventory[selectedIndex].itemName == "iphone16 pro max"){
                GuideManager.Instance.ShowHintByID("pressF");
            }
            if(inventory[selectedIndex].itemName == "臭袜子"){
                GuideManager.Instance.ShowHintByID("pressC");
            }
        }
        else
        {
            popupText.text = "";
        }
    }

    public void AddItem(Item newItem)
    {
        if (inventory.Count >= itemSlots.Length)
        {
            ShowPopup("物品栏已满！");
            return;
        }

        inventory.Add(newItem);
        RefreshUI();
        ShowPopup("获得：" + newItem.itemName);
        if(newItem.itemName == "iphone16 pro max"){
                GuideManager.Instance.ShowHintByID("pressF");
            }
        if(newItem.itemName == "臭袜子"){
                GuideManager.Instance.ShowHintByID("pressC");
        }
        if(newItem.itemName == "学生卡"){
                GuideManager.Instance.ShowHintByID("pressQ");
        }
    }

    public void RefreshUI()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (i < inventory.Count && inventory[i] != null)
            {
                SetIconPreserveHeight(itemSlots[i], inventory[i].icon,125f);
                itemSlots[i].color = Color.white;
            }
            else
            {
                itemSlots[i].sprite = null;
                itemSlots[i].color = new Color(1, 1, 1, 0);
            }

            // 背景恢复为普通样式
            Image bg = itemSlots[i].transform.parent.GetComponent<Image>();
            if (bg != null)
            {
                bg.sprite = (i == selectedIndex ? highlightSlotSprite : normalSlotSprite);
                bg.color = Color.white;
            }
        }
    }

    public void ShowPopup(string message)
    {
        if (popupText != null)
        {
            popupText.text = message;

            if (popupCoroutine != null)
                StopCoroutine(popupCoroutine);

            popupCoroutine = StartCoroutine(FadePopup());
        }
    }

    public IEnumerator FadePopup()
    {
        Color color = popupText.color;

        for (float t = 0; t < appearTime; t += Time.deltaTime)
        {
            color.a = Mathf.Lerp(0, 1, t / appearTime);
            popupText.color = color;
            yield return null;
        }
        color.a = 1f;
        popupText.color = color;

        yield return new WaitForSeconds(entireTime);

        for (float t = 0; t < disappearTime; t += Time.deltaTime)
        {
            color.a = Mathf.Lerp(1, 0, t / disappearTime);
            popupText.color = color;
            yield return null;
        }
        color.a = 0f;
        popupText.color = color;
        popupText.text = "";

        popupCoroutine = null;
    }

    public bool HasItem(string itemName)
    {
        foreach (Item item in inventory)
        {
            if (item.itemName == itemName)
                return true;
        }
        return false;
    }
    public void ShowItemInfo()
    {
        if (selectedIndex >= 0 && selectedIndex < inventory.Count && inventory[selectedIndex] != null)
        {
            Item currentItem = inventory[selectedIndex];
            infoIcon.sprite = currentItem.icon;
            SetIconPreserveHeight(infoIcon, inventory[selectedIndex].icon,1000f);
            infoText.text = currentItem.description;

            if (infoCoroutine != null)
                StopCoroutine(infoCoroutine);

            infoCoroutine = StartCoroutine(FadeInfo(true));
        }
    }

    public void HideItemInfo()
    {
        if (infoCoroutine != null)
            StopCoroutine(infoCoroutine);

        infoCoroutine = StartCoroutine(FadeInfo(false));
    }
    public IEnumerator FadeInfo(bool fadeIn)
    {
        infoPanel.SetActive(true);
        float duration = fadeIn ? infoFadeInTime : infoFadeOutTime;
        float start = fadeIn ? 0f : 1f;
        float end = fadeIn ? 1f : 0f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(start, end, t / duration);
            infoCanvasGroup.alpha = alpha;
            yield return null;
        }

        infoCanvasGroup.alpha = end;

        if (!fadeIn)
            infoPanel.SetActive(false);

        infoVisible = fadeIn;
        infoCoroutine = null;
    }
    void SetIconPreserveHeight(Image img, Sprite sprite, float maxWidth)
{
    img.sprite = sprite;
    img.preserveAspect = true;

    if (sprite == null)
        return;

    float targetHeight = img.rectTransform.rect.height;
    float spriteWidth = sprite.rect.width;
    float spriteHeight = sprite.rect.height;

    float scale = targetHeight / spriteHeight;
    float scaledWidth = spriteWidth * scale;

    // 限制最大宽度
    if (scaledWidth > maxWidth)
    {
        scale = maxWidth / spriteWidth;
    }

    img.rectTransform.sizeDelta = new Vector2(spriteWidth * scale, spriteHeight * scale);
}
    public void SetSelectedIndex(int index)
    {
        selectedIndex = index;
        HighlightSelectedSlot();
    }
    public bool CraftItem(List<string> requiredItemNames, Item resultItem)
    {
        // 统计每种物品需要的数量
        Dictionary<string, int> requiredCount = new Dictionary<string, int>();
        foreach (string name in requiredItemNames)
        {
            if (!requiredCount.ContainsKey(name))
                requiredCount[name] = 0;
            requiredCount[name]++;
        }

        // 统计背包中拥有的物品
        Dictionary<string, int> ownedCount = new Dictionary<string, int>();
        foreach (Item item in inventory)
        {
            if (!ownedCount.ContainsKey(item.itemName))
                ownedCount[item.itemName] = 0;
            ownedCount[item.itemName]++;
        }

        // 检查是否拥有所有必需品
        foreach (var pair in requiredCount)
        {
            if (!ownedCount.ContainsKey(pair.Key) || ownedCount[pair.Key] < pair.Value)
            {
                ShowPopup("缺少材料: " + pair.Key);
                return false;
            }
        }

        // 删除材料
        foreach (var pair in requiredCount)
        {
            for (int i = 0; i < pair.Value; i++)
            {
                RemoveItemByName(pair.Key);
            }
        }

        // 添加合成结果
        AddItem(resultItem);
        ShowPopup("合成成功：" + resultItem.itemName);
        return true;
    }
    public void RemoveItemByName(string itemName)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].itemName == itemName)
            {
                inventory.RemoveAt(i);
                break;
            }
        }
        RefreshUI();
    }
    public void ClearInventory()
    {
        inventory.Clear();
        RefreshUI();
    }
}