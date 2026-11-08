using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;
    [SerializeField] private AudioSource soundFXObject;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        //Spawn in game object
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        //assign the clip
        audioSource.clip = audioClip;
        //assign the volume
        audioSource.volume = volume;
        // play sound
        audioSource.Play();
        // get length of clip
        float clipLength = audioSource.clip.length;
        // destroy clip after length of clip
        Destroy(audioSource.gameObject, clipLength);
    }
}
