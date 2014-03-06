using UnityEngine;
using System.Collections;

public class CueBall : MonoBehaviour 
{
	#region Public Properties
	public Transform whiteReset;

	public AudioClip pocket;
	public AudioClip impact;
	public AudioClip cuehit;
	public Camera topCamera;

	public GameObject cue;

	public GUITexture guibar;
    public Texture whiteBallIcon;
    public Texture cueMarker;
    public Vector2 gizmoPos = Vector2.zero;

	public Transform imaginaryCursor;
	public GameObject shadowPrefab;
	#endregion

	#region Private Properties
    private enum ShotTypes { Normal, Stun, Topspin, Backspin, RightEnglish, LeftEnglish }
    private ShotTypes shotType;
	private LineRenderer lineRenderer;

	private float mouseDistance = 0.0f;
	private float cuestr = 0.0f;
    private float maxx = 0.0f;
    private float maxy = 0.0f;
    private Vector3 shotDirection;
    private float shotVelocity;
	private bool shotTaken = false;
	private bool movingCue = false;

	private Vector3 initialCuePosition = new Vector3(0.0f, 0.1f, -0.4f);
	private Vector3 cueStartPostition;
	private float cueMovementStartTime = 0.0f;
	private float shotTime = 0.1f;

	private Vector3 lastMousePosition;
    private int noOfCollisions;

	private GameObject shadow;
	private Vector3 shadowOffset = new Vector3(-0.2f, -0.4f, 0.2f);
	#endregion

	// Use this for initialization
	void Start () 
	{
		EventManager.instance.AddListener<StateChanged> (OnStateChanged);
		//EventManager.instance.AddListener<ShotTaken> (OnShotTaken);
		lineRenderer = GetComponent<LineRenderer> ();
		lineRenderer.enabled = false;
		whiteReset.renderer.enabled = false;

		shadow = Instantiate (shadowPrefab, transform.position + shadowOffset, Quaternion.Euler (new Vector3(90, 0, 0))) as GameObject;
		guibar.pixelInset = new Rect (-64f,-29f,78f,32f);

		Reset ();
	}

	#region Updates
	// FixedUpdate is called when physics is updated.
	void FixedUpdate ()
	{
		if(rigidbody.velocity.y > 0.0f) 
		{
			Vector3 v = rigidbody.velocity;
			v.y = 0.0f;
			rigidbody.velocity = v;
		}

		switch(GameManager.Instance.CurrentState)
		{
		case GameStates.Shooting:
			if (shotTaken)
			{			
				lineRenderer.enabled = false;
				cue.renderer.enabled = false;
				cue.transform.localPosition = new Vector3(0.0f, 0.1f, -0.4f);

                shotDirection = transform.forward;
                noOfCollisions = 0;
				rigidbody.AddForce(cuestr * transform.forward);
				
				cuestr = 0;
				audio.PlayOneShot(cuehit);
				guibar.pixelInset = new Rect (-64f,-29f,78f,32f);
				mouseDistance = 0;
				shotTaken = false;
				movingCue = false;
				EventManager.instance.Raise (new ShotTaken());
			}
			break;
		case GameStates.BallMove:
			if ((rigidbody.IsSleeping ()) && (rigidbody.velocity.magnitude == 0))
			{			
				rigidbody.velocity = new Vector3(0.0f, 0.0f, 0.0f);
				cue.renderer.enabled = true;
				
				EventManager.instance.Raise (new BallStopped());
			}
			break;
		}
	}

