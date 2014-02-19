//This is the main script of the game and is attached to the White gameobject, which is the white ball.

#pragma strict

var cam:Transform;
var camtop:GameObject;
var cue:GameObject;//the actual cue
var cuehelp:Transform;//a helper object placed exactly at the position of the cue's front
var trigono:GameObject;
var whitereset:Transform;//a transparent box used for repositioning the white ball when needed.
var guibar:GUITexture;
var impact:AudioClip;
var cuesound:AudioClip;
var pocket:AudioClip;

private var cuex:float;
private var cuey:float;
private var camx:float;
private var camz:float;
private var torquex:float;
private var torquez:float;
private var cuestr:int;//this is the strength that the cue will hit the ball
private var maxx:float;
private var maxy:float;//
private var lineRender:LineRenderer;//this is used for the raytracing line
private var enable:boolean;//this is used to know whether the player is enabled to play
private var shotTaken : boolean;

public enum BallState { Start, Idle, Shooting, Rolling, Resetting }
public var state : BallState;

//this is used for determining the strength of the shot
public var imaginaryCursor: Transform;

private var lastMousePosition:Vector3;
private var isMouseDrag:boolean;
private var mouseDistance = 0.0f;
private var topCamera: Camera;

function Start()
{
	cuestr = 0;
	maxx = 0;
	maxy = 0;
	lineRender = GetComponent(LineRenderer);
	lineRender.enabled = false;
	enable = false;
	isMouseDrag = false;
	
	whitereset.renderer.enabled = false;
	topCamera = camtop.GetComponent("Camera");
	
	StartGame();
}




function OnTriggerEnter(other:Collider){
	
	if (other.gameObject.name == "abyss"){
		
		WhiteReset();
		
	}
	
	if (other.gameObject.name == "room"){
		
		WhiteReset();
		
	}
	
	//rigidbody.AddForce(Vector3(0,-100,0));
	
}

function OnGUI(){
	
	if (GUI.Button(Rect(Screen.width*0.01,Screen.height*0.94,80,30),"Restart"))
	{
		Restart();
	}
	if (GUI.Button(Rect(Screen.width*0.01,Screen.height*0.89,80,30),"Reposition"))
	{
		WhiteReset();
	}
}

//Function to reposition the white ball
function WhiteReset()
{
	state = BallState.Resetting;
	Debug.Log("[whiteball]: Resetting");
	enable = false;
	audio.PlayOneShot(pocket);
	rigidbody.angularVelocity = Vector3(0,0,0);
	whitereset.renderer.enabled = true;
	cue.renderer.enabled = false;
	cuehelp.renderer.enabled = false;
	
	//Until the player press click, open up top camera view, position the ball at the position of the red box, move the red box accordingly to mouse position, but limit
	//it's movement to the table
	while (Input.GetButtonDown("Fire1") == false){
		//camtop.camera.rect = Rect(0,0,1,1);
		whitereset.position += Vector3(Input.GetAxis("Mouse X"), 0, Input.GetAxis("Mouse Y"));
		whitereset.position.x = Mathf.Clamp(whitereset.position.x,-11.6,17);
		whitereset.position.z = Mathf.Clamp(whitereset.position.z,-16.2,-2.4);
		transform.position = whitereset.position;
		yield;
	}
	//make the white ball perfectly still
	rigidbody.velocity = Vector3(0,0,0);
	rigidbody.angularVelocity = Vector3(0,0,0);
	////////////////////////////
	whitereset.renderer.enabled = false;
	yield WaitForSeconds(1);
	cue.renderer.enabled = true;
	cuehelp.renderer.enabled = true;
	state = BallState.Idle;
}

