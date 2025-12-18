using UnityEngine;

public class Chaining : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip chainSfx;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (chainSfx != null)
        {
            if (sfxSource == null)
            {
                sfxSource = GetComponent<AudioSource>();
                if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.loop = false;
                sfxSource.spatialBlend = 0f; 
            }
            sfxSource.PlayOneShot(chainSfx, volume);
        }
        Destroy(gameObject, 0.2f);
    }
}