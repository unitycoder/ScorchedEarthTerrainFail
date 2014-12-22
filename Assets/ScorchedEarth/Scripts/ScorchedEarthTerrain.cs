// Scorched Earth Terrain Test : unitycoder.com

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class ScorchedEarthTerrain : MonoBehaviour 
{

	public GUIText coords;
	public Transform sliceExploPrefab;
	public Transform exploPrefab;
	public Material sliceMaterial;
	public Transform closerCam;
	
	private float radius = 12.0f;
	
	private int slices = 0;
	
	private int resolution = 2; // only works with event numbers!
	private int width;
	float[] height;
	Vector3[] newVertices;

	float h;
	
	int ti=0;
	
	private Mesh mesh;
	
	void Start () 
	{
		width = Screen.width;
		buildTerrainMesh2();
	}
	
	void Update () 
	{
		Vector2 mpos = Input.mousePosition;
		
		// display height of that row
		if (mpos.x>0 && mpos.x<width)
		{
			
			int rowIndex = (int)mpos.x/resolution;
			h = height[rowIndex];

		
			// mouse is inside terrain
			if (mpos.y>0 && mpos.y<h)
			{
				// make hole
				if (Input.GetMouseButtonDown(0))
				{
					Transform clone = Instantiate(exploPrefab, new Vector3(mpos.x-width/2,mpos.y-Screen.height/2,0), Quaternion.identity) as Transform;
					clone.gameObject.GetComponent<ExploFx>().mpos = new Vector3(mpos.x,mpos.y,rowIndex);
					clone.gameObject.GetComponent<ExploFx>().rootObj = transform;
					closerCam.position = clone.position-new Vector3(0,-50,90);
					
				}
				
			}

			// TODO: check borders
			// dirtball
			if (Input.GetMouseButtonDown(1))
			{
				List<Vector4> slice = new List<Vector4>();
				int index = (int)mpos.x/resolution;
				float angle, nh;
				// create dirtball slices
				for (int i=1;i<radius-1;i++)
				{
					angle = Mathf.Asin(i/radius);
					nh = Mathf.Cos(angle)*radius;

					slice.Add( new Vector4( newVertices[(index+i)*2].x, mpos.y, nh*2 ,  (mpos.y-nh*2) - (newVertices[(index+i)*2].y)));
					slices++;
					
					slice.Add( new Vector4( newVertices[(index-i)*2].x, mpos.y, nh*2 ,  (mpos.y-nh*2) - (newVertices[(index-i)*2].y)));
					slices++;
					
					slice.Add( new Vector4( newVertices[(index+i)*2].x, mpos.y+nh*2, nh*2 ,  (mpos.y-nh*2) - (newVertices[(index+i)*2].y)));
					slices++;
					
					slice.Add( new Vector4( newVertices[(index-i)*2].x, mpos.y+nh*2, nh*2 ,  (mpos.y-nh*2) - (newVertices[(index-i)*2].y)));
					slices++;
				}

				angle = Mathf.Asin(1/radius);
				nh = Mathf.Cos(angle)*radius;
				slice.Add( new Vector4( newVertices[(index)*2].x, mpos.y, nh*2 ,  (mpos.y-nh*2) - (newVertices[(index)*2].y)));
				slices++;
				slice.Add( new Vector4( newVertices[(index)*2].x, mpos.y+nh*2, nh*2 ,  (mpos.y-nh*2) - (newVertices[(index)*2].y)));
				slices++;

				for (int s=0;s<slice.Count;s++)
				{
					makeSlice(slice[s]);
				}					
				
				slice.Clear();
				
				closerCam.position = new Vector3(mpos.x,mpos.y-50,90);
				
			}
			
			
		}
		coords.text = "mouse:"+mpos+" | height:"+h;
		
		
		if (Input.GetKeyDown("f"))
		{
			WeldAllSlices();
		}
		
		// test
		if (Input.GetKeyDown("1"))
		{
			// take 1 child, weld it
			WeldAllSlices();
		}
		
	}
	
	
	void Exploded(Vector3 in_mpos)
	{
		height[(int)in_mpos.z] = in_mpos.y;
		updateMesh((int)in_mpos.z, in_mpos);
		
	}
	
	void killSlice(int none)
	{
		slices--;
		if (slices<1)
		{
			//weld
			WeldAllSlices();
		}
	}
	
	// weld slice(s) back to terrain
	void WeldAllSlices()
	{
		for (int i=transform.childCount-1;i>-1;i--)
		{
			Transform t = transform.GetChild(i);
			newVertices[(int)((t.position.x+2)+width/2)] += new Vector3(0,t.renderer.bounds.size.y,0);
			Destroy(transform.GetChild(i).gameObject);
			mesh.vertices = newVertices;
		}		

	}
	
	void updateMesh(int index, Vector2 mpos)
	{
		int start = (int)(index-radius)*2;
		int end = (int)(index+radius)*2;
		
		Debug.Log("hitcenter:"+mpos+" | hitindex:"+index+" | startvert:"+start+" | endvert:"+end + " | radius:"+radius);

		List<Vector4> slice = new List<Vector4>();

		
		int oldpos=-1;
		float counter=1.0f;

		for (int i=1;i<radius-1;i++)
		{
			float angle = Mathf.Asin(counter/radius);
			float h = Mathf.Cos(angle)*radius;
		
			
			if (newVertices[(index+i)*2].y>mpos.y)
			{
				if (newVertices[(index+i)*2].y > (mpos.y+h*2))
				{
					slice.Add( new Vector4( newVertices[(index+i)*2].x, newVertices[(index+i)*2].y, newVertices[(index+i)*2].y-(mpos.y+h*2),  Mathf.Abs( ((mpos.y+h*2) - (mpos.y-h*2+radius/2)) ) ));
					slices++;
				}
				newVertices[(index+i)*2] = new Vector3(newVertices[(index+i)*2].x, mpos.y-h*2+radius/2 ,newVertices[(index+i)*2].z);
			}
			
			if (newVertices[(index-i+2)*2].y>mpos.y)
			{
				if (newVertices[(index-i+2)*2].y > (mpos.y+h*2))
				{
					slice.Add( new Vector4( newVertices[(index-i+2)*2].x, newVertices[(index-i+2)*2].y, newVertices[(index-i+2)*2].y-(mpos.y+h*2),  Mathf.Abs( ((mpos.y+h*2) - (mpos.y-h*2+radius/2)) ) ));
					slices++;
				}
				
				newVertices[(index-i+2)*2] = new Vector3(newVertices[(index-i+2)*2].x, mpos.y-h*2+radius/2 ,newVertices[(index-i+2)*2].z);
			}
			
			counter+=1.0f;
		}

		for (int s=0;s<slice.Count;s++)
		{
			makeSlice(slice[s]);
		}
		
		
		mesh.vertices = newVertices;
		mesh.RecalculateBounds();
	}
	
	
	// slicedata: x=xpos, y=ypos, z=height, w=distance to move
	GameObject makeSlice(Vector4 sliceData)
	{
		GameObject go = new GameObject();
		Mesh sliceMesh = new Mesh();
		go.AddComponent<MeshFilter>().mesh = sliceMesh;
		go.AddComponent<MeshRenderer>();		
		
		Vector3[] sliceVertices = new Vector3[4];
		int[] sliceIndices = new int[4];
		Vector2[] sliceUvs = new Vector2[4];

		// bottom left
		sliceVertices[0] = new Vector3(-1,0,0);
		sliceIndices[0] = 0;
		sliceUvs[0] = new Vector2(0,0);
		
		// top left
		sliceVertices[1] = new Vector3(-1,sliceData.z,0);
		sliceIndices[1] = 1;
		sliceUvs[1] = new Vector2(0,1);
		
		// top right
		sliceVertices[2] = new Vector3(resolution,sliceData.z,0);
		sliceIndices[2] = 2;
		sliceUvs[2] = new Vector2(1,1);
		
		// bottom right
		sliceVertices[3] = new Vector3(resolution,0,0);
		sliceIndices[3] = 3;
		sliceUvs[3] = new Vector2(1,0);
		
		sliceMesh.vertices = sliceVertices;
		sliceMesh.uv = sliceUvs;
		sliceMesh.SetIndices(sliceIndices,MeshTopology.Quads,0);
		
		sliceMesh.RecalculateNormals();
		sliceMesh.Optimize();
		sliceMesh.RecalculateBounds();		
		
		// move to right pos?
		go.transform.position = new Vector3(sliceData.x-width/2,sliceData.y-Screen.height/2-sliceData.z,0);
		
		go.transform.parent = transform;
		go.name = "slice"+sliceData.x;
		
		//go.AddComponent<Rigidbody>();
		go.AddComponent<SliceMover>();
		
		go.GetComponent<SliceMover>().distance = sliceData.w;
		go.GetComponent<SliceMover>().rowIndex = sliceData.x; // xpos
		//go.GetComponent<SliceMover>().height = sliceData.x; // xpos
		
		go.renderer.material = sliceMaterial;
		
		Transform clone = Instantiate(sliceExploPrefab,go.transform.position+new Vector3(0,sliceData.z,0),Quaternion.identity) as Transform;

		Destroy(clone.gameObject,5);
		return go;
	}
	
	
	
	Mesh buildTerrainMesh()
	{
		mesh = new Mesh();

		newVertices = new Vector3[width*4];
		int[] newIndices = new int[width*4];
		Vector2[] newUvs = new Vector2[width*4];
		height = new float[width];
		
		int i=-1;
		int maxHeight=255;
		float perlinScale = 0.007f;
		float y=0.0f; // using ints?
		float sx = 0.0f; // screen X
		
		for (int x=0;x<width;x+=resolution)
		{
			
			y = 1+Mathf.PerlinNoise(x*perlinScale,0)*maxHeight;

			height[x]=y;

			sx=x;
			
			// top left
			i++;
			newVertices[i] = new Vector3(sx,y,0);
			newIndices[i] = i;
			newUvs[i] = new Vector2(0,1);
	
			// top right
			i++;
			newVertices[i] = new Vector3(sx+resolution,y,0);
			newIndices[i] = i;
			newUvs[i] = new Vector2(1,1);
			
			// bottom right
			i++;
			newVertices[i] = new Vector3(sx+resolution,0,0);
			newIndices[i] = i;
			newUvs[i] = new Vector2(1,0);
			
			//Debug.Log(x);
			// bottom left
			i++;
			newVertices[i] = new Vector3(sx,0,0);
			newIndices[i] = i;
			newUvs[i] = new Vector2(0,0);
			
		}
		
		mesh.vertices = newVertices;
		mesh.uv = newUvs;
		mesh.SetIndices(newIndices,MeshTopology.Quads,0);
		
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();	
		
		GetComponent<MeshFilter>().mesh = mesh;
		
		transform.position = new Vector3(-width/2,-Screen.height/2,0);
		
		return mesh;
	}
	
	Mesh buildTerrainMesh2()
	{
		
		// add mesh to object
		mesh = new Mesh();
		
		List<Vector3> verts = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> indices = new List<int>();


		
		// TODO: build quad mesh for screen size, /\_/\
		newVertices = new Vector3[width*2];
		int[] newIndices = new int[width*2];
		Vector2[] newUvs = new Vector2[width*2];
		height = new float[width/2];
		
		int v=-1;
		int maxHeight=255;
		float perlinScale = 0.007f;
		float y=0.0f; // using ints?
		float y2=0.0f; // using ints?
		float sx = 0.0f; // screen X
		float xx = 0;
		int connect = 0;
		
		
		// vert 0
		verts.Add(new Vector3(0,0,0));
		v++;
		uvs.Add(new Vector2(0,1));
		indices.Add(v);
		
		// vert 1
		verts.Add(new Vector3(0,0,0));
		v++;
		uvs.Add(new Vector2(0,1));
		//Debug.Log(verts.Count);
		indices.Add(v);
		
		int index=0;

		//Debug.Log (height.Length);
		Debug.Log ("Make sure screen width is even value!, other wise you get error after few lines from here..");

		
		for (int x=0;x<width;x+=resolution)
		{

			y = 1+Mathf.PerlinNoise(x*perlinScale,0)*maxHeight;

			height[index]=y;
			index++;
				// top 
				verts.Add(new Vector3(x,y,0));
				v++;
				uvs.Add(new Vector2(0,1));

				indices.Add(v);

			
				// bottom 
				verts.Add(new Vector3(x,0,0));
				v++;
				uvs.Add(new Vector2(0,0));
				indices.Add(v);

				// connect this to previous bottom column
				if (x % 2 == 0)
				{
						indices.Add(v);
						indices.Add(v-1);
					
	
				}else{
						indices.Add(v-1);
						indices.Add(v);
					
				}

		}

		newVertices = verts.ToArray();
		mesh.vertices = verts.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.SetIndices(indices.ToArray(),MeshTopology.Quads,0);
		
		mesh.RecalculateNormals();
		//mesh.Optimize();
		mesh.RecalculateBounds();	
		
		GetComponent<MeshFilter>().mesh = mesh;
		
		transform.position = new Vector3(-width/2,-Screen.height/2,0);
		
		return mesh;
	}
	
}