	// Update is called once per frame
	void Update () 
	{
		switch(GameManager.Instance.CurrentState)
		{
		case GameStates.PlayerTurn:
			if (Input.GetButtonDown("Fire1"))
			{
				lastMousePosition = Input.mousePosition;
                if (maxx <= 0.04f && maxx >= -0.04f)
                {
                    if (maxy <= 0.02f && maxy >= -0.02f)
                    {
                        Debug.Log("[CueBall] {NormalShot} zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz6");
                        shotType = ShotTypes.Normal;
                    }
                    else if (maxy > 0.02f)
                    {
                        Debug.Log("[CueBall] {topspin} xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
                        shotType = ShotTypes.Topspin;
                    }
                    else if (maxy < -0.02f && maxy > -0.08f)
                    {
                        Debug.Log("[CueBall] {stun}");
                        shotType = ShotTypes.Stun;
                    }
                    else
                    {
                        Debug.Log("[CueBall] {backspin}");
                        shotType = ShotTypes.Backspin;
                    }
                }
                else if (maxx < -0.04f)
                {
                    Debug.Log("[CueBall] {left english}");
                    shotType = ShotTypes.LeftEnglish;
                }
                else
                {
                    Debug.Log("[CueBall] {right english}");
                    shotType = ShotTypes.RightEnglish;
                }

				//Debug.Log ("[CueBall]: event raise shot power");
				EventManager.instance.Raise (new PowerSelect());
			}
			else 
			{
				Ray ray = topCamera.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				if(Physics.Raycast (ray, out hit, 100.0f))
				{
					imaginaryCursor.position = new Vector3(hit.point.x, transform.position.y, hit.point.z);
				}
				transform.LookAt (imaginaryCursor);
				
				RaycastHit rayhit;
				Physics.Raycast (transform.position, transform.forward, out rayhit);
				lineRenderer.enabled = true;
				lineRenderer.SetPosition(0, gameObject.transform.position);
				lineRenderer.SetPosition(1, rayhit.point);

                maxx = Mathf.Clamp(maxx + Input.GetAxis("Horizontal") * 0.1f, -0.32f, 0.32f);
                maxy = Mathf.Clamp(maxy + Input.GetAxis("Vertical") * 0.1f, -0.32f, 0.16f);
                //Debug.Log("{CueBall] {maxx,maxy=" + maxx.ToString() + "," + maxy.ToString() + "}");
			}
			break;
		case GameStates.Shooting:
			if (Input.GetButtonUp("Fire1") && !movingCue)
			{
				movingCue = true;
				cueMovementStartTime = Time.time;
				cueStartPostition = cue.transform.localPosition;
				StartCoroutine (MoveCue ());
			}
			else if(movingCue)
			{
				float t = (Time.time - cueMovementStartTime)/shotTime;
				cue.transform.localPosition = Vector3.Lerp (cueStartPostition, initialCuePosition, t);
			}
			else 
			{
				Vector3 currentMousePosition;
				float delta;
				float lastMouseDistance;
				
				currentMousePosition = new Vector3(Input.mousePosition.x, 0.0f, Input.mousePosition.y);
				lastMouseDistance = mouseDistance;
				
				delta = Vector3.Dot((currentMousePosition - lastMousePosition), transform.forward.normalized);
				mouseDistance += delta;
				mouseDistance = Mathf.Clamp(mouseDistance, -75f, 0.0f);
				
				delta = mouseDistance - lastMouseDistance;
				
				cuestr = -mouseDistance * 5.0f;
				guibar.pixelInset = new Rect (-64f,-29f,78f + (cuestr * 0.2587f),32f);
				
				cue.transform.Translate(new Vector3((delta * 0.05f) , 0f, 0f), Space.Self);
				
				lastMousePosition = currentMousePosition;
				//print(mouseDistance);
			}
			break;
/*		case GameStates.BallMove:
			if (cue.renderer.enabled)
			{
				Debug.Log ("[CueBall]: cue is rendering");
				cue.renderer.enabled = false;
				//cuehelp.renderer.enabled = false;
			}
			break;*/
		case GameStates.Reposition:
			if(Input.GetButtonDown ("Fire1"))
			{
				whiteReset.renderer.enabled = false;
				cue.renderer.enabled = true;
                rigidbody.velocity = Vector3.zero;
				
				EventManager.instance.Raise (new RepositionDone());
			}
			else
			{
				//whiteReset.position = new Vector3(Mathf.Clamp(Input.GetAxis("Mouse X"), -11.6f, 17f), whiteReset.position.y, Mathf.Clamp(Input.GetAxis("Mouse Y"), -16.2f, -2.4f));	
				whiteReset.position += new Vector3(Input.GetAxis("Mouse X"), 0.0f, Input.GetAxis("Mouse Y"));
				transform.position = whiteReset.position;
			}
			break; 
		}
	}

	void LateUpdate()
	{
		shadow.transform.position = transform.position + shadowOffset;
	}

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(gizmoPos.x, gizmoPos.y, 50.0f, 50.0f), whiteBallIcon);
        float markerXPos = gizmoPos.x + 20f + maxx * 50f / 0.64f;
        float markerYPos = gizmoPos.y + 20f - maxy * 50f / 0.64f;
        GUI.DrawTexture(new Rect(markerXPos, markerYPos, 10f, 10f), cueMarker);
    }
	#endregion

	#region Private Methods
	private void Reset ()
	{
		cue.renderer.enabled = false;

		rigidbody.velocity = new Vector3(0.0f, 0.0f, 0.0f);
		rigidbody.angularVelocity = new Vector3(0.0f, 0.0f, 0.0f);
		transform.position = new Vector3(-7.9f, 4.5f, -9.2f);

		EventManager.instance.Raise (new InGameStart());
	}

	private IEnumerator MoveCue ()
	{
		//Debug.Log ("[CueBall] moving cue");
		yield return new WaitForSeconds(shotTime);
		shotTaken = true;
	}

	private IEnumerator WaitBeforeHidingCue ()
	{
		yield return new WaitForSeconds(0.3f);
		cue.renderer.enabled = false;
	}

    private Vector2 perpendicular(Vector2 input)
    {
        float x = Mathf.Sqrt(1/(1+(input.x*input.x)/(input.y*input.y)));
        return new Vector2(x, x * (-input.x / input.y)); 
    }
	#endregion

	#region Event Listeners

	private void OnCollisionEnter(Collision other)
	{
        noOfCollisions++;
		shotVelocity = Mathf.Max (rigidbody.velocity.x, rigidbody.velocity.z);
		if(other.gameObject.tag == "ball")
		{
			audio.PlayOneShot (impact, shotVelocity * 0.01f);
		}
 	}

    private void OnCollisionExit(Collision other)
    {
        if (noOfCollisions <= 1)
        {
            Vector2 perp = perpendicular(new Vector2(shotDirection.x, shotDirection.z));
            if (other.gameObject.tag == "ball" || other.gameObject.name == "borders")
            {
                switch (shotType)
                {
                    case ShotTypes.Stun:
                        rigidbody.velocity = Vector3.zero;
                        break;
                    case ShotTypes.Backspin:
                        rigidbody.velocity = Vector3.zero;
                        rigidbody.AddForce(-shotDirection * shotVelocity * 10f);
                        //rigidbody.AddForce(-shotDirection * shotVelocity * 2f * 1.5f);
                        //shotType = ShotTypes.Normal;
                        break;
                    case ShotTypes.Topspin:
                        //rigidbody.velocity = Vector3.zero;
                        rigidbody.AddForce(shotDirection * shotVelocity * 10f);
                        //rigidbody.AddForce(shotDirection * shotVelocity *2f);
                        //shotType = ShotTypes.Normal;
                        break;
                    case ShotTypes.LeftEnglish:
                        //rigidbody.velocity = Vector3.zero;                   
                        rigidbody.AddForce(-(new Vector3(perp.x, 0, perp.y)) * shotVelocity * 10f);
                        break;
                    case ShotTypes.RightEnglish:
                        //rigidbody.velocity = Vector3.zero;                  
                        rigidbody.AddForce((new Vector3(perp.x, 0, perp.y)) * shotVelocity * 10f);
                        break;
                }
            }
        }
    }

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log ("[CueBall]: event (OnTriggerEnter) triggered by " + other.gameObject.name);
		if (other.gameObject.name.ToLower () == "abyss")
		{
			Debug.Log ("[CueBall]: event (OnTriggerEnter) abyss");
			audio.PlayOneShot (pocket);

			rigidbody.angularVelocity = new Vector3(0f, 0f, 0f);
			whiteReset.renderer.enabled = false;
			cue.renderer.enabled = false;

			EventManager.instance.Raise (new WhiteReposition());
			//StartCoroutine(Reposition ());
		}
	}

	private void OnStateChanged (StateChanged e)
	{
		//Debug.Log ("[CueBall]: event stat changed");
		switch(GameManager.Instance.CurrentState)
		{
		case GameStates.TurnStart:
			cue.renderer.enabled = true;
			lineRenderer.enabled = true;
            maxx = 0.0f;
            maxy = 0.0f;
			EventManager.instance.Raise (new TurnReady());
			break;
		case GameStates.BallMove:
			lineRenderer.enabled = false;
			StartCoroutine (WaitBeforeHidingCue ());
			break;
		}
	}
	#endregion
}
