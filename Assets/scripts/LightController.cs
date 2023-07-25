using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if WINDOWS_UWP
using Windows.Storage;
using Windows.System;
using System.Threading.Tasks;
using Windows.Storage.Streams;

using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Devices.Bluetooth;

Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService _service;
Windows.Networking.Sockets.StreamSocket _socket;
#endif

public class LightController : MonoBehaviour{
    [SerializeField] Material Blooming, nonBlooming;
    [SerializeField] GameObject Light1, Light2, Light3, Light4, Light5;


#if WINDOWS_UWP
    Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
    Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

    bool firstSave = true;


// This App requires a connection that is encrypted but does not care about
// whether it's authenticated.
bool SupportsProtection(RfcommDeviceService service){
    switch (service.ProtectionLevel){
        case SocketProtectionLevel.PlainSocket:
            if ((service.MaxProtectionLevel == SocketProtectionLevel
                    .BluetoothEncryptionWithAuthentication)
                || (service.MaxProtectionLevel == SocketProtectionLevel
                    .BluetoothEncryptionAllowNullAuthentication)){
                // The connection can be upgraded when opening the socket so the
                // App may offer UI here to notify the user that Windows may
                // prompt for a PIN exchange.
                return true;
            }
            else{
                // The connection cannot be upgraded so an App may offer UI here
                // to explain why a connection won't be made.
                return false;
            }
        case SocketProtectionLevel.BluetoothEncryptionWithAuthentication:
            return true;
        case SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication:
            return true;
    }
    return false;
}

// This App relies on CRC32 checking available in version 2.0 of the service.
const uint SERVICE_VERSION_ATTRIBUTE_ID = 0x0300;
const byte SERVICE_VERSION_ATTRIBUTE_TYPE = 0x0A;   // UINT32
const uint MINIMUM_SERVICE_VERSION = 200;
async Task<bool> IsCompatibleVersionAsync(RfcommDeviceService service){
    var attributes = await service.GetSdpRawAttributesAsync(
        BluetoothCacheMode.Uncached);
    var attribute = attributes[SERVICE_VERSION_ATTRIBUTE_ID];
    var reader = DataReader.FromBuffer(attribute);

    // The first byte contains the attribute's type
    byte attributeType = reader.ReadByte();
    if (attributeType == SERVICE_VERSION_ATTRIBUTE_TYPE){
        // The remainder is the data
        uint version = reader.ReadUInt32();
        return version >= MINIMUM_SERVICE_VERSION;
    }
    else return false;
}

async void Initialize(){
    // Enumerate devices with the object push service
    var services =
        await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(
            RfcommDeviceService.GetDeviceSelector(
                RfcommServiceId.ObexObjectPush));

    if (services.Count > 0){
        // Initialize target Bluetooth BR device
        var hc06Device = services.FirstOrDefault(service => service.Name == "HC-06");
        var service = await RfcommDeviceService.FromIdAsync(hc06Device.Id);


        bool isCompatibleVersion = await IsCompatibleVersionAsync(service);

        // Check that the service meets this App's minimum requirement
        if (SupportsProtection(service) && isCompatibleVersion){
            _service = service;

            // Create a socket and connect to the target
            _socket = new StreamSocket();
            await _socket.ConnectAsync(
                _service.ConnectionHostName,
                _service.ConnectionServiceName,
                SocketProtectionLevel
                    .BluetoothEncryptionAllowNullAuthentication);

            // The socket is connected. At this point the App can wait for
            // the user to take some action, for example, click a button to send a
            // file to the device, which could invoke the Picker and then
            // send the picked file. The transfer itself would use the
            // Sockets API and not the Rfcomm API, and so is omitted here for
            // brevity.
        }
    }
}

async void DataSender(int LEDNumber) {
    string strOfLED = LEDNumber.ToString();
    var dataWriter = new DataWriter(_socket.OutputStream);

    dataWriter.WriteString(strOfLED);
    await dataWriter.StoreAsync();
    await dataWriter.FlushAsync();

    dataWriter.Close();
}
#endif

    List<GameObject> SceneLights = new List<GameObject>();
    //CSVLogger myLogger = new CSVLogger();
    float timer = 0;
    bool isVirtual, MaterialOff = false;
    int SleepDuration, LEDNumber, ResetDuration;
    string saveInformation, strOfInt;
    private static string fileName = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + ".txt";

    /*Instead of having 5 different materials for the 5 lights, have 2 materials: emissive and non-emissive
     Change between them as needed. This should be faster and more efficient
    (and I can't fucking find the emission toggle so fuck it this works)
     */

    private void Start(){
        SceneLights.Add(Light1);
        SceneLights.Add(Light2);
        SceneLights.Add(Light3);
        SceneLights.Add(Light4);
        SceneLights.Add(Light5);

#if WINDOWS_UWP 
        Initialize();
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

            strOfInt = LEDNumber.ToString();
            Debug.Log("Lighting LED#" + LEDNumber + " isVirtual: " + isVirtual +
                "\nFor " + SleepDuration + " seconds, sleeping for " + ResetDuration + " seconds");
#if WINDOWS_UWP
            saveInformation = System.DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss-fff") + ", " + LEDNumber + ", " + isVirtual;
            WriteData(saveInformation);
#endif


            if (isVirtual) LightUpVirtualMaterial(SceneLights[LEDNumber], true);
            else {//this part will send the LEDNumber to the LED controller hardware, which will then turn on the said LED
#if WINDOWS_UWP
                DataSender(LEDNumber);
#endif
            }
        }
        //sleep for SleepDuration + ResetDuration seconds
        timer += Time.deltaTime;

		if (timer > SleepDuration) {//LED has been on for SleepDuration seconds
            if (!MaterialOff) {//this must happen only once
                if (isVirtual)
                    LightUpVirtualMaterial(SceneLights[LEDNumber], false);//turn off LED
                /*else {
                    //Send a turn off command to the physical controller
                    //handled on the arduino (LED's light up for 1 second)
                }*/
                
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
#if WINDOWS_UWP
	private void OnApplicationQuit() {
		_socket.Dispose();
	}
#endif
}
