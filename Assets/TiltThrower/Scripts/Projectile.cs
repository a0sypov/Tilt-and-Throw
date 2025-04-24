using UnityEngine;

public class Projectile : MonoBehaviour
{
    ProjectileSO projectileSO;
    Vector2 direction;
    string tagToIgnore = "Player";

    public void SetDirection(Vector2 dir)
    {
        direction = dir * projectileSO.speed;
        RotateTowardsMovement();

    }

    public void RotateTowardsMovement()
    {
        if (!projectileSO.shouldSpin)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
        }
    }

    public void SetProjectileSO(ProjectileSO projectile)
    {
        projectileSO = projectile;
        GetComponent<SpriteRenderer>().sprite = projectile.sprite;
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(projectile.sprite.bounds.size.x, projectile.sprite.bounds.size.y);
        boxCollider.isTrigger = true;
        boxCollider.enabled = true;

        transform.localScale = new Vector3(projectile.scale, projectile.scale, 1);
    }

    public void SetTagToIgnore(string tag)
    {
        tagToIgnore = tag;
    }

    void FixedUpdate()
    {
        // Move along the direction
        transform.position = Vector2.MoveTowards(transform.position, (Vector2)transform.position + direction, projectileSO.speed * Time.deltaTime);

        // Spin the projectile if it should
        if (projectileSO.shouldSpin)
        {
            transform.Rotate(0, 0, projectileSO.spinSpeed * 400 * Time.deltaTime);
        }

        // Check if the projectile is out of bounds based on camera;
        // if out of bounds at x coordinates then reverse direction;
        // if out of bounds at y coordinates then destroy
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);
        if (screenPos.x < 0 || screenPos.x > 1)
        {
            if (projectileSO.shouldRichochet)
            {
                // Reverse direction
                direction.x = -direction.x;
                RotateTowardsMovement();
            }
            else if (projectileSO.shouldStickToWalls)
            {
                direction = Vector2.zero;

                // Destroy the projectile after 5 seconds
                Destroy(gameObject, 5f);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        if (screenPos.y < 0 || screenPos.y > 1)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null && !collision.gameObject.CompareTag(tagToIgnore))
        {
            if(collision.gameObject.CompareTag("Player"))
            {
                Debug.Log("Player hit by projectile");
            }
            damageable.TakeDamage(projectileSO.damage);
            Destroy(gameObject);
        }
    }
}