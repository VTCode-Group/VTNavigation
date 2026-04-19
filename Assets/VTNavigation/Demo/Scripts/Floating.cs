using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floating : MonoBehaviour
{
    public float m_FloatingPower = 1.0f;
    public float m_FloatingSpeed = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float localY =  m_FloatingPower * Mathf.Sin(Time.time * m_FloatingSpeed);
        transform.localPosition = new Vector3(0, localY, 0);
    }
}
