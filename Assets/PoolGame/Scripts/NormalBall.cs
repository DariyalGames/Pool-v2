using UnityEngine;
using System.Collections;

public class NormalBall : MonoBehaviour 
{
	#region Properties
	public int ballID;
	public BallTypes ballType;
	public AudioClip impact;
	public AudioClip pocket;
	public GameObject shadowPrefab;

	private GameObject shadow;
	private Vector3 shadowOffset = new Vector3(-0.2f, -0.4f, 0.2f);
	#endregion

	void Start() 
	{
		Debug.Log ("[NormalBall] Start");
		shadow = Instantiate (shadowPrefab, transform.position + shadowOffset, Quaternion.Euler (new Vector3(90, 0, 0))) as GameObject;
	}

	#region Updates
	// Update is called once per frame
	void Update () 
	{}

	void FixedUpdate()
	{
		if(rigidbody.velocity.y > 0.0f) 
		{
			Vector3 v = rigidbody.velocity;
			v.y = 0.0f;
			rigidbody.velocity = v;
		}
	}

	void LateUpdate()
	{
		shadow.transform.position = transform.position + shadowOffset;
	}
	#endregion

	#region Unity Events Listeners
	void OnCollisionEnter (Collision other)
	{
		float velocity = Mathf.Max (rigidbody.velocity.x, rigidbody.velocity.z);
		if(other.gameObject.tag == "ball")
		{
			audio.PlayOneShot (impact, velocity * 0.01f);
		}
	}

	void OnTriggerEnter (Collider other)
	{
		if(other.gameObject.name == "abyss")
		{
			audio.PlayOneShot (pocket);
			GameObject.Find (gameObject.name + "_icon").animation.Play ();
			Destroy (gameObject);
			Debug.Log ("[NormalBall]: Raising event pocketed.");
			EventManager.instance.Raise (new BallPocketEvent(ballID, ballType));
		}

		rigidbody.AddForce (new Vector3(0.0f, -100.0f, 0.0f));
	}
	#endregion
}
