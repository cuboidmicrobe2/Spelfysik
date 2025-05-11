using UnityEngine;

public abstract class ShootableObject : MonoBehaviour
{
    public abstract void OnHit(ref HitInfo hitInfo);
}
