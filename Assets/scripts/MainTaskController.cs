using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeProperties {
    public Transform CubeTransform;
    public bool isReached;
    public int TargetIndex;

    public CubeProperties(Transform mainObj) {
        this.CubeTransform = mainObj;
        this.isReached = true;
        this.TargetIndex = -1;
    }
}

public class MainTaskController : MonoBehaviour{
    [SerializeField] GameObject CubesParent, MovementLocParent;
    [SerializeField] [Range(0.0f, 1.0f)] float Velocity;

    List<CubeProperties> CubesStructList = new List<CubeProperties>();
    Transform[] CubeTargets;
    List<int> UsedTargetIndex = new List<int>();
    int randomIndex;

    /* Get all the children of MovementLocParent, these are target locations
     * Do the same to the cubes
     * 
     * Make it so the cubes target a random location and move to it
     * they must not pick the same location!!!
     * they must move to it until they reach it!!!
     * * create a custom struct that has: cube, isReached, targetLoc
     * 
     * Maybe move the cubes to the locations in scene view as well
     */

    void Start(){
        foreach (Transform childCube in CubesParent.transform) {//only gets children
            CubeProperties myCube = new CubeProperties(childCube);
            CubesStructList.Add(myCube);
        }

        CubeTargets = MovementLocParent.transform.GetComponentsInChildren<Transform>();//also gets parent
    }

    void Update(){
        //move cubes randomly among CubeTargets
        foreach(CubeProperties cube in CubesStructList) {
            //select new unused target if necessary
            if (cube.isReached) {
                do {
                    randomIndex = UnityEngine.Random.Range(1, CubeTargets.Length);
                } while (UsedTargetIndex.Contains(randomIndex));
                UsedTargetIndex.Add(randomIndex);

                cube.TargetIndex = randomIndex;
                cube.isReached = false;
            }

            //move currently selected object to target
            cube.CubeTransform.position = Vector3.MoveTowards(cube.CubeTransform.position, CubeTargets[cube.TargetIndex].position, Velocity * Time.deltaTime);

            //if target is reached, mark the cube and clean up the target
            if (cube.CubeTransform.position == CubeTargets[cube.TargetIndex].position) {
                cube.isReached = true;
                UsedTargetIndex.Remove(cube.TargetIndex);
            }
        }
    }
}
