using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class PerformanceTestRunner : MonoBehaviour {


	public    string    report_text
	{
		get;
		protected set;
	}
	public    float     fpsWaitTime  = 3; //add object wait time
	public    int       testCount    = 30;
	public    int       fpsTestLimitValue = 60;
	public    Material  test_object_material;
	protected Stack<GameObject> testGameobjectLst = new Stack<GameObject>();
	[System.NonSerialized]
	protected int       currentFps   = 0;
	public    int       precisionFps = 5;
	[System.NonSerialized]
	public  bool 	    testOver  = false;

	void Awake()
	{
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}
	void Start()
	{
		execute();
	}

	public void execute()
	{
		StartCoroutine(run());
	}
	

	protected virtual IEnumerator run()
	{

		yield return 1;
	}
	
	protected IEnumerator accurateTestFps()
	{
		yield return new WaitForSeconds(fpsWaitTime);
	
		int   count  = 0;
		int   fpsSum = 0;
		while(count < testCount )
		{
			count++;
			fpsSum += FPSCounter.Instance.currentFps;
			yield return 0;
		}
		currentFps = fpsSum/count;
		yield return 1;
	}
	protected virtual void biuldReport()
	{
        //Debug.Log("???????????00");
		this.report_text = "";
		this.report_text += "[99ff00]GameObject Count1 [-]: " + testGameobjectLst.Count.ToString()+ "\n";
		int triangle_count = 0;
		int vertex_count   = 0;
		
		foreach(var item in testGameobjectLst)
		{
			var ms = item.GetComponent<MeshFilter>().mesh;
			triangle_count += ms.triangles.Length;
			vertex_count   += ms.vertexCount;
		}
		triangle_count /= 3;
		
		
		this.report_text += "[99ff00]Triangle Count [-]: "  + (triangle_count/1000.0).ToString() + "k \n";
		this.report_text += "[99ff00]Vertices Count [-]: "  + (vertex_count/1000.0).ToString() + "k \n";
		this.report_text += "[99ff00]FPS Limit Count [-]: " + this.fpsTestLimitValue.ToString() +"\n";

		
	}
	void Update()
	{
		MUI.Instance.ReportLable.text = this.report_text;
	}
}
