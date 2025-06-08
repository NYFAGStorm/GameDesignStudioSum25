using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NYFA Studio/Utility/Screenshot Manager")]
public class ScreenshotManager : MonoBehaviour {

	// Author: Glenn Storm
	// This manager handles screenshot capture and image file storage.

	[Tooltip("Base name for screenshot image files. The full name will include a date and time stap, plus an index number that resets each session.")]
	public	string			screenshotBaseName = "Game";
	public	enum StorageLocation
	{
		Desktop,
		MyDocuments,
		MyComputer,
		MyPictures,
		Personal
	}
	[Tooltip("Location to store screenshot images on Windows machines. Mac machines will store to named account folder using MyDocuments.")]
	public	StorageLocation	screenshotSaveTo;
	[Tooltip("Scale of screenshot can be adjusted with Keypad Enter + Keypad Minus or Keypad Plus. However, UI elements may not be captured if screenshot scale is more than 1. [Unity bug]")]
	public	int				maxScale = 9;

	private	string			screenshotPath;
	private	string			screenshotName;
	private	int				screenshotIndex;
	private	int				screenshotScale = 1;
	private	int				savedScale = 1;
	private	float			scaleDisplayTimer;


	void Start () {

		// configure screenshot path, name and index
		screenshotIndex = 0;
		screenshotName = screenshotBaseName+"-";
		switch ( screenshotSaveTo ) {
			case StorageLocation.Desktop:
				screenshotPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\\";
				break;
			case StorageLocation.MyDocuments:
				screenshotPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\";
				break;
			case StorageLocation.MyComputer:
				screenshotPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyComputer) + "\\";
				break;
			case StorageLocation.MyPictures:
				screenshotPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures) + "\\";
				break;
			case StorageLocation.Personal:
				screenshotPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "\\";
				break;
		}
	}
	
	void Update () {

		// allow screenshot capture with keypad "*"
		if ( Input.GetKeyDown( KeyCode.KeypadMultiply ) ) {
			StartCoroutine( AtEndOfFrame() );
		}

		// allow adjust screenshot scale with keypad "+" and "-"
		if ( Input.GetKey( KeyCode.KeypadEnter ) && Input.GetKeyDown( KeyCode.KeypadPlus ) ) {
			screenshotScale++;
			if ( screenshotScale > maxScale )
				screenshotScale = maxScale;
		}
		if ( Input.GetKey( KeyCode.KeypadEnter ) && Input.GetKeyDown( KeyCode.KeypadMinus ) ) {
			screenshotScale--;
			if ( screenshotScale < 1 )
				screenshotScale = 1;
		}

		// set display timer if scale has changed
		if ( savedScale != screenshotScale ) {
			scaleDisplayTimer = 2f;
			savedScale = screenshotScale;
		}

		// run display timer
		if ( scaleDisplayTimer > 0f ) {
			scaleDisplayTimer -= Time.unscaledDeltaTime;
			if ( scaleDisplayTimer <= 0f ) {
				scaleDisplayTimer = 0f;
			}
		}
	}

	public IEnumerator AtEndOfFrame() {

		yield return new WaitForEndOfFrame();

		// handle screenshot creation

		string currentTimeStamp = System.DateTime.Now.Month.ToString("00") + "-" + System.DateTime.Now.Day.ToString("00") + "-" + System.DateTime.Now.Hour.ToString("00") + "-" + System.DateTime.Now.Minute.ToString("00") + "-" + System.DateTime.Now.Second.ToString("00") + "-";
		ScreenCapture.CaptureScreenshot( ( screenshotPath + screenshotName + currentTimeStamp + screenshotIndex.ToString() + ".png" ), screenshotScale );
		screenshotIndex++;
	}

	void OnGUI() {

		// display changes to screenshot scale
		if ( scaleDisplayTimer > 0f ) {
			Rect r = new Rect();
			float w = Screen.width;
			float h = Screen.height;
			GUIStyle g = new GUIStyle( GUI.skin.label );
			g.fontSize = Mathf.RoundToInt( 24 * ( w / 1024f) );
			g.fontStyle = FontStyle.Bold;
			g.alignment = TextAnchor.MiddleCenter;
			g.wordWrap = true;
			Color c = Color.white;
			if ( scaleDisplayTimer <= 1f )
				c.a = ( scaleDisplayTimer / 1f );
			GUI.color = c;
			GUI.depth = -999;
			string s = "Capture Scale: "+screenshotScale;

			r.x = 0.4f * w;
			r.y = 0.05f * h;
			r.width = 0.2f * w;
			r.height = 0.1f * h;

			GUI.Label( r,s,g );
		}
	}
}
