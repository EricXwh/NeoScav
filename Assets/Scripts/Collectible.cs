using System.Collections;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public bool isCollectibe = false; 
    public float animationDuration = 0.5f;      
    public float targetScaleMultiplier = 1.5f;  
    public AudioClip collectSound;
    public float soundVolumeScale = 1f;

    private MeshRenderer meshRenderer;
    private Vector3 initialScale;
    private Color initialColor;
    private AudioSource audioSource;

    void Start()
    {
        if (GameManager.Instance.selectedCollectible == CollectibleState.None)
        {
            gameObject.SetActive(false);
            return;
        }

        if (isCollectibe)
        {
            meshRenderer = GetComponent<MeshRenderer>();
            initialScale = transform.localScale;
            initialColor = meshRenderer.material.color;
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollectibe && other.CompareTag("Player"))
        {
            if (collectSound != null)
            {
                audioSource.PlayOneShot(collectSound, soundVolumeScale);
            }
            if (GameManager.Instance.selectedCollectible == CollectibleState.HasCollectible)
            {
                GameManager.Instance.collectedCount++;
            }
            StartCoroutine(AnimateAndDestroy());
        }
    }

    private IEnumerator AnimateAndDestroy()
    {
        float elapsed = 0f;
        Vector3 targetScale = initialScale * targetScaleMultiplier;
        Material mat = meshRenderer.material;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            Color newColor = initialColor;
            newColor.a = Mathf.Lerp(initialColor.a, 0, t);
            mat.color = newColor;
            yield return null;
        }
        Destroy(gameObject);
    }
}
