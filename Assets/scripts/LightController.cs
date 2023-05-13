using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if WINDOWS_UWP
using Windows.Storage;
using Windows.System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
#endif


public class LightController : MonoBehaviour{
    [SerializeField] Material Blooming, nonBlooming;
    [SerializeField] GameObject Light1, Light2, Light3, Light4, Light5;

#if WINDOWS_UWP
    Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
    Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
#endif

    List<GameObject> SceneLights = new List<GameObject>();
    //CSVLogger myLogger = new CSVLogger();
    float timer = 0;
    bool isVirtual, MaterialOff = false, firstSave = true;
    int SleepDuration, LEDNumber, ResetDuration;
    string saveInformation;
    private static string fileName = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + ".txt";

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

#if WINDOWS_UWP 
        WriteData("year-month-day_hour-minute-second-ms , LED_number , isVirtual");
#endif
    }

#if WINDOWS_UWP
//following code is acquired from here: forums.hololens.com/discussion/3290/save-a-string-to-a-text-or-cvs-file-on-hololens
    async void WriteData(string DataToWrite){
    // text is saved to: "User Files\LocalAppData[APP name]\LocalState\[file name]
        if (firstSave){
            StorageFile sampleFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.AppendTextAsync(sampleFile, DataToWrite + "\r\n");
            firstSave = false;
        }
		else{
			StorageFile sampleFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
			await FileIO.AppendTextAsync(sampleFile, DataToWrite + "\r\n");
		}
    }
#endif

    void LightUpVirtualMaterial(GameObject Light, bool TurnOn) {
		if (TurnOn) {
            Light.GetComponent<Renderer>().material = Blooming;
            return;
        }
        Light.GetComponent<Renderer>().material = nonBlooming;
    }

	void Update(){
        /*generate random number -> determines which LED [1-5]
                                 -> determines if physical or virtual [0-1]
                                 -> determines how long it will be awake [1] seconds
                                 -> determines how long reset period will last [4-10] seconds
        this process will repeat until program termination*/


        if (timer == 0) {//we haven't started yet, initialize the scene
            SleepDuration = 1;//SleepDuration = Random.Range(3, 6);
            LEDNumber = UnityEngine.Random.Range(0, 5);
            isVirtual = (UnityEngine.Random.value > 0.5f);
            ResetDuration = UnityEngine.Random.Range(4, 11);

            Debug.Log("Lighting LED#" + LEDNumber + " isVirtual: " + isVirtual +
                "\nFor " + SleepDuration + " seconds, sleeping for " + ResetDuration + " seconds");
#if WINDOWS_UWP
            saveInformation = System.DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss-fff") + ", " + LEDNumber + ", " + isVirtual;
            WriteData(saveInformation);
#endif


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
