using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.TrafficV3
{
    public enum IState
    {
        Red,
        Green
    }

    public enum IType
    {
        Main,
        LeftTurn,
        RightTurn
    }

    [System.Serializable]
    public class IntersectionInfo
    {
        public TrafficPathController path;
        public float u; //stop point
        public IType type;
        [HideInInspector]
        public VehicleController[] obstacles;

        private bool alreadyAdded;

        public void AddObstacles(Material mat)
        {
            if (alreadyAdded) {
                return;
            }
            Debug.Log("Adding Obstacles");
            alreadyAdded = true;
            path.AddObstacles(obstacles, true);
            SetMaterial(mat);
        }
        public void RemoveObstacles(Material mat)
        {
            alreadyAdded = false;
            path.RemoveObstacles(obstacles);
            SetMaterial(mat);
        }

        private void SetMaterial(Material mat)
        {
            foreach (var item in obstacles) {
                item.GetComponent<Renderer>().material = mat;
            }
        }
    }

    [System.Serializable]
    public class AdjacentSet
    {
        public IntersectionInfo[] intersectionInfos;
    }

    public class IntersectionController : MonoBehaviour
    {
        public int currentActiveSet;
        public IType currentType;
        public IState currentState;

        public float currentTime;

        public float greenTimeMain;
        public float greenTimeLeftTurn;
        public float redTime;

        public AdjacentSet[] sets;
        public GameObject stopBar;
        public Material greenMat;
        public Material redMat;

        private void Awake()
        {
            //construct dummy obstacles
            foreach (var item in sets) {
                foreach (var item2 in item.intersectionInfos) {
                    var obstaclesCount = item2.path.path.lanesCount;
                    var obstacles = new VehicleController[obstaclesCount];
                    for (int i=0; i<obstaclesCount;  i++) {
                        var go = GameObject.Instantiate(stopBar);
                        //go.transform.position = new Vector3(-1000, -1000, -1000);
                        go.transform.SetParent(transform);
                        var obstacle = go.GetComponent<VehicleController>();
                        obstacle.Renew(item2.u, i, 0, "obstacle");
                        obstacles[i] = obstacle;
                    }
                    item2.obstacles = obstacles;
                }
            }

            //set everything to red
            foreach (var item in sets) {
                foreach (var item2 in item.intersectionInfos) {
                    item2.AddObstacles(redMat);
                }
            }
            StartCoroutine(InitRoutine());
            currentTime = greenTimeMain;

        }

        private void Update()
        {
            if (currentTime <= 0) {
                if (currentType == IType.LeftTurn) {
                    currentType = IType.Main;
                    currentTime = greenTimeMain;
                } else {
                    currentType = IType.LeftTurn;
                    currentTime = greenTimeLeftTurn;
                }
                UpdateGreenLight(currentActiveSet, currentType);
                if (currentType == IType.LeftTurn) {
                    currentActiveSet++;
                    currentActiveSet %= sets.Length;
                }
            }
            currentTime -= Time.deltaTime;


        }

        private void UpdateGreenLight(int activeSet, IType type, bool debug=false)
        {
            if (!debug) {
                //set everything to red
                foreach (var item in sets) {
                    foreach (var item2 in item.intersectionInfos) {
                        item2.AddObstacles(redMat);
                    }
                }
            }

            //set current set to green
            var set = sets[activeSet];
            foreach (var item in set.intersectionInfos) {
                if (item.type == type) {
                    item.RemoveObstacles(greenMat);
                }
            }

        }

        private void UpdateRightTurn()
        {

        }

        private IEnumerator InitRoutine()
        {
            yield return null; //skip to next frame
            UpdateGreenLight(currentActiveSet, currentType);
        }


    }

    
}