// Created by Carlos Arturo Rodriguez Silva (Legend)
// Video: https://www.youtube.com/watch?v=LXYWPNltY0s
// Contact: carlosarturors@gmail.com

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Linq;

public class MusicPlayer : MonoBehaviour {

	[Header("Assignation")]
	public AudioSource audioSource;
	public Text songName;
	public Image playPauseButton;
	public Sprite playSprite;
	public Sprite pauseSprite;
	public Image playerBar;
	public Slider sliderBar;
	public Text actualTime;
	public Text totalTime;
	public InputField searchSong;

	[Header ("Songs List")]
	public GameObject songList;

	[HideInInspector]
	public List<SongData> songs;
	[HideInInspector]
	public List<GameObject> songSearchList;

	[Header ("Song Selection")]
	public GameObject songPrefab;
	public GameObject songSelectionPanel;
	public Transform songsSelectionTransform;

	int actualPos = 0;

	float amount = 0f;
	bool playing;
	bool active;

	bool showingList;
	public bool animateSearch = true;
	public Animator contentAnim;


	void Awake () {
		var songsDataList = songList.GetComponentsInChildren<SongData> ();

		foreach (SongData song in songsDataList) {
			songs.Add (song);
		}

		active = false;
		playing = false;

		CreateSongSelectionList ();

		actualPos = UnityEngine.Random.Range (0, songSearchList.Count - 1);
		ChangeSong (actualPos);
	}

	/// <summary>
	/// Creates the song selection list.
	/// </summary>
	void CreateSongSelectionList () {
		var pos = 0;

		foreach (SongData i in songs) {

			// Create a new clone
			var clone = Instantiate (songPrefab, transform) as GameObject;

			// Assigns Name to clone
			clone.name = i.artist + " - " + i.songName;

			// Calculate duration to show in 00:00 format
			var totalSeconds = i.songClip.length;
			int seconds = (int)(totalSeconds % 60f); 
			int minutes = (int)((totalSeconds / 60f) % 60f); 

			// Show Song Data
			clone.GetComponent<ActivateSong> ().ShowSongData (i.artist, i.songName, pos, minutes, seconds);

			clone.transform.SetParent (songsSelectionTransform);

			// Change scale
			var rectTransform = clone.GetComponent<RectTransform> ();
			rectTransform.localPosition = Vector3.zero;
			rectTransform.localScale = Vector3.one;

			// Add to list
			songSearchList.Insert (songSearchList.Count, clone);
			pos++;
		}
	}

	/// <summary>
	/// Searchs song by name.
	/// </summary>
	public void SearchSongByName () {
		var nameToSearch = searchSong.text;

		foreach (GameObject s in songSearchList) {
			if (System.Text.RegularExpressions.Regex.IsMatch (s.name, nameToSearch, System.Text.RegularExpressions.RegexOptions.IgnoreCase)) {
				s.SetActive (true);
			} else {
				s.SetActive (false);
			}
		}
	}

	/// <summary>
	/// Orders the list.
	/// </summary>
	public void OrderList () {
		
		// Create ref to the comparer
		IComparer myComparer = new OrderSongs ();

		// Create an array with the songs
		GameObject[] gameobjects = new GameObject[songSearchList.Count];

		// Add songs to the array
		for (int i = 0; i < songSearchList.Count; i++) {
			gameobjects [i] = songSearchList [i];
		}

		// Order the array
		Array.Sort (gameobjects, myComparer);
	
		// Order the transform
		foreach (GameObject i in gameobjects) {
			i.transform.SetAsLastSibling ();
		}

		// Clear the previous list
		songSearchList.Clear();

		// Add the new list
		for (int i = 0; i < gameobjects.Length; i++) {
			songSearchList.Insert (songSearchList.Count, gameobjects [i]);
		}
			
		// Update actual position
		for (int b = 0; b < songSearchList.Count; b++) {
			if (songSearchList [b].name == songName.text) {
				actualPos = b;
				break;
			}
		}
	}

	/// <summary>
	/// Activates or deactivate the song selection list.
	/// </summary>
	public void ActivateOrDeactivate () {
		showingList = !showingList;

		if (showingList) {
			StopCoroutine("HideAnimate");
			contentAnim.SetBool("Show", true);
			songSelectionPanel.SetActive (true);

		} else {
			if (animateSearch)
			{
				StartCoroutine(HideAnimation());
			}
			else
			{
				songSelectionPanel.SetActive (false);
			}
		}
	}

	/// <summary>
	/// Hide animation.
	/// </summary>
	IEnumerator HideAnimation()
	{
		if (contentAnim != null) {
			contentAnim.SetBool ("Show", false);
			yield return new WaitForSeconds (contentAnim.GetCurrentAnimatorStateInfo (0).length);
			songSelectionPanel.SetActive (false);
		} else {
			songSelectionPanel.SetActive (false);
		}
	}

