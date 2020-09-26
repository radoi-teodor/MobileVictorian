using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceEssentials : MonoBehaviour {

    /// <summary>
    /// Original properties of an audio source
    /// </summary>

    public bool loop = false;
    [Range(0, 256)]
    public int priority = 128;
    [Range(0, 1)]
    public float volume = 1;
    [Range(-3, 3)]
    public float pitch = 1;
    [Range(-1, 1)]
    public float stereoPan = 1;
    [Range(0, 1)]
    public float spatialBlend = 1;
    [Range(0, 1.1f)]
    public float reverbZoneMix = 1;

    [System.Serializable]
    public class SoundSettings3D{ // a class to make it easier to read in inspector

        [Range(0, 5)]
        public float dopplerLevel = 1;
        [Range(0, 360)]
        public int spread = 0;
        public AudioRolloffMode volumeRolloff;
        public float minDistance = 1, maxDistance = 500;

    }

    public SoundSettings3D soundSettings3D = new SoundSettings3D(); // object of the class

    public void PlayAudio(AudioClip audClip)
    {
        if (audClip) // if audioclip is not null
        {
            AudioSource audS = gameObject.AddComponent<AudioSource>(); // we create an audiosource
            audS.clip = audClip; // assign a clip
            setAudioSource(audS); // set audio source properties

            audS.Play(); // play the clip

            if (!audS.loop) // if we don't loop
            {
                Destroy(audS, audClip.length + .5f); // we destroy the component
            }
        }
    }

    public void setAudioSource(AudioSource audS) // we set the properties of the audiosource
    {
        audS.playOnAwake = false;
        audS.loop = loop;
        audS.priority = priority;
        audS.volume = volume;
        audS.pitch = pitch;
        audS.panStereo = stereoPan;
        audS.spatialBlend = spatialBlend;
        audS.reverbZoneMix = reverbZoneMix;

        audS.dopplerLevel = soundSettings3D.dopplerLevel;
        audS.spread = soundSettings3D.spread;
        audS.rolloffMode = soundSettings3D.volumeRolloff;
        audS.minDistance = soundSettings3D.minDistance;
        audS.maxDistance = soundSettings3D.maxDistance;
    }
}
