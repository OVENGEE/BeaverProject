using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MiniMapAutoFit : MonoBehaviour
{
    [Header("Manual Terrain Bounds")]
    [SerializeField] private Vector2 mapCenter = Vector2.zero;
    [SerializeField] private float mapHeight = 30f;
    [SerializeField] private float verticalPadding = 1f;

    private Camera miniMapCam;

    private void Start()
    {
        miniMapCam = GetComponent<Camera>();
        FitToTerrain();
    }

    [ContextMenu("Fit To Terrain")]
    public void FitToTerrain()
    {
        if (miniMapCam == null) miniMapCam = GetComponent<Camera>();

        // 1. Center camera over map center
        transform.position = new Vector3(mapCenter.x, mapCenter.y, transform.position.z);

        // 2. Set orthographic size based on vertical height
        miniMapCam.orthographicSize = (mapHeight / 2f) + verticalPadding;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizes map boundary box in Scene view for easy calibration
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(mapCenter, new Vector3(mapHeight * miniMapCam.aspect, mapHeight, 0f));
    }
}