using UnityEngine;

public class MoveUpAndDisappear : MonoBehaviour
{
    public float moveDistance = 5f;
    public float moveSpeed = 2f;

    private Vector3 m_StartPosition;
    private bool m_IsMoving;

    public bool IsMoving
    {
        get
        {
            return m_IsMoving;
        }
    }

    public void Trigger()
    {
        m_StartPosition = transform.position;
        m_IsMoving = true;
    }

    public void Stop()
    {
        m_IsMoving = false;
    }

    void Update()
    {
        if (!m_IsMoving) return;

        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(m_StartPosition, transform.position) >= moveDistance)
        {
            m_IsMoving = false;
            gameObject.SetActive(false);
        }
    }
}
