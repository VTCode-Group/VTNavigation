using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowCamera : MonoBehaviour
{
    public Vector3 m_Offset;

    public float m_FlowPower;

    public Transform m_Player;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void LateUpdate()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        Transform mainCameraTransform = mainCamera.transform;
        Vector3 playerPos = m_Player.position;
        Vector3 cameraPosition = playerPos + m_Offset.x * m_Player.right + m_Offset.y * m_Player.up +
                                 m_Offset.z * m_Player.forward;
        mainCameraTransform.position =
            Vector3.Lerp(mainCameraTransform.position, cameraPosition, m_FlowPower * Time.deltaTime);
        mainCameraTransform.LookAt(m_Player);
    }
}