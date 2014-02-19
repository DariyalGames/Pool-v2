#pragma strict

var white:GameObject;
public var fadeOutTexture : Texture2D;
public var fadeSpeed:float = 0.3;

var drawDepth:float = -1000;


private var alpha:float = 1.0;
private var fadeDir:int = -1;


function Start(){
	
	alpha=1;
	fadeIn();
}



function OnGUI(){
	
	alpha += fadeDir * fadeSpeed * Time.deltaTime;
	alpha = Mathf.Clamp01(alpha);
	
	GUI.color.a = alpha;
	GUI.depth = drawDepth;
	GUI.DrawTexture(Rect(0, 0, Screen.width, Screen.height), fadeOutTexture);
}


function fadeIn(){
	fadeDir = -1;
	yield WaitForSeconds(2);
	//white.rigidbody.AddForce(Vector3(500,0,0));
	
}


function fadeOut(){
	fadeDir = 1;
	yield WaitForSeconds(1);
	Destroy(gameObject);
}

//this animation is not used right now but exists in case is needed. The menu camera moves by the photos on the wall.
function Credits(){
	animation.CrossFade("Menu_camera_creditsRun", 0.2);
}