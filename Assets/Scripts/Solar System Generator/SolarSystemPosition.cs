using UnityEngine;
public class SolarSystemPosition : MonoBehaviour
{
    private float systemRadius = 0f; 
    private bool showCollisionGizmo = true;
    public float CalculateMinDistance(float orbitDistance, float planetRadius)
    {
        float occupiedDistance = orbitDistance + planetRadius;
        systemRadius = Mathf.Max(systemRadius, occupiedDistance);
        return occupiedDistance;
    }
    
    public float GetSystemRadius()
    {
        return systemRadius;
    }
    
    public void ResetSystemRadius()
    {
        systemRadius = 0f;
    }
    
    private void OnDrawGizmos()
    {
        if (showCollisionGizmo && systemRadius > 0)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            DrawWireSphere(transform.position, systemRadius, 32);
        }
    }
    
    private void DrawWireSphere(Vector3 center, float radius, int segments)
    {
        DrawCircle(center, radius, Vector3.right, segments);
        DrawCircle(center, radius, Vector3.up, segments);
        DrawCircle(center, radius, Vector3.forward, segments);
    }
    
    private void DrawCircle(Vector3 center, float radius, Vector3 normal, int segments)
    {
        normal = normal.normalized;
        
        Vector3 right = Vector3.Cross(normal, Vector3.forward);
        if (right.sqrMagnitude < 0.0001f)
        {
            right = Vector3.Cross(normal, Vector3.right);
        }
        right = right.normalized;
        
        Vector3 forward = Vector3.Cross(normal, right).normalized;
        
        Vector3 lastPoint = center + right * radius;
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 point = center + (Mathf.Cos(angle) * right + Mathf.Sin(angle) * forward) * radius;
            Gizmos.DrawLine(lastPoint, point);
            lastPoint = point;
        }
    }
}
