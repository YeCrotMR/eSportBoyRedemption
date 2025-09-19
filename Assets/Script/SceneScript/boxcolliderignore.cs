using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boxcolliderignore : MonoBehaviour
{
    // Start is called before the first frame update
    // 2D 例子
    public Collider2D col1; // 物体1的Collider2D
    public Collider2D col2; // 物体2的Collider2D

    void Start()
    {
        Physics2D.IgnoreCollision(col1, col2, true);
    }


    // Update is called once per frame
    void Update()
    {
        Physics2D.IgnoreCollision(col1, col2, true);
    }
}
