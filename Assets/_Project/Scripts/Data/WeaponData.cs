using UnityEngine;

namespace PortfolioShowcase.Data
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Showcase/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        public GameObject projectilePrefab;
        public float damage = 10f;
        public float fireRate = 4f; // shots per second
        public float projectileSpeed = 18f;
        public float projectileLifetime = 3f;
    }
}
