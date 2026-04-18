//-----------------------------------------------------------------  
//1���ѱ�����Ϊһ������������� GameObject �С�  
//2����������ϵ��  
//-----------------------------------------------------------------  

using UnityEngine;
//-----------------------------------------------------------------  
public class FiCameraControl : MonoBehaviour
{
	public float moveSpeed = 30.0f;
	public float rotateSpeed = 0.2f;


	public static Vector3 kUpDirection = new Vector3(0.0f, 1.0f, 0.0f);


	//�����������ת�ĳ�Ա������  
	private float m_fLastMousePosX = 0.0f;
	private float m_fLastMousePosY = 0.0f;
	private bool m_bMouseRightKeyDown = false;


	//-----------------------------------------------------------------  
	void Start()
	{


	}
	//-----------------------------------------------------------------  
	void Update()
	{
		//�ж���ת  
		if (Input.GetMouseButtonDown(1)) //����Ҽ��ոհ�����  
		{
			if (m_bMouseRightKeyDown == false)
			{
				m_bMouseRightKeyDown = true;
				Vector3 kMousePos = Input.mousePosition;
				m_fLastMousePosX = kMousePos.x;
				m_fLastMousePosY = kMousePos.y;
			}
		}
		else if (Input.GetMouseButtonUp(1)) //����Ҽ��ո�̧����  
		{
			if (m_bMouseRightKeyDown == true)
			{
				m_bMouseRightKeyDown = false;
				m_fLastMousePosX = 0;
				m_fLastMousePosY = 0;
			}
		}
		else if (Input.GetMouseButton(1)) //����Ҽ����ڰ���״̬��  
		{
			if (m_bMouseRightKeyDown)
			{
				Vector3 kMousePos = Input.mousePosition;
				float fDeltaX = kMousePos.x - m_fLastMousePosX;
				float fDeltaY = kMousePos.y - m_fLastMousePosY;
				m_fLastMousePosX = kMousePos.x;
				m_fLastMousePosY = kMousePos.y;


				Vector3 kNewEuler = transform.eulerAngles;
				kNewEuler.x -= (fDeltaY * rotateSpeed);
				kNewEuler.y -= -(fDeltaX * rotateSpeed);
				transform.eulerAngles = kNewEuler;
			}
		}


		//�ж�λ��  
		float fMoveDeltaX = 0.0f;
		float fMoveDeltaZ = 0.0f;
		float fDeltaTime = Time.deltaTime;
		if (Input.GetKey(KeyCode.A))
		{
			fMoveDeltaX -= moveSpeed * fDeltaTime;
		}
		if (Input.GetKey(KeyCode.D))
		{
			fMoveDeltaX += moveSpeed * fDeltaTime;
		}
		if (Input.GetKey(KeyCode.W))
		{
			fMoveDeltaZ += moveSpeed * fDeltaTime;
		}
		if (Input.GetKey(KeyCode.S))
		{
			fMoveDeltaZ -= moveSpeed * fDeltaTime;
		}
		if (fMoveDeltaX != 0.0f || fMoveDeltaZ != 0.0f)
		{
			Vector3 kForward = transform.forward;
			Vector3 kRight = Vector3.Cross(kUpDirection, kForward);
			Vector3 kNewPos = transform.position;
			kNewPos += kRight * fMoveDeltaX;
			kNewPos += kForward * fMoveDeltaZ;
			transform.position = kNewPos;
		}
	}
}
//-----------------------------------------------------------------