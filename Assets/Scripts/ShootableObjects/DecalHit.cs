using UnityEngine;

public class DecalHit : ShootableObject
{
    public GameObject decalObject;
    float maxScale = 0.5f;
    float minScale = 0.2f;
    float lifeTime = 20.0f;

    public override void OnHit(ref HitInfo hitInfo)
    {
        GameObject instantiatedDecal = Instantiate(decalObject, hitInfo.hit.point + hitInfo.hit.normal * 0.01f,
            Quaternion.LookRotation(hitInfo.hit.normal, Quaternion.LookRotation(hitInfo.hit.normal) * Random.insideUnitCircle), transform.root.parent);

        instantiatedDecal.transform.localScale *= Random.Range(minScale, maxScale);
        Destroy(instantiatedDecal, lifeTime);
    }
}
