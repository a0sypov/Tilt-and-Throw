using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(LineRenderer))]
public class ShootingController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public GameObject projectilePrefab;
    public GameObject player;
    public GameObject aimObject;
    public ProjectileSO currentProjectile;

    private float nextFireTime = 0f;
    private Vector2 _dir;

    private LineRenderer lineRenderer;
    private bool isTouched = false;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (!isTouched)
        {
            lineRenderer.enabled = false;
            return;
        }

        _dir = GetDirection2D(aimObject.transform.position, player.transform.position);

        Vector3 start = player.transform.position;
        Vector3 end = start + (Vector3)(_dir * 50f);

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.enabled = true;

        if (Time.time >= nextFireTime)
        {
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.green;
        }
        else
        {
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
        }
    }

    public static Vector2 GetDirection2D(Vector2 start, Vector2 end)
    {
        return (end - start).normalized;
    }

    public void Shoot()
    {
        if (Time.time < nextFireTime)
            return;

        nextFireTime = Time.time + currentProjectile.fireRate;

        GameObject projectile = Instantiate(projectilePrefab, player.transform.position, Quaternion.identity);
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        
        projectile.transform.parent = GameObject.Find("ProjectilesContainer").transform;

        projectileScript.SetProjectileSO(currentProjectile);
        projectileScript.SetDirection(_dir);
        projectileScript.SetTagToIgnore(player.tag);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isTouched = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTouched = false;
    }
}
