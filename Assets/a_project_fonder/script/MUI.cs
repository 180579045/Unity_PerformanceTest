using UnityEngine;
using System.Collections;

public class MUI : MonoBehaviour {

	public UILabel FpsLable;
	public UILabel ReportLable;
	static MUI _instance;
	public GameObject runer;
	public GameObject menuBtn;

	string report_text;

	public static MUI Instance{
		get{
			return _instance;
		}
	}
	void Awake()
	{
		DontDestroyOnLoad(this);

		_instance = this;
	}


	void Start () {
	
		UIEventListener.Get(runer).onClick += click ;
		menuBtn.GetComponent<ui_state_btn>().stateChange += statachanged;
	}
	void statachanged(GameObject g)
	{
		MUI.Instance.runer.SetActive(false);
		if(g.name=="Triangle")
		{
			Application.LoadLevel(1);
		}
		else if(g.name == "Drawcall")
		{
			
			Application.LoadLevel(2);
		}
		else if(g.name == "AlphaBlend")
		{
			Debug.Log(g.name);
			Application.LoadLevel(3);
			
		}
		else if(g.name == "AlphaTest")
		{
			Application.LoadLevel(4);
			
		}
		else if(g.name == "SkinMesh")
		{
			
			Application.LoadLevel(5);
		}
		else if(g.name == "Rigibody")
		{
			Application.LoadLevel(6);
			
		}
		else{
			
			
		}
	}
	int curent_level =1;
	void click(GameObject g)
	{
//		menuBtn.SetActive(false);
//		g.SetActive(false);
//		Application.LoadLevel(curent_level);

	}

	void Update () {
		FpsLable.text = FPSCounter.Instance.currentFpsText;

//		GameObject g = GameObject.Find("runner");
//		if(g==null)
//			return;
//		PerformanceTestRunner runner = g.GetComponent<PerformanceTestRunner>();
//		if(runner == null)
//			return;
//		if(runner.testOver && curent_level<6)
//		{
//			if(curent_level == 1)
//			{
//				report_text += "Triangle Test:\n";
//			}
//			else if(curent_level ==2)
//			{
//				report_text += "draw call Test:\n";
//			}
//			else if(curent_level ==3)
//			{
//				report_text +="AlphaBlend Test:\n";
//			}
//			else if(curent_level ==4)
//			{
//				report_text += "AlphaTest Test:\n";
//			}
//			else if(curent_level ==5)
//			{
//				report_text += "SkinMesh Test: \n";
//			}
//			report_text  += runner.report_text;
//			curent_level +=1;
//			Application.LoadLevel(curent_level);
//		    
//		}
//		else if(runner.testOver && curent_level ==6)
//		{
//			report_text += "Rigibody Test:\n";
//			report_text += runner.report_text;
//			ReportLable.text = report_text;
//		}

	}
}
