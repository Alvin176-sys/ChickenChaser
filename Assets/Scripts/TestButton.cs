using UnityEngine;
using UnityEngine.UI;

public class TestButton : MonoBehaviour
{
    private Button button;
    [SerializeField] Rigidbody rigidbody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        rigidbody = FindFirstObjectByType<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void MoveForward() 
    {
        rigidbody.AddForce (transform.forward);
        rigidbody.AddForce(0, 0, -500, ForceMode.Impulse);
    }
}
 