using UnityEngine;

public class AutoMoveForward : MonoBehaviour
{
	public float moveSpeed;

    // Update is called once per frame
    void Update()
    {
        Vector3 position = transform.position;
		position += (Vector3.forward + Vector3.right)*Time.deltaTime * moveSpeed;
		transform.position = position;
    }
}
