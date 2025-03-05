using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Analytics;

public class AnalyticsInitializer : MonoBehaviour
{
    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            AnalyticsService.Instance.StartDataCollection();
            Debug.Log("Unity Services and Analytics initialized successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to initialize Unity Services: " + e.Message);
        }
    }
}
