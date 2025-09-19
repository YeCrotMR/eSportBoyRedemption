using UnityEngine;

public class TestCrafting : MonoBehaviour
{
    public Recipe testRecipe; // Õœ»Î≈‰∑Ω

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            CraftingManager.Instance.Craft(testRecipe);
        }
    }
}