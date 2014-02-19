#pragma strict

var cam:GameObject;
var menucam:GameObject;
var x:whiteball;
var cue:GameObject;

function Start(){
	x = GameObject.Find("white").GetComponent(whiteball);
//	cam.GetComponent(MouseOrbit).enabled = false;
	print("MainMenu: Started");
}

function OnMouseDown()
{
	menucam.SendMessage ("fadeOut");
	menucam.SetActive(false);
//	cam.GetComponent(MouseOrbit).enabled = true;
	print("MainMenu: Shifting to Game cam");
	x.StartGame();
	Destroy(gameObject);
}
function Update () {
}