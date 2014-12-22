using UnityEngine;
using System.Collections;

public class SliceMover : MonoBehaviour {

	public float distance = 0.0f;
	public float rowIndex = 0.0f;
	public float height = 0.0f;
	public bool done=false;
	private float Speed=0.0f;
	float Gravity=-9.8f;
	float Position;
	
	void Update () 
	{
	
		if(distance>0)
		{
			
			float Acceleration=Gravity;
			Position+=Acceleration*Time.deltaTime*Time.deltaTime;
			Speed+=Acceleration*Time.deltaTime;
			
			distance+=Position;
			transform.Translate(Vector3.up * Position, Space.World);
			
			
		}else{
			
			if (done) return;
			
			transform.parent.SendMessage("killSlice", 0, SendMessageOptions.DontRequireReceiver);
			done=true;
		}		
		
	}
}