//Function to adjust where to hit the ball. Actually the cue itself in only used for visuals. The cuehelp object is what is used for the maths.
function CueAdjust(){
	
	maxx = Mathf.Clamp(maxx+ Input.GetAxis("Mouse X"),-0.4,0.4) ;
	maxy = Mathf.Clamp(maxy+ Input.GetAxis("Mouse Y"),-0.4,0.2) ;
	//Those bellow are just visuals
	cue.transform.localPosition.x = maxx;
	cue.transform.localPosition.y = maxy;
	//cue.transform.localRotation.y = 0.708 + maxx * 0.1;
	//cue.transform.localRotation.z = 0 - maxy * 0.03;
	///////
	cuehelp.localPosition.x = maxx;
	cuehelp.localPosition.y = maxy;
	cuehelp.localRotation.y = 0.708 + maxx*0.5; //We rotate the cuhelp to simulate the ball trajectory when hited at it's sides.
	
}

//When the white ball rigidbody sleeps, reset positions, rotations and enable player actions.
function Aim(){

	//transform.eulerAngles = Vector3(0,cam.rotation.y,0);
	cue.renderer.enabled = true;
	cuehelp.renderer.enabled = true;
	//cuehelp.localPosition = Vector3(0,0,11.2);
	//cuehelp.localEulerAngles = Vector3(0,270,0);
	enable = true;
	
}

function StartGame()
{
	Restart();
}

//Function that resets the game table. Balls are placed at correct positions.
function Restart()
{
	state = BallState.Resetting;
	Debug.Log("[whiteball]: restarting");
	topCamera.rect = Rect (0,0,1,1);
	cue.renderer.enabled = false;
	cuehelp.renderer.enabled = false;
	gameObject.Find("1").transform.position = Vector3(7.1,1.8,-9.2);
	gameObject.Find("2").transform.position = Vector3(7.8,1.8,-9.6);
	gameObject.Find("3").transform.position = Vector3(7.8,1.8,-8.8);
	gameObject.Find("4").transform.position = Vector3(8.5,1.8,-10.0);
	gameObject.Find("5").transform.position = Vector3(8.5,1.8,-9.2);
	gameObject.Find("6").transform.position = Vector3(8.5,1.8,-8.4);
	gameObject.Find("7").transform.position = Vector3(9.2,1.8,-10.4);
	gameObject.Find("8").transform.position = Vector3(9.2,1.8,-9.6);
	gameObject.Find("9").transform.position = Vector3(9.2,1.8,-8.8);
	gameObject.Find("10").transform.position = Vector3(9.2,1.8,-8.0);
	gameObject.Find("11").transform.position = Vector3(9.9,1.8,-10.8);
	gameObject.Find("12").transform.position = Vector3(9.9,1.8,-10.0);
	gameObject.Find("13").transform.position = Vector3(9.9,1.8,-9.2);
	gameObject.Find("14").transform.position = Vector3(9.9,1.8,-8.4);
	gameObject.Find("15").transform.position = Vector3(9.9,1.8,-7.6);
	//trigono.animation.Play();
	rigidbody.velocity = Vector3(0,0,0);
	rigidbody.angularVelocity = Vector3(0,0,0);
	transform.position = Vector3(-7.9,4.5,-9.2);
	
	//loop to place the ball icons in place spaced evenly
	var xpos:float = 0.1;
	for (var i = 1;i <= 15;i++){
		xpos = xpos + 0.03;
		gameObject.Find(i+"_icon").transform.localScale = Vector3(0,0,1);
		gameObject.Find(i+"_icon").GetComponent(GUITexture).enabled = true;
		gameObject.Find(i+"_icon").transform.position.x = xpos;
		//print("sorting...");
		yield WaitForSeconds(0.1);
		
	}
	
	yield WaitForSeconds(1); //wait a bit before we make visible the cue and cuehelp.
	cue.renderer.enabled = true;
	cuehelp.renderer.enabled = true;
	shotTaken = false;
	//print("Set to Idle");
	state = BallState.Idle;
}


//Thanks for purchasing
//Contact me at nuverian@creative-minds.gr for support