using System.Collections;
using UnityEngine;

public class ball : MonoBehaviour
{
    ParticleSystem ps;
    SpriteRenderer sr;
    Collider2D c;

    private void Start()
    {
        c = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        ps = GetComponent<ParticleSystem>();
    }

    public void OnDestroy()
    {
        if (c != null) c.enabled = false;
        if (sr != null) sr.enabled = false;
        if (ps != null)
        {
            ps.Play();
            if (gameObject.activeSelf) { StartCoroutine(DisableAfterParticles()); }
        }
    }

    private IEnumerator DisableAfterParticles()
    {
        var mainModule = ps.main;
        mainModule.startColor = sr.color;
        yield return new WaitWhile(() => ps.IsAlive(true));
        gameObject.SetActive(false);
    }
}