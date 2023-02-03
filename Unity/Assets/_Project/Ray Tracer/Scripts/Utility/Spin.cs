using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    bool spin = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (spin)
            transform.Rotate(-10f * Time.deltaTime, 40f * Time.deltaTime, 10f * Time.deltaTime, Space.Self);
    }

    public void spinToggle()
    {
        spin = !spin;
    }
}
