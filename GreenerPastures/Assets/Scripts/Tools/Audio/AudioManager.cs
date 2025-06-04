using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NYFA Studio/Audio/AudioManager")]
public class AudioManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This manages audio sources

    public enum AType
    {
        SFXSting,
        SFXLoop,
        MusicSting,
        MusicLoop
    }

    [System.Serializable]
    public struct Audio
    {
        [Tooltip("An identifying name for this sound. Use this to start sound by name.")]
        public string name;
        [Tooltip("The audio clip.")]
        public AudioClip clip;
        [Tooltip("The type of sound and whether it should loop.")]
        public AType type;
        [Tooltip("The amount of time in seconds to delay before playing the clip.")]
        public float delay;
        [Tooltip("The maximum amount of time to delay, must be more than delay. If 0, there is no delay variance, just use delay.")]
        public float maxDelay;
        [Tooltip("The spacial blend to use for this clip. 0 = 2D, 1 = 3D")]
        public float spaceBlend;
        [Tooltip("The pitch to use when this sound is played. NOTE: if zero, will be automatically set to one upon start.")]
        public float pitch;
        [Tooltip("The amount to vary the pitch each time it is played.")]
        public float pitchVariance;
        [Tooltip("The volume to use when this sound is played. NOTE: if zero, will be automatically set to one upon start.")]
        [Range(0f, 1f)]
        public float volume;
        [Tooltip("The amount to vary the volume each time it is played. NOTE: this cannot be more than 1.")]
        public float volumeVariance;
    }
    [Tooltip("The list of sounds to use in your game.")]
    public Audio[] sounds;
    
    public struct APlay
    {
        public AudioSource source;
        public int soundIndex;
        public float playTimer; // delay to play, or loop
        public GameObject externalObj; // use this game object to play in 3D space
    }
    public APlay[] plays;

    private bool musicCrossfading;
    private float musicCrossProgress;
    private float musicFadeTimeTotal;
    private int musicCrossA = -1;
    private int musicCrossB = -1;


    void Start()
    {
        // validate
        // initialize
        plays = new APlay[0];
        // fix any blank pitch or volume properties to defaults
        for ( int i=0; i<sounds.Length; i++ )
        {
            if (sounds[i].pitch == 0f)
                sounds[i].pitch = 1f;
            if (sounds[i].volume == 0f)
                sounds[i].volume = 1f;
        }
    }

    void Update()
    {
        SoundUpdate();
        // handle crossfades
        if ( musicCrossfading )
        {
            if ( plays[musicCrossB].playTimer == 0f )
            {
                plays[musicCrossA].source.volume = 0f;
                plays[musicCrossB].source.volume = 1f * sounds[plays[musicCrossB].soundIndex].volume;
                plays[musicCrossA].source.Stop();
                RemovePlay(musicCrossA);
                musicCrossA = -1;
                musicCrossB = -1;
                musicCrossfading = false;
            }
            else
            {
                musicCrossProgress = 1f - (plays[musicCrossB].playTimer / musicFadeTimeTotal);
                plays[musicCrossA].source.volume = (1f - musicCrossProgress) * sounds[plays[musicCrossA].soundIndex].volume;
                plays[musicCrossB].source.volume = musicCrossProgress * sounds[plays[musicCrossB].soundIndex].volume;
            }
        }
    }

    void AddPlay( APlay play )
    {
        APlay[] tmp = new APlay[ (plays.Length+1) ];
        for (int i = 0; i < plays.Length; i++)
        {
            tmp[i] = plays[i];
        }
        tmp[plays.Length] = play;
        plays = tmp;
    }

    void RemovePlay( int playIndex )
    {
        if ( playIndex < 0 || playIndex >= plays.Length )
        {
            Debug.LogWarning("--- AudioManager [RemovePlay] : play index out of range "+playIndex+". Will ignore.");
            return;
        }
        EndSound(playIndex);
        // if crossfading during this operation, repair indexes
        if ( musicCrossfading )
        {
            if (musicCrossA > playIndex)
                musicCrossA--;
            if (musicCrossB > playIndex)
                musicCrossB--;
        }
        APlay[] tmp = new APlay[(plays.Length - 1)];
        int cnt = 0;
        for (int i = 0; i < plays.Length; i++)
        {
            if ( i != playIndex )
            {
                tmp[cnt] = plays[i];
                cnt++;
            }
        }
        plays = tmp;
    }

    int CreatePlay( int soundIndex )
    {
        int retInt = plays.Length;
        APlay newPlay = new APlay();
        newPlay.soundIndex = soundIndex;
        newPlay.playTimer = sounds[soundIndex].delay;
        if (sounds[soundIndex].maxDelay > sounds[soundIndex].delay)
            newPlay.playTimer += Random.Range(0f, (sounds[soundIndex].maxDelay - sounds[soundIndex].delay));
        if (newPlay.playTimer == 0f)
            newPlay.playTimer = 0.001f; // force timer run
        AddPlay(newPlay);
        return retInt;
    }

    public bool SoundExists( string name )
    {
        bool retBool = false;
        if (FindSoundByName(name) > -1)
            retBool = true;
        return retBool;
    }

    int FindSoundByName( string name )
    {
        int retInt = -1;
        for ( int i=0; i<sounds.Length; i++ )
        {
            if ( sounds[i].name == name )
            {
                retInt = i;
                break;
            }
        }
        if (retInt == -1)
            Debug.LogWarning( "--- AudioManager [FindSoundByName] : No sound '"+name+"' configured in Audio Mgr tool. Will ignore." );
        return retInt;
    }

    /// <summary>
    /// Play a sound.
    /// </summary>
    /// <param name="soundName">The name of the sound as configured.</param>
    public void StartSound( string soundName )
    {
        int sndIdx = FindSoundByName(soundName);
        if ( sndIdx == -1 )
            return;
        // create play
        int playIdx = CreatePlay(sndIdx);
        plays[playIdx].source = gameObject.AddComponent<AudioSource>();
        InitSource(plays[playIdx].source, sounds[sndIdx].clip);
        // determine if music loop, if cross fading
        if ( sounds[sndIdx].type == AType.MusicLoop )
        {
            if (musicCrossfading)
            {
                Debug.LogWarning("--- AudioManager [StartSound] : Music loop '" + sounds[sndIdx].name + "' launched during music cross fade. Will cancel.");
                RemovePlay(playIdx);
            }
            else
            {
                for ( int i=0; i<plays.Length; i++ )
                {
                    if (i != playIdx && sounds[plays[i].soundIndex].type == AType.MusicLoop)
                    {
                        musicCrossA = i;
                        break;
                    }
                }
                if ( musicCrossA > -1 && sndIdx == plays[musicCrossA].soundIndex )
                {
                    // ignore if asking to start current looping music
                    RemovePlay(playIdx);
                    musicCrossA = -1;
                }
                else if (musicCrossA > -1)
                {
                    musicCrossfading = true;
                    musicCrossProgress = 0f;
                    musicFadeTimeTotal = plays[playIdx].playTimer;
                    musicCrossB = playIdx;
                    plays[playIdx].source.volume = 0f;
                    plays[playIdx].source.Play();
                }
                else
                    musicCrossA = -1;
            }
        }
    }

    /// <summary>
    /// Play a sound from a particular object in 3D space
    /// </summary>
    /// <param name="soundName">The name of the sound as configured.</param>
    /// <param name="gameObj">The game object used as the audio source.</param>
    /// <param name="minDist">The distance to begin volume falloff.</param>
    /// <param name="maxDist">The distance the volume falloff ends.</param>
    public void StartSound( string soundName, GameObject gameObj, float minDist, float maxDist )
    {
        int sndIdx = FindSoundByName(soundName);
        if (sndIdx == -1)
            return;
        if ( sounds[sndIdx].type == AType.MusicLoop )
        {
            StartSound(soundName);
            Debug.LogWarning("--- AudioManager [StartSound] : sound "+soundName+" is a music loop and cannot be in 3D.");
            return;
        }
        if ( gameObj == null )
        {
            Debug.LogWarning("--- AudioManager [StartSound] : Game Object missing. Will ignore.");
            return;
        }
        // create play
        int playIdx = CreatePlay(sndIdx);
        plays[playIdx].externalObj = gameObj;
        plays[playIdx].source = plays[playIdx].externalObj.AddComponent<AudioSource>();
        plays[playIdx].source.spatialBlend = 1f;
        plays[playIdx].source.rolloffMode = AudioRolloffMode.Linear;
        plays[playIdx].source.minDistance = minDist;
        plays[playIdx].source.maxDistance = maxDist;
        plays[playIdx].source.volume = sounds[plays[playIdx].soundIndex].volume;
        InitSource(plays[playIdx].source, sounds[sndIdx].clip);
    }

    void InitSource( AudioSource src, AudioClip clp )
    {
        src.playOnAwake = false;
        src.loop = false; // handled with this, using delay, etc.
        src.clip = clp;
    }

    void EndSound( int playIndex )
    {
        if ( playIndex < 0 || playIndex >= plays.Length )
        {
            Debug.LogWarning("--- AudioManager [EndSound] : play index out of range "+playIndex+". Will ignore.");
            return;
        }
        if (plays[playIndex].source == null)
            return;
        plays[playIndex].source.Stop();
        // remove audio source
        Destroy(plays[playIndex].source, 5f);
    }

    void SoundUpdate()
    {
        if (plays == null || plays.Length == 0) { return; }

        // store dead sounds
        int[] deadSounds = new int[0];

        // handle plays
        for ( int i=0; i<plays.Length; i++ )
        {
            // run timer
            if (plays[i].playTimer > 0f)
                plays[i].playTimer -= Time.deltaTime;
            if (plays[i].playTimer < 0f)
            {
                plays[i].playTimer = 0f;
                // play
                if (plays[i].source != null && !plays[i].source.isPlaying)
                {
                    plays[i].source.volume = sounds[plays[i].soundIndex].volume;
                    plays[i].source.volume += (Random.Range(0f,sounds[plays[i].soundIndex].volumeVariance) - (sounds[plays[i].soundIndex].volumeVariance/2f));
                    plays[i].source.volume = Mathf.Clamp(plays[i].source.volume, 0f, 1f);
                    plays[i].source.pitch = sounds[plays[i].soundIndex].pitch;
                    plays[i].source.pitch += (Random.Range(0f, sounds[plays[i].soundIndex].pitchVariance) - (sounds[plays[i].soundIndex].pitchVariance/2f));
                    plays[i].source.pitch = Mathf.Clamp(plays[i].source.pitch, 0f, 10f); // REVIEW: 10x+ pitch disallowed
                    plays[i].source.Play();
                }
            }
            if (plays[i].playTimer == 0f && (plays[i].source == null || !plays[i].source.isPlaying))
            {
                if (sounds[plays[i].soundIndex].type == AType.SFXLoop || sounds[plays[i].soundIndex].type == AType.MusicLoop)
                {
                    // set timer for loop
                    plays[i].playTimer = sounds[plays[i].soundIndex].delay;
                    if (sounds[plays[i].soundIndex].maxDelay > sounds[plays[i].soundIndex].delay)
                        plays[i].playTimer += Random.Range(0f, (sounds[plays[i].soundIndex].maxDelay - sounds[plays[i].soundIndex].delay));
                    if (plays[i].playTimer == 0f)
                        plays[i].playTimer = 0.001f; // force timer run for loop
                }
                else
                {
                    // store dead sound
                    int[] tmp = new int[(deadSounds.Length + 1)];
                    for (int n = 0; n < deadSounds.Length; n++)
                    {
                        tmp[n] = deadSounds[n];
                    }
                    tmp[deadSounds.Length] = i;
                    deadSounds = tmp;
                }
            }
        }
        if ( deadSounds.Length > 0 )
        {
            // remove dead sounds in reverse order
            for (int i = deadSounds.Length-1; i > -1; i--)
            {
                RemovePlay(deadSounds[i]);
            }
        }
    }
}
