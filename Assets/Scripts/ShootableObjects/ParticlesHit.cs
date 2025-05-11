using UnityEngine;

public class ParticlesHit : ShootableObject
{
    public GameObject particlesPrefab;
    float lifeTime = 2.0f;

    public override void OnHit(ref HitInfo hitInfo)
    {
        GameObject instantiatedParticles = Instantiate(particlesPrefab, hitInfo.hit.point + hitInfo.hit.normal * 0.05f,
            Quaternion.LookRotation(hitInfo.hit.normal), transform.root.parent);

        instantiatedParticles.GetComponent<ParticleSystem>().startColor = GetComponent<Renderer>().material.color;
        Destroy(instantiatedParticles, lifeTime);
    }
}
