using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameLoader : MonoBehaviour
{
    public static GameLoader Instance;
    private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);  // 关键
    }
    else
    {
        Destroy(gameObject);
    }
}


    public void LoadGame(int slot)
    {
        GameSaveData data = SaveSystem.LoadGame(slot);
        if (data == null) return;

        if (SceneManager.GetActiveScene().name != data.sceneName)
        {
            void OnSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                StartCoroutine(RestoreState(data));
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            FadeController.Instance.FadeAndLoadScene(data.sceneName);
        }
        else
        {
            StartCoroutine(RestoreState(data));
        }
    }


    public IEnumerator RestoreState(GameSaveData data)
    {
        //yield return new WaitForSeconds(0.1f); // 等待场景加载完，确保UI/物体就绪
        yield return null;

        // 1. 玩家位置
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);
        }

        // 2. 还原物品栏
        Item[] allItems = Resources.LoadAll<Item>("Items");
        var inventory = InventoryManager.Instance;

        inventory.inventory.Clear();
        foreach (string itemName in data.inventoryItemNames)
        {
            Item found = System.Array.Find(allItems, i => i.itemName == itemName);
            if (found != null)
                inventory.inventory.Add(found);
        }

        // 3. 还原选中物品槽并刷新 UI
        inventory.SetSelectedIndex(data.selectedIndex);
        inventory.RefreshUI();
    }
    
}