	/// <summary>
	/// Sets to active the actual song image.
	/// </summary>
	void SetActiveSongImage () {
		foreach (GameObject i in songSearchList) {
			i.GetComponent<ActivateSong>().activeSongImage.enabled = false;
		}
		songSearchList [actualPos].GetComponent<ActivateSong>().activeSongImage.enabled = true;
	}

	/// <summary>
	/// Change the song using his id
	/// </summary>
	/// <param name="pos">Position.</param>
	public void ChangeSong (int pos) {
		actualPos = pos;

		songName.text = songs[pos].artist + " - " + songs[pos].songName;

		PrepareToLoadSong (pos);

		SetActiveSongImage ();
	}

	void Update () {
		if (active) {
			if (playing) {
				if (audioSource.isPlaying) {
					amount = (audioSource.time / audioSource.clip.length);
					playerBar.fillAmount = amount;

						// Calculate duration to show in 00:00 format
						var totalSeconds = audioSource.time;
						int seconds = (int)(totalSeconds % 60f); 
						int minutes = (int)((totalSeconds / 60f) % 60f); 

						actualTime.text = minutes + ":" + seconds.ToString("D2");
		
				} else {
					active = false;
					playing = false;
					NextSong ();
				}
			}
		}
	}

	/// <summary>
	/// Changes the playback position in the song.
	/// </summary>
	public void ChangePosition () {
		if (audioSource.clip != null) {
			active = false;
			audioSource.time = sliderBar.value * audioSource.clip.length;
			playerBar.fillAmount = sliderBar.value;
			active = true;
		}
	}

	/// <summary>
	/// Stops the song.
	/// </summary>
	public void StopSong () {
	//	Debug.Log ("Stop");
		StopAllCoroutines ();
		active = false;
		playing = false;

		audioSource.Stop();
		playPauseButton.sprite = playSprite;
		amount = 0f;
		sliderBar.value = 0f;
		playerBar.fillAmount = 0f;
	}

	/// <summary>
	/// Play or pause the song.
	/// </summary>
	public void PlayOrPauseSong() {
		if (playing) {
		//	Debug.Log ("Pause");
			active = false;
			playing = false;
			audioSource.Pause();
			playPauseButton.sprite = playSprite;

		} else {
			audioSource.Play();

		//	Debug.Log ("Play");
			playPauseButton.sprite = pauseSprite;
			playing = true;
			active = true;
		}
	}

	/// <summary>
	/// Plays the next song.
	/// </summary>
	public void NextSong () {
		StopSong ();

		++actualPos;
		if (actualPos > songSearchList.Count - 1) {
			actualPos = 0;
		}

		var nombre = songSearchList [actualPos].transform.name;
		songName.text = nombre;

		PrepareToLoadSong (actualPos);

		SetActiveSongImage ();
	}

	/// <summary>
	/// Plays the previous song.
	/// </summary>
	public void PreviousSong () {
		StopSong ();

		--actualPos;

		if (actualPos < 0) {
			actualPos = songSearchList.Count - 1;
		}

		var nombre = songSearchList [actualPos].transform.name;
		songName.text = nombre;
			
		PrepareToLoadSong (actualPos);

		SetActiveSongImage ();
	}

	/// <summary>
	/// Prepares to load the song.
	/// </summary>
	/// <param name="pos">Position.</param>
	void PrepareToLoadSong (int pos) {
		StopCoroutine ("LoadSong");
		StartCoroutine (LoadSong (songs[pos]));
	}

	/// <summary>
	/// Loads the song.
	/// </summary>
	/// <returns>The song.</returns>
	/// <param name="song">Song.</param>
	IEnumerator LoadSong (SongData song) {

		// Rename the clip
		AudioClip a = song.songClip;
		a.name = song.artist + " - " + song.songName;

		// Loads and wait for song load
		#pragma warning disable 618
		while (!a.isReadyToPlay)
			#pragma warning restore 618
		{
			//	Debug.Log("Loading Song...");
			yield return null; 
		}


		StopSong ();

		// Assign the clip, and play
		audioSource.clip = a;

		// Calculate duration to show in 00:00 format
		var totalSeconds = audioSource.clip.length;
		int seconds = (int)(totalSeconds % 60f); 
		int minutes = (int)((totalSeconds / 60f) % 60f); 

		totalTime.text = minutes + ":" + seconds.ToString("D2");

		PlayOrPauseSong ();
	}
}

public class OrderSongs : IComparer  {

	int IComparer.Compare( System.Object x, System.Object y )  {
		return( (new CaseInsensitiveComparer()).Compare( ((GameObject)x).name, ((GameObject)y).name) );
	}
}
