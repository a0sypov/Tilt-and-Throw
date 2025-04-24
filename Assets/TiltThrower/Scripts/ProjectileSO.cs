using UnityEngine;

[CreateAssetMenu(fileName = "Projectile", menuName = "Weapons/Default Projectile")]
public class ProjectileSO : ScriptableObject
{
    public string projectileName;

    public Sprite sprite;

    public int damage;
    public float speed;
    public float fireRate;

    public float scale;

    public bool shouldRichochet;
    public bool shouldStickToWalls;

    public bool shouldSpin;
    public float spinSpeed;
}
