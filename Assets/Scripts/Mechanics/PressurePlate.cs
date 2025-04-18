using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class PressurePlate : MonoBehaviour, ILinkable
{
    [Header("触发条件设置")]
    public string[] triggeringTags;

    [Header("关联的机关对象")]
    [Tooltip("拖入需要响应触发的机关组件")]
    public MechanismBase[] linkedMechanisms;

    [Header("音效设置")]
    public AudioClip plateSound;
    public float soundVolume = 1f;

    private int activatorCount = 0;
    private AudioSource audioSource;

    MechanismBase[] ILinkable.LinkedMechanisms
    {
        get => linkedMechanisms;
        set => linkedMechanisms = value;
    }
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CheckTriggerCondition(other))
        {
            activatorCount++;
            if (activatorCount == 1)
            {
                foreach (var mechanism in linkedMechanisms)
                {
                    mechanism.TriggerActivate();
                }
                UpdatePlateColor(true);
                if (plateSound != null)
                {
                    audioSource.PlayOneShot(plateSound, soundVolume);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (CheckTriggerCondition(other))
        {
            activatorCount--;
            if (activatorCount < 0)
                activatorCount = 0;

            if (activatorCount == 0)
            {
                foreach (var mechanism in linkedMechanisms)
                {
                    mechanism.TriggerDeactivate();
                }
                UpdatePlateColor(false);
                if (plateSound != null)
                {
                    audioSource.PlayOneShot(plateSound, soundVolume);
                }
            }
        }
    }

    private bool CheckTriggerCondition(Collider other)
    {
        if (triggeringTags != null)
        {
            foreach (var tag in triggeringTags)
            {
                if (other.CompareTag(tag))
                    return true;
            }
        }
        return false;
    }

    private void UpdatePlateColor(bool activated)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = activated ? Color.green : Color.yellow;
        }
    }
}
