using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TrackedImageAugmentation : MonoBehaviour
{

    //  Reference to AR tracked image manager
     private ARTrackedImageManager _trackedImagesManager;

    /*
     * List of prefabs to instantiate : named the same ast
     * their corresponding 2D images in Reference Image Library
     */
    public GameObject[] ARPrefabs;

    // Keep dictionary of created prefabs
    private readonly Dictionary<string, GameObject> _instantiatedPrefabs = new Dictionary<string, GameObject>();

    private void Awake()
    {
        // Cache a reference to the Tracked Image Manager Object
        _trackedImagesManager = GetComponent<ARTrackedImageManager>();
    }

    private void OnEnable()
    {
        //Attach event handler when tracked images change
        _trackedImagesManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    { 
        // Detach event handler
        _trackedImagesManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Loop through all tracked images that have been detected
        foreach (var trackedImage in eventArgs.added)
        {
            //get the name of reference image 
            var imageName = trackedImage.referenceImage.name;
            // loop over the array of prefabs
            foreach(var curPrefab in ARPrefabs) 
            {
                /*
                 * Check whether this particular prefab matches the tracked image name
                 * and that the prefab hasn't already been created
                 */
                if(string.Compare(curPrefab.name,imageName,StringComparison.OrdinalIgnoreCase) == 0 
                    && !_instantiatedPrefabs.ContainsKey(imageName))
                {
                    //Instantiate the prefab, parenting it to the ARTrackedImage
                    var newPrefab = Instantiate(curPrefab, trackedImage.transform);
                    //Add created prefab to prefab dictionary
                    _instantiatedPrefabs[imageName] = newPrefab;
                }
            }
        }
        /*
         * For all prefabs that have been created so far, set them active or not
         * depending on whether their corresponding image is currently being tracked
         */
        foreach(var trackedImage in eventArgs.updated)
        {
            _instantiatedPrefabs[trackedImage.referenceImage.name]
                .SetActive(trackedImage.trackingState == TrackingState.Tracking);
        }

        // If the AR subsystem has given up looking for a tracked image
        foreach(var trackedImage in eventArgs.removed) 
        {
            //Destroy its prefab
            Destroy(_instantiatedPrefabs[trackedImage.referenceImage.name]);
            //and its instance from dictionary
            _instantiatedPrefabs.Remove(trackedImage.referenceImage.name);
            /* 
             * Alternatively:
             * Set prefab instance state to inactive
             * _instantiatedPrefabs[trackedImage.referenceImage.name].SetActive(false);
             */

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
