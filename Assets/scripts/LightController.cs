using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour{
    [SerializeField] Material Blooming, nonBlooming;
    [SerializeField] GameObject Light1, Light2, Light3, Light4, Light5;

    List<GameObject> SceneLights = new List<GameObject>();
    float timer = 0;
    bool isVirtual, MaterialOff = false;
    int SleepDuration, LEDNumber, ResetDuration;

	/*Instead of having 5 different materials for the 5 lights, have 2 materials, 1 emissive and 1 non-emissive.
     Change between these materials as needed. This should be faster and more efficient 
    (and I can't fucking find the emission toggle so fuck it this works)
     */

	private void Start(){
        SceneLights.Add(Light1);
        SceneLights.Add(Light2);
        SceneLights.Add(Light3);
        SceneLights.Add(Light4);
        SceneLights.Add(Light5);
    }

	void LightUpVirtualMaterial(GameObject Light, bool TurnOn) {
		if (TurnOn) {
            Light.GetComponent<Renderer>().material = Blooming;
            return;
        }
        Light.GetComponent<Renderer>().material = nonBlooming;
    }

	// Update is called once per frame
	void Update(){
		//generate random number -> determines which LED [1-5]
		//generate random number -> determines if physical or virtual [0-1]
		//generate random number -> determines how long it will be awake [3-5] seconds
        //generate random number -> determines how long reset period will last [3-5] seconds

		//this process will repeat until program termination


		if (timer == 0) {//we haven't started yet, initialize the scene
            SleepDuration = 1;//SleepDuration = Random.Range(3, 6);
            LEDNumber = Random.Range(0, 5);
            isVirtual = (Random.value > 0.5f);
            ResetDuration = Random.Range(4, 11);

            Debug.Log("Lighting LED#" + LEDNumber + " isVirtual: " + isVirtual +
                "\nFor " + SleepDuration + " seconds, sleeping for " + ResetDuration + " seconds");

            if (isVirtual) LightUpVirtualMaterial(SceneLights[LEDNumber], true);
			else {//------------------------------------------------TO DO------------------------------------------------
                //this part will send the LEDNumber to the LED controller hardware, which will then turn on the said LED
                //Kutluhan is looking into it, will update if he does it
            }
        }
        //sleep for SleepDuration + ResetDuration seconds
        timer += Time.deltaTime;

		if (timer > SleepDuration) {//LED has been on for SleepDuration seconds
            if (!MaterialOff) {//this must happen only once
                if (isVirtual)
                    LightUpVirtualMaterial(SceneLights[LEDNumber], false);//turn off LED
                else {//------------------------------------------------TO DO------------------------------------------------
                    //Send a turn off command to the physical controller
                }
                
                MaterialOff = true;
            }
                
            //sleep for ResetDuration seconds
			if (timer > (SleepDuration + ResetDuration)) {
                //reset the scene and start from top
                MaterialOff = false;
                timer = 0;
                //LED is off, decision variables are reset
            }
        }
    }
}
