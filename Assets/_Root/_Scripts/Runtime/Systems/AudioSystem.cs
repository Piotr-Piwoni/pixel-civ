using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using PixelCiv.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;
using Debug = UnityEngine.Debug;

namespace PixelCiv.Systems
{
[RequireComponent(typeof(AudioSource))]
public class AudioSystem : Singleton<AudioSystem>
{
	[SerializeField, TabGroup("", "Debug")]
	private bool _Log;
	[SerializeField,
	 TabGroup("", "Settings", SdfIconType.GearFill, TextColor = "yellow"),
	 MinValue(1)]
	private int _MaxAudioSources = 50;

	private readonly Dictionary<AudioSource, Coroutine> _AudioSourceCoroutines =
		new();
	private AudioMixer _Mixer;
	private AudioSource _MusicSrc;
	private ObjectPool<AudioSource> _AudioSourcePool;

	protected override void Awake()
	{
		base.Awake();

		// Load the AudioMixer.
		_Mixer = Resources.Load<AudioMixer>("GameAudioMixer");

		// The AudioSource on this object will act as the music source.
		_MusicSrc = GetComponent<AudioSource>();
		_MusicSrc.loop = true;
		_MusicSrc.playOnAwake = false;
		FindMixerGroup(_MusicSrc, "Music");

		// Initialize the pool.
		_AudioSourcePool = new ObjectPool<AudioSource>(CreateAudioSource,
			OnGetAudioSource, OnReleaseAudioSource, OnDestroyAudioSource,
			defaultCapacity: 10, maxSize: _MaxAudioSources);
	}

	private void Update()
	{
		// Only if logging is enabled.
		LogSources();
	}

	/// <summary>
	///     Plays the provided audio clip.
	/// </summary>
	/// <param name="clip">The audio to play.</param>
	/// <param name="position">The position of the clip in 3D.</param>
	/// <param name="type">What type of audio is it.</param>
	/// <param name="volume">The clip's volume.</param>
	/// <param name="loop">If the clip should be looping.</param>
	/// <returns>The audio source used.</returns>
	public AudioSource PlayClip(AudioClip clip, Vector3? position = null,
		AudioType type = AudioType.Sfx,
		float volume = 1f, bool loop = false)
	{
		// Retrieve an AudioSource from the pool.
		AudioSource audioSrc = _AudioSourcePool.Get();

		// Set up the AudioSource.
		// If position was provided, turn the current audio source into 3D sound.
		if (position.HasValue && position.Value != Vector3.zero)
		{
			audioSrc.transform.position = position.Value;
			audioSrc.spatialBlend = 1f; //< 3D sound.
		}
		// Reset.
		else
		{
			audioSrc.transform.position = Vector3.zero;
			audioSrc.spatialBlend = 0f;
		}

		// Based on the AudioType, find the corresponding mixer group.
		switch (type)
		{
			case AudioType.Music:
				FindMixerGroup(audioSrc, "Music");
				break;
			case AudioType.Sfx:
				FindMixerGroup(audioSrc, "SFX");
				break;
			case AudioType.Dialogue:
				FindMixerGroup(audioSrc, "Dialogue");
				break;
		}

		audioSrc.clip = clip;
		audioSrc.volume = volume;
		audioSrc.loop = loop;
		audioSrc.Play();

		// If the AudioSource is not already in the dictionary, add it.
		if (!_AudioSourceCoroutines.ContainsKey(audioSrc))
		{
			// Start the coroutine and add it to the dictionary for quick lookup.
			Coroutine coroutine = WaitToFinishPlaying(audioSrc);
			_AudioSourceCoroutines.Add(audioSrc, coroutine);
		}
		else
		{
			// Start the coroutine and add it to the dictionary for quick lookup.
			Coroutine coroutine = WaitToFinishPlaying(audioSrc);
			_AudioSourceCoroutines[audioSrc] = coroutine;
		}

		return audioSrc;
	}

	/// <summary>
	///     Plays the provided audio clip on loop.
	/// </summary>
	/// <param name="clip">The audio to play.</param>
	/// <param name="volume">The clip's volume.</param>
	public void PlayMusic(AudioClip clip, float volume = 1f)
	{
		// Stop any existing music before playing a new one.
		if (_MusicSrc.isPlaying)
			_MusicSrc.Stop();

		_MusicSrc.volume = volume;
		_MusicSrc.clip = clip;
		_MusicSrc.Play();
	}

	/// <summary>
	///     Stop the current audio clip from playing.
	/// </summary>
	/// <param name="audioSrc">The audio source that you wish to stop playing.</param>
	public void StopClip(AudioSource audioSrc)
	{
		if (!audioSrc)
			return;

		// Stop the audio source and update the dictionary.
		audioSrc.Stop();

		// If it's in the dictionary, and it has a coroutine, stop it.
		if (!_AudioSourceCoroutines.TryGetValue(audioSrc,
			    out Coroutine coroutine))
			return;

		// Clean up.
		if (coroutine != null)
			StopCoroutine(coroutine);
		_AudioSourceCoroutines[audioSrc] = null;

		// Check if the audio source is already released into the pool.
		if (!audioSrc.gameObject.activeSelf)
			return;

		// Release the audio source back to the pool.
		_AudioSourcePool.Release(audioSrc);
	}

