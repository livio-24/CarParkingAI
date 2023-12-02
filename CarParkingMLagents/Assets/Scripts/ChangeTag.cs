using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTag : MonoBehaviour
{
    public string tagName;
    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.tag = tagName;
        }
        gameObject.tag = tagName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
