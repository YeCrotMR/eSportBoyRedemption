using UnityEngine;
using UnityEngine.SceneManagement;

public class gameover2 : MonoBehaviour
{

    // 选择加载方式：固定索引或场景名（二选一）
    [SerializeField] private int sceneIndexToLoad = 7;
    // [SerializeField] private string sceneNameToLoad;
    public NPCconditionChat n1;
    public NPCconditionChat n2;
    public NPCconditionChat n3;
    public NPCconditionChat n4;
    public NPCconditionChat n5;
    public NPCcontinueMover r1;
    public NPCcontinueMover r2;
    public NPCcontinueMover r3;
    public InteractDialogueTrigger t1;
    
    
    public static bool momisrunning = false;
    
    private bool loading; // 防止重复加载

    private void Update()
    {
        if (loading) return;

        if(r1?.isMoving == true || r2?.isMoving == true|| r3?.isMoving == true){
            momisrunning = true;
        }

        // 条件1：npc1 对话完成即可过关
        if (n1?.finished == true || n2?.finished == true|| n3?.finished == true || n4?.finished == true || n5?.finished == true || t1?.finished == true)
        {
            LoadTargetScene();
            Debug.Log("傻逼");
            return;
        }

       
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