	/// Stops the current music from playing.
	public void StopMusic()
	{
		_MusicSrc.Stop();
	}

	/// Create a new AudioSource.
	private AudioSource CreateAudioSource()
	{
		var tempObj = new GameObject("TempSrc", typeof(AudioSource));
		tempObj.transform.SetParent(transform);
		var source = tempObj.GetComponent<AudioSource>();
		source.playOnAwake = false;
		return source;
	}

	/// <summary>
	///     Find and assign the desired group from the Audio Mixer.
	/// </summary>
	/// <param name="audioSrs">The source that the mixer group will be assigned to.</param>
	/// <param name="groupName">The name of the mixer group to find.</param>
	private void FindMixerGroup(AudioSource audioSrs, string groupName)
	{
		var mixerGroups = _Mixer.FindMatchingGroups(groupName);
		if (mixerGroups.Length > 0)
			audioSrs.outputAudioMixerGroup = mixerGroups[0];
		else
		{
			Debug.LogError($"The group: \"{groupName}\", was not found in " +
			               "Audio Mixer! Defaulting to \"Master\".");

			// Fallback to Master.
			audioSrs.outputAudioMixerGroup =
				_Mixer.FindMatchingGroups("Master")[0];
		}
	}

	/// Output to console the AudioSources and their respected Coroutines.
	[Conditional("UNITY_EDITOR")]
	private void LogSources()
	{
		if (!_Log || _AudioSourceCoroutines.Count <= 0)
			return;

		Debug.Log(
			$"<color=Red>------------- {name}<AudioSystem> Log Start -------------</color>");
		foreach ((AudioSource key, Coroutine value) in _AudioSourceCoroutines)
		{
			if (!key)
				continue;

			// If there's an active coroutine, get its hash code, otherwise output NULL.
			var logValue = value != null
				? value.GetHashCode().ToString()
				: "Null";

			// If the source is looping, specify that it is.
			if (key.loop)
				logValue += ", is Looping";

			Debug.Log($"{key.name}: " +
			          $"<color=yellow>{key.GetInstanceID()}</color>\t" +
			          $"Coroutine: <color=green>{logValue}</color>");
		}

		Debug.Log(
			$"<color=Red>------------- {name}<AudioSystem> End -------------</color>");
	}

	/// Called when an AudioSource is destroyed.
	private void OnDestroyAudioSource(AudioSource audioSource)
	{
		// Stop its corresponding coroutine and clean up.
		if (_AudioSourceCoroutines.ContainsKey(audioSource))
		{
			StopCoroutine(_AudioSourceCoroutines[audioSource]);
			_AudioSourceCoroutines.Remove(audioSource);
		}

		Destroy(audioSource.gameObject);
	}

	/// Called when an AudioSource is retrieved from the pool
	private static void OnGetAudioSource(AudioSource audioSource)
	{
		audioSource.gameObject.SetActive(true);
	}

	/// Called when an AudioSource is released back to the pool.
	private static void OnReleaseAudioSource(AudioSource audioSource)
	{
		audioSource.gameObject.SetActive(false);
	}

	/// Release the AudioSource back to the pool after it finishes playing.
	private IEnumerator ReturnAudioSourceToPool(AudioSource audioSource,
		float clipLength)
	{
		yield return new WaitForSeconds(clipLength);
		_AudioSourcePool.Release(audioSource);

		// Set the coroutine to NULL in the dictionary after the audio finishes.
		if (_AudioSourceCoroutines.ContainsKey(audioSource))
			_AudioSourceCoroutines[audioSource] = null;
	}

	private Coroutine WaitToFinishPlaying(AudioSource src)
	{
		// Ensure that the coroutine only happens for non-looping sources.
		if (src.loop) return null;

		// Ensure no coroutine is running for the same source.
		Coroutine coroutine = null;
		if (!_AudioSourceCoroutines.ContainsKey(src) ||
		    _AudioSourceCoroutines[src] == null)
		{
			coroutine = StartCoroutine(ReturnAudioSourceToPool(src,
				src.clip.length));
		}

		return coroutine;
	}

#if UNITY_EDITOR

	[SerializeField,
	 TabGroup("", "Debug", SdfIconType.BugFill, TextColor = "red"),
	 EnableIf(nameof(_Log))]
	private AudioClip _TestingClip;
	[SerializeField, TabGroup("", "Debug"), EnableIf(nameof(_Log))]
	private bool _LoopClip;
	[SerializeField, TabGroup("", "Debug"), EnableIf(nameof(_Log)), ReadOnly]
	private AudioSource _TestingSource;


	[ResponsiveButtonGroup]
	private void StopTestingClip()
	{
		if (!_Log || !_TestingSource)
			return;
		StopClip(_TestingSource);
	}

	[ResponsiveButtonGroup]
	private void PlayTestingClip()
	{
		if (!_Log) return;
		if (!_TestingClip)
		{
			Debug.Log("\"_TestingClip\" was not assigned!");
			return;
		}

		_TestingSource = PlayClip(_TestingClip, loop: _LoopClip);
	}
#endif
}

public enum AudioType
{
	Music = 0,
	Sfx = 1,
	Dialogue = 2
}
}