using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public Sound[] musicSounds, sfxSounds;
    public AudioSource musicSource, sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //PlayMusic("WorldMusic");        //Asi llamarias las canciones por su nombre en caso de usar varias
        PlayFirstMusic();          
    }

    public void PlayMusic(string name)  //Usar este si se debe cambiar la musica en algun momento
    {                                                        //en la misma escena
        Sound s = Array.Find(musicSounds, x => x.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
        }
        else
        {
            musicSource.clip = s.clip;
            musicSource.Play();
        }
    }

    public void PlayFirstMusic()    //Reproduce la primera musica que asignes en el inspector
    {
        if (musicSounds.Length > 0)
        {
            musicSource.clip = musicSounds[0].clip;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("No music sounds available in the array!");
        }
    }


    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
        }
        else
        {
            sfxSource.PlayOneShot(s.clip);
        }
    }

    //NOTA: para usar los sfx, ir al evento que lo activa y poner
    //      AudioManager.Instance.PlaySFX("NombreDelSFX");
    // (Asignar primero todos los sfx necesarios en el inspector.
}
