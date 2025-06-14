using UnityEngine;

public class TestMoveSphere : MonoBehaviour
{
    private Rigidbody rigidb;
    private void Awake()
    {
        rigidb = GetComponent<Rigidbody>();
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) 
        {
            rigidb.linearVelocity = Vector3.forward;
            rigidb.AddForce(0, 0, -5, ForceMode.Acceleration);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            rigidb.linearVelocity = Vector3.back;
            rigidb.AddForce(0, 0, 5, ForceMode.Acceleration);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            rigidb.linearVelocity = Vector3.left;
            rigidb.AddForce(5, 0, 0, ForceMode.Acceleration);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            rigidb.linearVelocity = Vector3.right;
            rigidb.AddForce(5, 0, 0, ForceMode.Acceleration);
        }
    }
}
