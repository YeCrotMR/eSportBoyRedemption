using UnityEngine;
using System.IO;

public class SaveDeleter : MonoBehaviour
{
    private static string saveFolder => Application.persistentDataPath;
    public startpage startpage;
    public GameObject surepanel;
    public static int buttonindex;

    // 按钮调用此方法来删除某个存档
    public void DeleteSaveFromButton(int slotIndex)
    {
        buttonindex = slotIndex;
        if (SaveSystem.HasSave(slotIndex))
        {
            startpage.PushUI(surepanel);
            Debug.Log($"槽位 {slotIndex} 有存档");
        }
        else
        {
            Debug.Log($"槽位 {slotIndex} 没有存档，不执行删除");
        }
    }

    public static void DeleteSave()
    {
        string path = GetSaveFilePath(buttonindex);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"已成功删除存档文件：save_slot_{buttonindex}.json");
        }
        else
        {
            Debug.LogWarning($"找不到存档文件：save_slot_{buttonindex}.json");
        }
    }

    private static string GetSaveFilePath(int slotIndex)
    {
        return Path.Combine(saveFolder, $"save_slot_{slotIndex}.json");
    }
}
