using UnityEngine;
using UnityEngine.SceneManagement;

public class gameover : MonoBehaviour
{
    [SerializeField] private NPCconditionChat npc1; // 第一位NPC
    [SerializeField] private NPCconditionChat npc2; // 第二位NPC
    [SerializeField] private NPCconditionChat1 npc3; // 第二位NPC
    [SerializeField] private NPCconditionChat npc4;
    [SerializeField] private NPCconditionChat npc5;
    [SerializeField] private NPCconditionChat npc6;
    // 选择加载方式：固定索引或场景名（二选一）
    [SerializeField] private int sceneIndexToLoad = 7;
    // [SerializeField] private string sceneNameToLoad;

    private bool loading; // 防止重复加载

   

    private void Update()
    {
        if (loading) return;

        // 条件1：npc1 对话完成即可过关
        if (npc1 != null && (npc1.finished || npc3.finished ||npc4.finished || npc5.finished || npc6.finished))
        {
            LoadTargetScene();
            return;
        }

        // 条件2：npc2 对话完成 且 第2行（索引1）的第2或第3个选项被选择过
        if (npc2 != null && npc2.finished &&
            (IsChoiceChosen(npc2.npcDialogueLines, 1, 1) || IsChoiceChosen(npc2.npcDialogueLines, 1, 2)))
        {
            LoadTargetScene();
            return;
        }
    }

    private bool IsChoiceChosen(DialogueLine[] lines, int lineIndex, int choiceIndex)
    {
        // 安全检查（数组下标是从0开始的）
        if (lines == null) return false;
        if (lineIndex < 0 || lineIndex >= lines.Length) return false;

        var line = lines[lineIndex];
        if (!line.hasChoices || line.choices == null) return false;
        if (choiceIndex < 0 || choiceIndex >= line.choices.Length) return false;

        return line.choices[choiceIndex].wasChosen;
    }

    private void LoadTargetScene()
    {
        if (loading) return;
        loading = true;

        // 固定索引加载
        if (sceneIndexToLoad >= 0 && sceneIndexToLoad < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(sceneIndexToLoad, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError($"sceneIndexToLoad={sceneIndexToLoad} 不在 Build Settings 范围内。");
            loading = false;
        }

        // 或者按场景名加载（把上面的索引加载注释掉，启用下面这段）
        // if (!string.IsNullOrEmpty(sceneNameToLoad))
        // {
        //     SceneManager.LoadScene(sceneNameToLoad, LoadSceneMode.Single);
        // }
        // else
        // {
        //     Debug.LogError("未指定 sceneNameToLoad。");
        //     loading = false;
        // }
    }
}
