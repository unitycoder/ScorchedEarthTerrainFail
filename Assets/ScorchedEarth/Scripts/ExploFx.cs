using UnityEngine;
using System.Collections;

public class ExploFx : MonoBehaviour {

	public Transform rootObj;
	public Vector3 mpos;

	private float size=0.0f;
	private float maxSize=0.5f;
	private float speed=1.0f;
	private bool done = false;

	
	void Start () {
		done=false;
	
	}
	
	void Update () 
	{
		
		if(size<maxSize)
		{
			
			size+=speed*Time.deltaTime;
			renderer.material.SetFloat( "_Radius", size );
			
		}else{
			

			if (done) return;
			done=true;			
			

			rootObj.SendMessage("Exploded", mpos, SendMessageOptions.DontRequireReceiver);
			Destroy(gameObject);
		}
		
		
	}
}
