using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiController : MonoBehaviour
{
    public GameObject nextcanvous;
    public bool timeswitch = false;
    public float delay = 5f;
 
    public void Nextcanvous()
    {
        gameObject.active = false;
        nextcanvous.active = true;
    }

    private IEnumerator switchcanvasdelay()
    {
        float pulsederation = 0.05f;

        float currenttime = 0f;

        while (currenttime < delay)
        {
            Handheld.Vibrate();
            yield return new WaitForSeconds(pulsederation);
            currenttime += pulsederation;
            Debug.Log("current time " + currenttime);
        }
        
        
        Nextcanvous();

    }
    
   
    private void Awake()
    {
        if (timeswitch==true)
        {
            StartCoroutine(switchcanvasdelay());
        }
        
    }
}
    // Start is called before the first frame update
    
