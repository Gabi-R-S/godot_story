using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public partial class AudioManager : Node
{
    public static AudioManager instance;
    public override void _Ready()
    {
        if (instance != null)
        {
            QueueFree();
            return;
        }
        instance = this;
    }

    Dictionary<(string, object), AudioStreamPlayer> activeSounds = new();
    Dictionary<string, AudioStream> soundEffects = new();
    Dictionary<string, AudioStream> backgroundMusic = new();
    Dictionary<string, AudioStreamPlayer> activeMusic = new();
    public void RegisterSoundEffect(string name,AudioStream sample) {
        soundEffects[name] = sample;
    }

    public void RegisterBackgroundMusic(string name, AudioStream music) { 
    backgroundMusic[name] = music;
    }
    public void PlaySound( string name,bool loop=false,object key=null) {
        if (soundEffects.ContainsKey(name)) {
            var stream= soundEffects[name];
            
            var player = new AudioStreamPlayer();
            player.Stream = stream;
            
            // Add it to the scene tree so it can play
            AddChild(player);

            if (key != null)
            {
               activeSounds[(name, key)] = player;
            } 
            // When it finishes, queue it for deletion
            player.Finished += () => {
                if (loop)
                {
                    player.Play();
                }
                else
                {
                    player.QueueFree();
                    if (key != null)
                    {
                        activeSounds.Remove((name, key));
                    }
                }
            };

            // Start playback
            player.Play();
           
        }
    }

    public bool IsPlayingSound(string name, object key) { 
        return activeSounds.ContainsKey((name,key));
    }
    public void StopSound(string name, object key)
    {
        activeSounds[(name, key)].Stop();
        activeSounds[(name, key)].QueueFree();
        activeSounds.Remove((name, key));
    }

    public void PlayMusic(string name) {
        if (backgroundMusic.ContainsKey(name))
        {
            var stream = backgroundMusic[name];

            var player = new AudioStreamPlayer();
            player.Stream = stream;

            // Add it to the scene tree so it can play
            AddChild(player);

            activeMusic[name] = player;
            
          
            // Start playback
            player.Play();

        }

    }
    public void StopMusic(string name)
    {
        activeMusic[name].Stop();
        activeMusic[name].QueueFree();
    }
}
