#pragma strict

var impact:AudioClip;
var pocket:AudioClip;

function Update () {
	
}


//If ball collides with another ball, get it's velocity and play the collision sound based on that velocity
function OnCollisionEnter (other:Collision){
	var vel:float = Mathf.Max(rigidbody.velocity.x,rigidbody.velocity.z);
	if (other.gameObject.tag == "8-ball" || other.gameObject.tag == "stripe-ball" || other.gameObject.tag == "solid-ball" || other.gameObject.tag == "ball"){
		audio.PlayOneShot(impact,vel*0.01);
	}
	
}

//If ball collides with the big box object named abyss, play a sound and the corresponding icon animation
function OnTriggerEnter (other:Collider){
	if (other.gameObject.name == "abyss"){
		audio.PlayOneShot(pocket);
		gameObject.Find(gameObject.name + "_icon").animation.Play();
		//Destroy(gameObject);
	}
	
	//In eithercase if the balls collide with any other trigger object (6 cylinders places at the 6 table pockets)
	// then apply those changes to force the ball into the pocket and not bounce in and out.
	rigidbody.mass = 1;
	rigidbody.AddForce(Vector3(0,-100,0));
	
}