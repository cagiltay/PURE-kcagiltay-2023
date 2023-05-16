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
    [SerializeField] GameObject CubesParent, MovementLocParent, PassedObject;
    [SerializeField] [Range(0.0f, 1.0f)] float Velocity;

    List<CubeProperties> CubesStructList = new List<CubeProperties>();
    CubeProperties PassedBall;
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
        PassedBall = new CubeProperties(PassedObject.transform);
    }

    void Update(){
        //move cubes randomly among CubeTargets
        foreach(CubeProperties cube in CubesStructList) {
            //select new unused target if necessary
            if (cube.isReached) {
                do {
                    //CubeTargets[0] == parent at (0,0,1)
                    randomIndex = UnityEngine.Random.Range(2, CubeTargets.Length);
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

		#region Same as above but scuffed for the ball
		//move red ball (PassedObject) towards one of the cubes
		//red ball must move towards a cube until it reaches it. When it reaches cube, it picks a new cube
		//pick a cube for the ball if needed
		if (PassedBall.isReached) {
            do {
                randomIndex = UnityEngine.Random.Range(0, CubesStructList.Count);
            } while (PassedBall.TargetIndex == randomIndex);

            PassedBall.TargetIndex = randomIndex;
            PassedBall.isReached = false;
        }
        //move currently selected object to target (10% faster than the cubes)
        PassedBall.CubeTransform.position = Vector3.MoveTowards(PassedBall.CubeTransform.position, CubesStructList[PassedBall.TargetIndex].CubeTransform.position, Velocity * 1.1f * Time.deltaTime);
        //if target is reached, mark the cube and clean up the target
        if (Vector3.Distance(PassedBall.CubeTransform.position, CubesStructList[PassedBall.TargetIndex].CubeTransform.position) < 0.02)
            PassedBall.isReached = true;
        #endregion
    }
}
