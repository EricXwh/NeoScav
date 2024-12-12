using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public GameObject linkedDescendingBlock; 
    public string targetTag = "Ball";
    private bool isTriggered = false; // 标记是否已触发

    private void Start()
    {
        UpdateColor();
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (!isTriggered && collision.gameObject.CompareTag(targetTag))
        {
            isTriggered = true; // 标记已触发
            Debug.Log($"压力板触发: {linkedDescendingBlock.name}");
            EventManager.Instance?.TriggerPressurePlateMechanism(linkedDescendingBlock);
            UpdateColor();
        }
    }

    // private void OnCollisionExit(Collision collision)
    // {
    //     if (collision.gameObject.CompareTag(targetTag))
    //     {
    //         isTriggered = false; 
    //         UpdateColor();
    //     }
    // }

    void UpdateColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = isTriggered ? Color.green : Color.red;
        }
    }
}
