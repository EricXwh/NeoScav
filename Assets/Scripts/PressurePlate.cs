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


    private void OnTriggerEnter(Collider other)
    {
        if (!isTriggered && (other.CompareTag(targetTag)||other.CompareTag("Player")))
        {
            isTriggered = true;
            Debug.Log($"压力板触发: {linkedDescendingBlock.name}");
            EventManager.Instance?.TriggerPressurePlateMechanism(linkedDescendingBlock);
            UpdateColor();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag)||other.CompareTag("Player"))
        {
            isTriggered = false;
            EventManager.Instance?.TriggerPressurePlateReset(linkedDescendingBlock);
            UpdateColor();
        }
    }

    void UpdateColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = isTriggered ? Color.green : Color.red;
        }
    }
}
