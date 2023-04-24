using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UXF;
using System.Linq;

/// <summary>
/// Attach this component to any gameobject (e.g. an empty one) and assign it in the trackedObjects field in an ExperimentSession to record
/// if ray casted from camera is hitting anything. NOTE: Update Type must be set to MANUAL. 
/// Please provide coordinates in form of lists named ray_x & ray_y in your .json file. For example \n\"ray_x\": [0.5],\n\"ray_y\": [0.5]\nif you want to have one ray in the middle of the screen. Multiple rays can be provide with this method."
/// </summary>
public class ScreenRayTracker : Tracker 
{
	// Public vars
    [Header("Necessary Input")]
    public Camera cam;
    public Session session;

    [Header("Optional Input")]
    [Tooltip("Enable if you want to visualise the rays in the scene view and the console output.")]
    public bool debugMode = true; 
    [Tooltip("The max distance the ray should check for collisions. For further information see manual of Physics.Raycast.")]
    public float distance = Mathf.Infinity;

    [Tooltip("Set to true if you want to use a LayerMask for the rays (see maunual of LayerMask.GetMask). Note you also need to set Layer Mask Names in this case with one or more layers that you want to use.")]
    public bool useLayerMask = false;
    [Tooltip("Provide the names of the layers for the mask.")]
    public string[] layerMaskNames;

    // Private vars
    private List<string> objectDetected = new List<string>();
    private UXFDataRow currentRow;
    private bool recording = false;
    private int layerMask;
    private string noObjectString = "NA";
    private int numRays;
    private List<float> x  = new List<float>();
    private List<float> y  = new List<float>();

    // Start calc
    void Start(){
        // Create layer mask 
        if(useLayerMask)
        {
            layerMask = LayerMask.GetMask(layerMaskNames);
        } else 
        {
            layerMask = ~0; // Set to everything as no mask is wanted. 
        }

    	// Start the recoding
        StartCoroutine(RecordRoutine());
    }

    /// <summary>
    /// Gets coordinates for the rays in screen space from .json file and prints screen resolution
    /// </summary>
    public void GetRayCoordinates()
    {
        // Coordinates
        x = session.settings.GetFloatList("ray_x");
        y = session.settings.GetFloatList("ray_y");

        // Screen resolution
        Debug.Log(Screen.currentResolution);
    }

    /// <summary>
    /// Starts the recording. This method needs to be added to [UXF_Rig] events called On Trial Begin
    /// </summary>
    public void StartRecording()
    {
    	recording = true;
    }

    /// <summary>
    /// Stops the recording. This method needs to be added to [UXF_Rig] events called On Trial End
    /// </summary>
    public void StopRecording()
    {
    	recording = false;
    }

    IEnumerator RecordRoutine()
    {
        while (true){
            if (recording){
                objectDetected = Ray2DetectObjects(x, y, cam);
                for(int i = 0; i < numRays; i++){
                    // When no object was detected save only if saveNoObject is true
                    if(objectDetected[i] != noObjectString)
                    {
                        var values = new UXFDataRow();
                        values.Add(("rayIndex", i));
                        values.Add(("x", x[i]));
                        values.Add(("y", y[i]));
                        values.Add(("objectDetected", objectDetected[i]));
                        currentRow = values;
                        RecordRow(); // record for each ray
                        currentRow = null;

                    } 
                }
            }
            yield return null; // wait until next frame
        }
    }

    /// <summary>
    /// Set headers and measurment descriptor
    /// </summary>
    public override string MeasurementDescriptor => "ObjectsOnScreenTracker";
    public override IEnumerable<string> CustomHeader => new string[] {"rayIndex", "x", "y", "objectDetected"};

    protected override UXFDataRow GetCurrentValues()
    {
        return currentRow;
    }

    /// <summary>
    /// Function that casts the rays and detects the hits. 
    /// </summary>
    List<string> Ray2DetectObjects(List<float> x, List<float> y, Camera cam)
    {
    	// Get number of rays
    	numRays = y.Count;

    	// Create var to reset the variable
    	List<string> nameOfObjects = new List<string>();

    	for (int i = 0; i < numRays; i++){
    		// Cast the ray and add to list
    		Ray ray = cam.ViewportPointToRay(new Vector3(x[i], y[i], 0));

    		// Display ray for debugging
            if(debugMode){
            	Debug.DrawRay(ray.origin, ray.direction * 50, Color.red);
            }

            // Raycast and check if something is hit
            RaycastHit hit1;
            if (Physics.Raycast(ray, out hit1, distance, layerMask)){
            	if(debugMode){
            		Debug.DrawRay(ray.origin, ray.direction * 50, Color.green);
            		Debug.Log("I'm looking at " + hit1.transform.name + " with ray " + i);
            	}
            	// Add name of GameObject that was hit
                nameOfObjects.Add(hit1.transform.name);
            } else {
            	// Add noObjectString becuase no object was hit by ray
            	nameOfObjects.Add(noObjectString);
            }
        }
    	return nameOfObjects;
    }
}
