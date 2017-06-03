// Created by Carlos Arturo Rodriguez Silva (Legend)
// Video: https://www.youtube.com/watch?v=LXYWPNltY0s
// Contact: carlosarturors@gmail.com

// Rhythm Visualizator PRO //

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RhythmVisualizator : MonoBehaviour {

	#region Variables

	public GameObject soundBarPrefabCenter;
	public GameObject soundBarPrefabDownside;
	public Transform soundBarsTransform;
	public AudioSource audioSource;

	[Space(5)]

	[HideInInspector]
	public GameObject[] soundBars;

	[Header ("Sound Bars Options [Requires Restart]")]
	[Range (64, 256)]
	public int barsQuantity = 160;

	public enum ScaleFrom {Center, Downside};
	public ScaleFrom scaleFrom = ScaleFrom.Downside;
	int scaleFromNum = 2;

//	public float particlesVelocity = 25;

	[Header("Camera Control")] [Tooltip("Deactivate to use your own camera")]
	public bool cameraControl = true;
	[Tooltip("Rotating around camera")]
	public bool rotateCamera = true;
	[Range(-35, 35)] [Tooltip("Camera rotating velocity, positive = right, negative = left")]
	public float velocity = 15f;
	[Range(0, 300)]
	public float distance = 40f;
	[Range(1, 179)]
	public int fieldOfView = 60;

	[Header("Form Control")]
	public bool ScaleByRhythm = false;

	[Range(10, 100f)] [Tooltip("Form Length")]
	public float length = 65f;

	public enum BarsForm {Line, Circle, ExpansibleCircle, Sphere};
	[Tooltip("Bars Form")]
	public BarsForm form = BarsForm.Line;

	[Range (1f, 50f)]
	public float extraScaleVelocity = 50f;

	[Header("Auto Rhythm Particles [Experimental]")]
	public ParticleSystem rhythmParticleSystem;

	public bool autoRhythmParticles = true;

	[Tooltip ("Rhythm Sensibility, highter values is equal to more particles. Recommended: 5")]
	[Range (0f, 20f)]
	public float rhythmSensibility = 5;

	// Rhythm Minimum Sensibility. This don't need to change, use Rhythm Sensibility instead. Recommended: 1.5
	const float minRhythmSensibility = 1.5f;

	[Tooltip ("Amount of Particles to Emit")]
	[Range (1, 150)]
	public int amountToEmit = 100;

	[Tooltip("Rhythm Particles Interval Time (Recommended: 0.05 Seconds).")]
	[Range (0.01f, 0.5f)]
	public float rhythmParticlesMaxInterval = 0.05f;

	float remainingRhythmParticlesTime;
	bool rhythmSurpassed = false;

	[Header("Bass Control")] // Channel 0 (LEFT)
	[Range(1f, 300f)]
	public float bassSensibility = 40f;
	[Range(0.5f, 2f)]
	public float bassHeight = 1.5f;
	[Range(1, 5)]
	public int bassHorizontalScale = 1;
	[Range(0, 256)] [Tooltip("Bass Horizontal Off-set")]
	public int bassOffset = 0;

	[Header("Treble Control")] // Channel 1 (RIGHT)
	[Range(1f, 300f)]
	public float trebleSensibility = 80f;
	[Range(0.5f, 2f)]
	public float trebleHeight = 1.35f;
	[Range(1, 5)]
	public int trebleHorizontalScale = 3;
	[Range(0, 256)] [Tooltip("Treble Horizontal Off-set, don't decrease or you will get bass values")]
	public int trebleOffset = 67;

	[Header("Levels Control")]
	[Range(0.75f, 15f)] [Tooltip("Sound Bars global scale")]
	public float globalScale = 3f;
	[Range(1, 15)] [Tooltip("Sound Bars smooth velocity to return to 0")]
	public int smoothVelocity = 3;
	public enum Channels {n512, n1024, n2048, n4096, n8192};
	[Tooltip("Large value of channels represents more spectrum values, you will need increase the SoundBars amount to represent all these values. Recommended: 4096, 2048")]
	public Channels channels = Channels.n2048;
	[Tooltip("FFTWindow to use, it is a type of filter. Rectangular = Very Low filter, BlackmanHarris = Very High filter. Recommended = Blackman")]
	public FFTWindow method = FFTWindow.Blackman;
	int channelValue = 2048;

	[Header("Appearance Control")]
	public bool soundBarsParticles = true;

	[Tooltip("Particles Interval Time (Recommended: 0.005 Seconds). 0 = No interval")]
	[Range(0f, 0.1f)]
	public float particlesMaxInterval = 0.005f;

	float remainingParticlesTime;
	bool surpassed = false;

	[Range(0.1f, 2f)]
	public float minParticleSensibility = 1.5f;
	public bool changeColor = true;

	[Range(0.1f, 5f)]
	public float colorIntervalTime = 3f;

	[Range(0.1f, 5f)]
	public float colorLerpTime = 2f;
	public Color color1 = new Color (0.6f, 0, 1, 1);
	public Color color2 = Color.red;
	public Color color3 = Color.blue;
	public Color color4 = new Color (0, 0.75f, 1, 1);

	int posColor;

	Color newColor;
	Vector3 prevLeftScale;
	Vector3 prevRightScale;

	Vector3 rightScale;
	Vector3 leftScale;

	float timeChange;

	int halfBarsValue;

	int numberForm = 1;

	float newLeftScale;

	float newRightScale;

	float spectrumHighestValue;

	#endregion

	#region Start

	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake () {
		Application.targetFrameRate = -1;

		// Get actual form
		if (form == BarsForm.Line) {
			numberForm = 1;
		} else if (form == BarsForm.Circle) {
			numberForm = 2;
		} else if (form == BarsForm.ExpansibleCircle) {
			numberForm = 3;
		} else if (form == BarsForm.Sphere) {
			numberForm = 4;
		}

		// Check the prefabs
		if (soundBarPrefabCenter != null) {
			halfBarsValue = barsQuantity / 2;

			CreateCubes ();

		} else {
			Debug.LogWarning ("Please assign Sound Bar Prefab to the script");
			enabled = false;
		}
	}

	/// <summary>
	/// Creates the cubes.
	/// </summary>
	void CreateCubes () {

		// Instantiate the required Bars Quantity
		if (scaleFrom == ScaleFrom.Center) {
			for (int i = 0; i < barsQuantity; i++) {
				var clone = Instantiate (soundBarPrefabCenter, transform.position, Quaternion.identity) as GameObject;
				clone.transform.SetParent (soundBarsTransform.transform);
//				clone.GetComponentInChildren<ParticleSystem> ().startSpeed = particlesVelocity;
			}

			scaleFromNum = 1;
		} else if (scaleFrom == ScaleFrom.Downside){
			for (int i = 0; i < barsQuantity; i++) {
				var clone = Instantiate (soundBarPrefabDownside, transform.position, Quaternion.identity) as GameObject;
				clone.transform.SetParent (soundBarsTransform.transform);
//				clone.GetComponentInChildren<ParticleSystem> ().startSpeed = particlesVelocity;
			}

			scaleFromNum = 2;
		}
			

		// Assign all these bars to the script
		try {
			soundBars = GameObject.FindGameObjectsWithTag ("SoundBar");
		} catch {
			Debug.LogWarning ("Please add the tag SoundBar in the editor. Edit > Project Settings > Tags and Layers > Tags > and create it, then assign that tag to the SoundBarPrefab");
			enabled = false;
			return;
		}
		if (soundBars.Length <= 0) {
			Debug.LogWarning ("Please add the tag SoundBar in the editor. Edit > Project Settings > Tags and Layers > Tags > and create it, then assign that tag to the SoundBarPrefab");
			enabled = false;
			return;
		}

		UpdateVisualizations ();
	}

	/// <summary>
	/// Change to the next form. TRUE = Next, FALSE = PREVIOUS
	/// </summary>
	/// <param name="next">If set to <c>true</c> next.</param>
	public void NextForm (bool next) {
		if (next) {
			numberForm++;
		} else {
			numberForm--;
		}

		if (numberForm > 4) {
			numberForm = 1;
		} else if (numberForm <= 0) {
			numberForm = 4;
		}

		if (numberForm == 1) {
			form = BarsForm.Line;
		} else if (numberForm == 2) {
			form = BarsForm.Circle;
		} else if (numberForm == 3){
			form = BarsForm.ExpansibleCircle;
		} else if (numberForm == 4){
			form = BarsForm.Sphere;
		}

		UpdateVisualizations ();
	}

	/// <summary>
	/// Updates the channels of audio.
	/// </summary>
	void UpdateChannels () {
		if (channels == Channels.n512) {
			channelValue = 512;
		} else if (channels == Channels.n1024) {
			channelValue = 1024;
		} else if (channels == Channels.n2048) {
			channelValue = 2048;
		} else if (channels == Channels.n4096) {
			channelValue = 4096;
		}  else if (channels == Channels.n8192) {
			channelValue = 8192;
		}
	}

	#endregion

	#region Camera

	/// <summary>
	/// Change to Camera Predefined Positions
	/// </summary>
	void CameraPosition () {
		if (form == BarsForm.Line) {
			Camera.main.fieldOfView = fieldOfView;
			var cameraPos = transform.position;
			cameraPos.z -= 70f + distance;
			Camera.main.transform.position = cameraPos;
			Camera.main.transform.LookAt (soundBarsTransform.position);
			cameraPos.y += 5f;
			Camera.main.transform.position = cameraPos;

		} else if (form == BarsForm.Circle) {
			Camera.main.fieldOfView = fieldOfView;
			var cameraPos = transform.position;
			cameraPos.y += 11f;
			cameraPos.z -= 15f + distance;
			Camera.main.transform.position = cameraPos;
			Camera.main.transform.LookAt (soundBarsTransform.position);

		} else if (form == BarsForm.ExpansibleCircle) {
			Camera.main.fieldOfView = fieldOfView;
			var cameraPos = transform.position;
			cameraPos.y += 55f + distance;
			Camera.main.transform.position = cameraPos;
			Camera.main.transform.LookAt (soundBarsTransform.position);

		} else if (form == BarsForm.Sphere) {
			Camera.main.fieldOfView = fieldOfView;
			var cameraPos = transform.position;
			cameraPos.z -= 40f + distance;
			Camera.main.transform.position = cameraPos;
			Camera.main.transform.LookAt (soundBarsTransform.position);
			cameraPos.y += 5f;
			Camera.main.transform.position = cameraPos;
		}
	}

	/// <summary>
	/// Camera Rotating Around Movement.
	/// </summary>
	void CameraMovement () {
		Camera.main.transform.RotateAround (transform.position, Vector3.up, -velocity * Time.deltaTime);
	}

	#endregion

	#region ColorLerp

	Color currentColor;

	/// <summary>
	/// Change SoundBars and Particles Color.
	/// </summary>
	void ChangeColor () {
		
		if (scaleFromNum == 1) { // From Center
			currentColor = soundBars [0].GetComponent<Renderer> ().material.color;
		} else if (scaleFromNum == 2) { // From Downside
			currentColor = soundBars [0].transform.Find ("Cube").GetComponent<Renderer> ().material.color;
		}

		if (posColor == 0) {
			newColor = Color.Lerp (currentColor, color1, Time.deltaTime / colorLerpTime);
		}
		else if (posColor == 1) {
			newColor = Color.Lerp (currentColor, color2, Time.deltaTime / colorLerpTime);
		} 
		else if (posColor == 2) {
			newColor = Color.Lerp (currentColor, color3, Time.deltaTime / colorLerpTime);
		}

		else if (posColor == 3) {
			newColor = Color.Lerp (currentColor, color4, Time.deltaTime / colorLerpTime);
		}

		if (scaleFromNum == 1) { // From Center
			foreach (GameObject cube in soundBars) {
				cube.GetComponentInChildren<Renderer> ().material.color = newColor;
				#pragma warning disable 618
				cube.GetComponentInChildren<ParticleSystem> ().startColor = newColor;
				rhythmParticleSystem.startColor = newColor;
				#pragma warning restore 618
			}	
		} else if (scaleFromNum == 2) { // From Downside
			foreach (GameObject cube in soundBars) {
				cube.transform.Find ("Cube").GetComponentInChildren<Renderer> ().material.color = newColor;
				#pragma warning disable 618
				cube.GetComponentInChildren<ParticleSystem> ().startColor = newColor;
				rhythmParticleSystem.startColor = newColor;
				#pragma warning restore 618
			}
		}
	}

	/// <summary>
	/// Change SoundBars and Particles Color Helper.
	/// </summary>
	void NextColor () {

		timeChange = colorIntervalTime;
		changeColor = false;

		if (posColor < 3) {
			posColor++;
		} else {
			posColor = 0;
		}
		changeColor = true;
	}

	#endregion

	#region Visualizations

	/// <summary>
	/// Updates the visualizations.
	/// </summary>
	public void UpdateVisualizations () {
		
		// Visualizations

		if (form == BarsForm.Circle) {
			for (int i = 0; i < barsQuantity; i++) {
				float angle = i * Mathf.PI * 2f / barsQuantity;
				Vector3 pos = soundBarsTransform.transform.localPosition;
				pos -= new Vector3 (Mathf.Cos (angle), 0, Mathf.Sin (angle)) * length;
				soundBars [i].transform.localPosition = pos;
				soundBars [i].transform.LookAt (soundBarsTransform.position);

				var rot = soundBars [i].transform.eulerAngles;
				rot.x = 0;
				soundBars [i].transform.localEulerAngles = rot;
			}

		} else if (form == BarsForm.Line) {
			for (int i = 0; i < barsQuantity; i++) {
				Vector3 pos = soundBarsTransform.transform.localPosition;
				pos.x -= length * 5;
				pos.x += (length / barsQuantity) * (i * 10);
				soundBars [i].transform.localPosition = pos;
				soundBars [i].transform.localEulerAngles = Vector3.zero;
			}
		} else if (form == BarsForm.ExpansibleCircle) {
			for (int i = 0; i < barsQuantity; i++) {
				float angle = i * Mathf.PI * 2f / barsQuantity;
				Vector3 pos = soundBarsTransform.transform.localPosition;
				pos -= new Vector3 (Mathf.Cos (angle), 0, Mathf.Sin (angle)) * length;
				soundBars [i].transform.localPosition = pos;
				soundBars [i].transform.LookAt (soundBarsTransform.position);

				var newRot = soundBars [i].transform.eulerAngles;
				newRot.x -= 90;

				soundBars [i].transform.eulerAngles = newRot;
			}

		} else if (form == BarsForm.Sphere) {

			var points = UniformPointsOnSphere(barsQuantity, length);

			for(var i = 0; i < barsQuantity; i++) {

				soundBars [i].transform.localPosition = points [i];

				soundBars [i].transform.LookAt (soundBarsTransform.position);

				var rot = soundBars [i].transform.eulerAngles;
				rot.x -= 90;

				soundBars [i].transform.eulerAngles = rot;
			}
		}
			
		UpdateChannels ();

		if (cameraControl) {
			CameraPosition ();
		}
	}

	/// <summary>
	/// Create a Sphere with the given verticles number.
	/// </summary>
	/// <returns>The points on sphere.</returns>
	/// <param name="verticlesNum">Verticles number.</param>
	/// <param name="scale">Scale.</param>
	Vector3[] UniformPointsOnSphere(float verticlesNum, float scale) {
		var points = new List<Vector3>();
		var i = Mathf.PI * (3 - Mathf.Sqrt(5));
		var o = 2 / verticlesNum;
		for(var k = 0; k < verticlesNum; k++) {
			var y = k * o - 1 + (o / 2);
			var r = Mathf.Sqrt(1 - y * y);
			var phi = k * i;
			points.Add (new Vector3(Mathf.Cos(phi) * r, y, Mathf.Sin(phi) * r) * scale);
		}
		return points.ToArray();
	}
	#endregion

	#region BaseScript

	/// <summary>
	/// Updates every frame this instance.
	/// </summary>
	void Update () {

		// Get Spectrum Data from Both Channels of audio
		#pragma warning disable 618
		float[] spectrumLeftData = audioSource.GetSpectrumData (channelValue, 0, method);
		float[] spectrumRightData = audioSource.GetSpectrumData (channelValue, 1, method);
		#pragma warning restore 618

		// Wait for Rhythm Particles Interval (for performance)
		if (remainingRhythmParticlesTime <= 0) {
			
			List <float> spectrumList = new List<float> ();

			// Using bass data only
			for (int i = 0; i < halfBarsValue; i++) {
				int spectrumLeft = i * bassHorizontalScale + bassOffset;
				spectrumList.Add (spectrumLeftData[spectrumLeft]);
			}
				
//			// Using all Spectrum Data Values
//			spectrumList.AddRange (spectrumLeftData);
//			spectrumList.AddRange (spectrumRightData);

			// Get the highest spectrum value
			spectrumHighestValue = Mathf.Max (spectrumList.ToArray ()) * rhythmSensibility;

			// If the spectrum value exceeds the minimum 
			if (spectrumHighestValue >= minRhythmSensibility) {
				rhythmSurpassed = true;
			}
		
			// Auto Rhythm Particles
			if (autoRhythmParticles) {
				if (rhythmSurpassed) {
					// Emit particles
					rhythmParticleSystem.Emit (amountToEmit);
				}
			}
		}
			
		// Scale All SoundBars by Rhythm
		if (ScaleByRhythm) {

			for (int i = 0; i < barsQuantity; i++) {
				
				prevLeftScale = soundBars [i].transform.localScale;

				// If Minimum Particle Sensibility is exceeded (volume is clamped beetween 0.01 and 1 to avoid 0)
				if (rhythmSurpassed) {

					// Apply extra scale to that SoundBar using Lerp
					newLeftScale = Mathf.Lerp (prevLeftScale.y, spectrumHighestValue * bassHeight * globalScale, Time.deltaTime * extraScaleVelocity);

					// If the Particles are activated, emit a particle too
					if (soundBarsParticles) {
						if (remainingParticlesTime <= 0f) {
							soundBars [i].GetComponentInChildren<ParticleSystem> ().Play ();

							surpassed = true;
						}
					}

				} else { 	// Else, Lerp to the previous scale
					newLeftScale = Mathf.Lerp (prevLeftScale.y, spectrumHighestValue * globalScale, Time.deltaTime * extraScaleVelocity);
				}

				// If the New Scale is greater than Previous Scale, set the New Value to Previous Scale
				if (newLeftScale > prevLeftScale.y) {
					prevLeftScale.y = newLeftScale;
					rightScale = prevLeftScale;
				} else { // Else, Lerp to 0.1
					rightScale = prevLeftScale;
					rightScale.y = Mathf.Lerp (prevLeftScale.y, 0.1f, Time.deltaTime * smoothVelocity);
				}

				// Set new scale
				soundBars [i].transform.localScale = rightScale;

				// Fix minimum Y Scale
				if (soundBars [i].transform.localScale.y < 0.11f) {
					soundBars [i].transform.localScale = new Vector3 (1f, 0.11f, 1f);
				}
			}

		} else { // Scale SoundBars Normally
			
			// SoundBars for Left Channel and Right Channel
			for (int i = 0; i < halfBarsValue; i++) {

				// Apply Off-Sets to get the AudioSpectrum
				int spectrumLeft = i * bassHorizontalScale + bassOffset;
				int spectrumRight = i * trebleHorizontalScale + trebleOffset;

				// Get Actual Scale from SoundBar in "i" position
				prevLeftScale = soundBars [i].transform.localScale;
				prevRightScale = soundBars [i + halfBarsValue].transform.localScale;

				var spectrumLeftValue = spectrumLeftData [spectrumLeft] * bassSensibility;
				var spectrumRightValue = spectrumRightData [spectrumRight] * trebleSensibility;

				// Left Channel //

				// If Minimum Particle Sensibility is exceeded
				if (spectrumLeftValue >= minParticleSensibility) {

					// Apply extra scale to that SoundBar using Lerp
					newLeftScale = Mathf.Lerp (prevLeftScale.y, spectrumLeftValue * bassHeight * globalScale, Time.deltaTime * extraScaleVelocity);

					// If the Particles are activated, emit a particle too
					if (soundBarsParticles) {
						if (remainingParticlesTime <= 0) {
							soundBars [i].GetComponentInChildren<ParticleSystem> ().Play ();

							surpassed = true;
						}
					}
				} else {
					newLeftScale = Mathf.Lerp (prevLeftScale.y, spectrumLeftValue * globalScale, Time.deltaTime * smoothVelocity);
				}

				// If the New Scale is greater than Previous Scale, set the New Value to Previous Scale
				if (newLeftScale > prevLeftScale.y) {
					prevLeftScale.y = newLeftScale;
					leftScale = prevLeftScale;
				} else { // Else, Lerp to 0.1 value
					leftScale = prevLeftScale;
					leftScale.y = Mathf.Lerp (prevLeftScale.y, 0.1f, Time.deltaTime * smoothVelocity);
				} 

				// Set new scale
				soundBars [i].transform.localScale = leftScale;

				// Fix minimum Y Scale
				if (soundBars [i].transform.localScale.y < 0.11f) {
					soundBars [i].transform.localScale = new Vector3 (1f, 0.11f, 1f);
				}

				// Right Channel //
			
				// If Minimum Particle Sensibility is exceeded
				if (spectrumRightValue >= minParticleSensibility) {

					// Apply extra scale to that SoundBar using Lerp
							newRightScale = Mathf.Lerp (prevRightScale.y, spectrumRightValue * trebleHeight * globalScale, Time.deltaTime * extraScaleVelocity);

					// If the Particles are activated, emit a particle too
					if (soundBarsParticles) {
						if (remainingParticlesTime <= 0f) {

							soundBars [i + halfBarsValue].GetComponentInChildren<ParticleSystem> ().Play ();

							surpassed = true;
						}
					}
				} else {
					newRightScale = Mathf.Lerp (prevRightScale.y, spectrumRightValue * globalScale, Time.deltaTime * smoothVelocity);
				}

				// If the New Scale is greater than Previous Scale, set the New Value to Previous Scale
				if (newRightScale > prevRightScale.y) {
					prevRightScale.y = newRightScale;
					rightScale = prevRightScale;
				} else { // Else, Lerp to 0.1
					rightScale = prevRightScale;
					rightScale.y = Mathf.Lerp (prevRightScale.y, 0.1f, Time.deltaTime * smoothVelocity);
				}

				// Set new scale
				soundBars [i + halfBarsValue].transform.localScale = rightScale;

				// Fix minimum Y Scale
				if (soundBars [i + halfBarsValue].transform.localScale.y < 0.11f) {
					soundBars [i + halfBarsValue].transform.localScale = new Vector3 (1f, 0.11f, 1f);
				}
			}
		}

		// Particles Interval Reset
		if (soundBarsParticles) {
			if (surpassed) {
				surpassed = false;
				remainingParticlesTime = particlesMaxInterval;
			} else {
				remainingParticlesTime -= Time.deltaTime;
			}
		}
			
		// Rhythm Interval Reset
		if (rhythmSurpassed) {
			rhythmSurpassed = false;
			remainingRhythmParticlesTime = rhythmParticlesMaxInterval;
		} else {
			remainingRhythmParticlesTime -= Time.deltaTime;
		}
	
			
		// Change Colors
		if (changeColor) {
			timeChange -= Time.deltaTime;

			// When the counter are less than 0, change to the next Color
			if (timeChange < 0f) {
				NextColor ();
			}

			// Execute color lerping
			ChangeColor ();
		}

		// Execute Camera Control
		if (cameraControl) {
			if (rotateCamera) {
				CameraMovement ();
			}
		}
	}

	#endregion
}