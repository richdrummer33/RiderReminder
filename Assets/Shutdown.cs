using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shutdown : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(Quit()); 
    }

    IEnumerator Quit()
    {
        yield return new WaitForSeconds(5f);
        Application.Quit();
    }
    
    
}
