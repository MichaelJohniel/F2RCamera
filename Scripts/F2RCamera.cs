using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class F2RCamera : MonoBehaviour
{
    //Toggles
    private bool playerInput;
    private bool sprint;
    Sprite sprintInd;
    Sprite waypointInd;
    //Rotation
    private float inputRotationX;
    private float inputRotationY;
    private float cameraRotationX;
    private float cameraRotationY;
    //Settings
    private float accelleration = .1f;
    private float maxMoveSpeed = 1.5f;
    private float mouseSens = 0.25f;
    private float increment = 0.05f;
    private float accellerationMax = 0.4f;
    private float mouseSensitivityMax = 1f;
    [Range(3f, 12f)]
    public float moveSpeedMax = 3f;
    //Speed
    private float currentMoveSpeed = 0f;
    private float currentStrafeSpeed = 0f;
    private float currentLiftSpeed = 0f;
    private float sprintSpeed;
    //Compass
    private int direction;
    [Range(0, 360)]
    public int north = 0;
    int vertical = 0;
    int horizontal = 0;
    //Waypoints
    private int waypointRadius = 50;
    private float markerRadius = 1500;
    private Vector3 waypointOnePos;
    private Vector3 waypointTwoPos;
    private Vector3 waypointThreePos;
    private GameObject waypointOne;
    private GameObject waypointTwo;
    private GameObject waypointThree;
    private Gradient red;
    private Gradient green;
    private Gradient blue;
    private float redAlpha;
    private float greenAlpha;
    private float blueAlpha;
    //Input
    private KeyCode inputForward = KeyCode.W;
    private KeyCode inputBackward = KeyCode.S;
    private KeyCode inputLeft = KeyCode.A;
    private KeyCode inputRight = KeyCode.D;
    private KeyCode inputUp = KeyCode.E;
    private KeyCode inputDown = KeyCode.Q;

    //GameObjects
    private GameObject Camera;
    private GameObject GUI;

    // Use this for initialization
    private void Start()
    {
        //Set the Main Camera
        F2RCamera mainCamera = FindObjectOfType<F2RCamera>();
        Camera = mainCamera.gameObject;

        //GUI
        if (GameObject.Find("GUI") == null)
        {
            GUI = Instantiate((GameObject)AssetDatabase.LoadAssetAtPath("Assets/F2RCamera/Resources/GUI.prefab", typeof(GameObject)));
            GUI.name = "GUI";
            GUI.transform.parent = Camera.transform;
        }


        //Default settings
        playerInput = true;
        sprint = false;

        //Lock Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //Set Waypoint Indicators
        waypointOne = GameObject.Find("WaypointOne");
        waypointTwo = GameObject.Find("WaypointTwo");
        waypointThree = GameObject.Find("WaypointThree");

        //Waypoint gradients
        redAlpha = 0f;
        greenAlpha = 0f;
        blueAlpha = 0f;
        red = new Gradient();
        green = new Gradient();
        blue = new Gradient();
    }

    // Update is called once per frame
    private void Update()
    {
        //Toggle Controls
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
            playerInput = !playerInput;

        //Allow or Disallow Player Input
        if (playerInput == true)
        {
            //Lock Cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            //Reset to default values
            if (Input.GetKeyDown(KeyCode.O))
                ResetChanges();

            CameraSettings();
            MoveCamera();
            RotateCamera();

            if (GameObject.Find("GUI") != null)
            {
                Compass();
                Waypoints();
            }
            
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


    //Features
    private void CameraSettings()
    {
        //Adjust Accelleration
        if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftAlt))
        {
            if (Input.mouseScrollDelta.y > .01f)
                accelleration += increment;
            if (Input.mouseScrollDelta.y < -.01f)
                accelleration -= increment;
        }
        //Adjust Max Movement Speed
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.mouseScrollDelta.y > .01f)
                maxMoveSpeed += increment;
            if (Input.mouseScrollDelta.y < -.01f)
                maxMoveSpeed -= increment;
        }  
        //Adjust Rotation Sensitivity
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            if (Input.mouseScrollDelta.y > .01f)
                mouseSens += increment;
            if (Input.mouseScrollDelta.y < -.01f)
                mouseSens -= increment;
        }

        //Clamp Settings
        accelleration = Mathf.Clamp(accelleration, .1f, accellerationMax);
        maxMoveSpeed = Mathf.Clamp(maxMoveSpeed, .05f, moveSpeedMax);
        mouseSens = Mathf.Clamp(mouseSens, .1f, mouseSensitivityMax);

    }

    private void MoveCamera()
    {
        //Sprint Indicator
        if (GameObject.Find("GUI") != null)
        {
            GameObject sprintToggle = GameObject.Find("SprintToggle");
            Image sprintToggleInd = sprintToggle.GetComponent<Image>();
            if (sprint)
                sprintInd = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/F2RCamera/Resources/Images/Sprint.png", typeof(Sprite));
            else
                sprintInd = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/F2RCamera/Resources/Images/Walk.png", typeof(Sprite));
            sprintToggleInd.sprite = sprintInd;
        }

        //Toggle Sprint
        if (Input.GetKeyDown(KeyCode.LeftShift))
            sprint = !sprint;
        sprintSpeed = maxMoveSpeed * 2;

        //Move
        if (Input.GetKey(inputForward) && !Input.GetKey(inputBackward))
            Accelleration(currentMoveSpeed, inputForward, true, sprint);
        if (Input.GetKey(inputBackward) && !Input.GetKey(inputForward))
            Accelleration(currentMoveSpeed, inputBackward, false, sprint);
        //Strafe
        if (Input.GetKey(inputRight) && !Input.GetKey(inputLeft))
            Accelleration(currentStrafeSpeed, inputRight, true, false);
        if (Input.GetKey(inputLeft) && !Input.GetKey(inputRight))
            Accelleration(currentStrafeSpeed, inputLeft, false, false);
        //Lift
        if (Input.GetKey(inputUp) && !Input.GetKey(inputDown))
            Accelleration(currentLiftSpeed, inputUp, true, false);
        if (Input.GetKey(inputDown) && !Input.GetKey(inputUp))
            Accelleration(currentLiftSpeed, inputDown, false, false);

        //Stop Moving
        if ((Input.GetKey(inputForward) && Input.GetKey(inputBackward)) || (!Input.GetKey(inputForward) && !Input.GetKey(inputBackward)))
            Deccelleration(currentMoveSpeed, inputForward);
        if ((Input.GetKey(inputLeft) && Input.GetKey(inputRight)) || (!Input.GetKey(inputLeft) && !Input.GetKey(inputRight)))
            Deccelleration(currentStrafeSpeed, inputRight);
        if ((Input.GetKey(inputUp) && Input.GetKey(inputDown)) || (!Input.GetKey(inputUp) && !Input.GetKey(inputDown)))
            Deccelleration(currentLiftSpeed, inputDown);

        //Clamp
        if (!sprint)
            currentMoveSpeed = Mathf.Clamp(currentMoveSpeed, -maxMoveSpeed, maxMoveSpeed);
        else
            currentMoveSpeed = Mathf.Clamp(currentMoveSpeed, -sprintSpeed, sprintSpeed);
        currentStrafeSpeed = Mathf.Clamp(currentStrafeSpeed, -maxMoveSpeed, maxMoveSpeed);
        currentLiftSpeed = Mathf.Clamp(currentLiftSpeed, -maxMoveSpeed, maxMoveSpeed);

        //Apply Camera Movement
        transform.position += transform.forward * currentMoveSpeed * 100f * Time.deltaTime;
        transform.position += transform.right * currentStrafeSpeed * 100f * Time.deltaTime;
        transform.position += transform.up * currentLiftSpeed * 100f * Time.deltaTime;
    }

    private void RotateCamera()
    {
        //X Rotation
        inputRotationX += Input.GetAxis("Mouse X") * 150f * mouseSens * Time.deltaTime;
        cameraRotationX = Mathf.Lerp(cameraRotationX, inputRotationX, .5f);

        //Y Rotation
        inputRotationY -= Input.GetAxis("Mouse Y") * 100f * mouseSens * Time.deltaTime;
        inputRotationY = Mathf.Clamp(inputRotationY, -90f, 90f);
        cameraRotationY = Mathf.Lerp(cameraRotationY, inputRotationY, .5f);
        
        //Apply Rotation
        transform.localRotation = Quaternion.Euler(cameraRotationY, cameraRotationX, 0f);
    }

    private void Waypoints()
    {
        //Update Waypoints
        if (Input.GetKeyDown(KeyCode.Alpha1))
            EditWaypoint(waypointOnePos, KeyCode.Alpha1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            EditWaypoint(waypointTwoPos, KeyCode.Alpha2);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            EditWaypoint(waypointThreePos, KeyCode.Alpha3);

        red.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.red, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(redAlpha, 0.0f), new GradientAlphaKey(redAlpha, 1.0f) }
        );
        green.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.green, 0.0f), new GradientColorKey(Color.green, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(greenAlpha, 0.0f), new GradientAlphaKey(greenAlpha, 1.0f) }
        );
        blue.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(Color.blue, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(blueAlpha, 0.0f), new GradientAlphaKey(blueAlpha, 1.0f) }
        );

        //Create Waypoint Markers
        if (waypointOnePos != Vector3.zero)
        {
            if (GameObject.Find("MarkerOne") == null)
                DrawLine(waypointOnePos, red, 1);
            else
            {
                float distance;
                distance = Mathf.Sqrt(Mathf.Abs((waypointOnePos.x - transform.position.x) * (waypointOnePos.x - transform.position.x)) + ((waypointOnePos.z - transform.position.z) * (waypointOnePos.z - transform.position.z)));
                if (distance / markerRadius < .01)
                    redAlpha = 0;
                else
                    redAlpha = distance / markerRadius;
                redAlpha = Mathf.Clamp(redAlpha, 0f, .8f);
                GameObject Marker = GameObject.Find("MarkerOne");
                LineRenderer marker = Marker.GetComponent<LineRenderer>();
                marker.colorGradient = red;
            }
            
        }
        else if (GameObject.Find("MarkerOne"))
            GameObject.Destroy(GameObject.Find("MarkerOne"));

        if (waypointTwoPos != Vector3.zero)
        {
            if (GameObject.Find("MarkerTwo") == null)
                DrawLine(waypointTwoPos, green, 2);
            else
            {
                float distance;
                distance = Mathf.Sqrt(Mathf.Abs((waypointTwoPos.x - transform.position.x) * (waypointTwoPos.x - transform.position.x)) + ((waypointTwoPos.z - transform.position.z) * (waypointTwoPos.z - transform.position.z)));
                if (distance / markerRadius < .01)
                    greenAlpha = 0;
                else
                    greenAlpha = distance / markerRadius;
                greenAlpha = Mathf.Clamp(greenAlpha, 0f, .8f);
                GameObject Marker = GameObject.Find("MarkerTwo");
                LineRenderer marker = Marker.GetComponent<LineRenderer>();
                marker.colorGradient = green;
            }
        }
        else if (GameObject.Find("MarkerTwo"))
            GameObject.Destroy(GameObject.Find("MarkerTwo"));

        if (waypointThreePos != Vector3.zero)
        {
            if (GameObject.Find("MarkerThree") == null)
                DrawLine(waypointThreePos, blue, 3);
            else
            {
                float distance;
                distance = Mathf.Sqrt(Mathf.Abs((waypointThreePos.x - transform.position.x) * (waypointThreePos.x - transform.position.x)) + ((waypointThreePos.z - transform.position.z) * (waypointThreePos.z - transform.position.z)));
                if (distance / markerRadius < .01)
                    blueAlpha = 0;
                else
                    blueAlpha = distance / markerRadius;
                blueAlpha = Mathf.Clamp(blueAlpha, 0f, .8f);
                GameObject Marker = GameObject.Find("MarkerThree");
                LineRenderer marker = Marker.GetComponent<LineRenderer>();
                marker.colorGradient = blue;
            }
        }
        else if (GameObject.Find("MarkerThree"))
            GameObject.Destroy(GameObject.Find("MarkerThree"));

    }

    private void Compass()
    {
        //Variables
        string[] compass = { "North", "NorthEast", "East", "SouthEast", "South", "SouthWest", "West", "NorthWest" };
        

        //Compass GUI
        GameObject Compass = GameObject.Find("Compass");
        Text compassDirection = Compass.GetComponent<Text>();
        compassDirection.text = compass[direction];

        //Set Compass Direction
        float cameraDirection = transform.localEulerAngles.y;

        //Set Directions relative to North
        int east = north + 90;
        if (east > 359)
            east -= 360;
        int south = north + 180;
        if (south > 359)
            south -= 360;
        int west = north + 270;
        if (west > 359)
            west -= 360;

        //Check if facing North
        if (north - 68 < 0)
        {
            if (cameraDirection > (north - 68 + 360) || cameraDirection < north + 68)
                vertical = 1;
            else
                vertical = 0;
        }
        else if (north + 68 > 359)
        {
            if (cameraDirection < (north + 68 - 360) || cameraDirection > north - 68)
                vertical = 1;
            else
                vertical = 0;
        }
        else if (north + 68 < 360 && north - 68 > 0)
        {
            if (cameraDirection > north - 68 && cameraDirection < north + 68)
                vertical = 1;
            else
                vertical = 0;
        }
        //Check if facing South
        if (south - 68 < 0)
        {
            if (cameraDirection > (south - 68 + 360) || cameraDirection < south + 68)
                vertical = -1;
        }
        else if (south + 68 > 359)
        {
            if (cameraDirection < (south + 68 - 360) || cameraDirection > south - 68)
                vertical = -1;
        }
        else if (south + 68 < 360 && south - 68 > 0)
        {
            if (cameraDirection > south - 68 && cameraDirection < south + 68)
                vertical = -1;
        }

        //Check if facing East
        if (east - 68 < 0)
        {
            if (cameraDirection > (east - 68 + 360) || cameraDirection < east + 68)
                horizontal = 1;
            else
                horizontal = 0;
        }
        else if (east + 68 > 359)
        {
            if (cameraDirection < (east + 68 - 360) || cameraDirection > east - 68)
                horizontal = 1;
            else
                horizontal = 0;
        }
        else if (east + 68 < 360 && east - 45 > 0)
        {
            if (cameraDirection > east - 68 && cameraDirection < east + 68)
                horizontal = 1;
            else
                horizontal = 0;
        }
        //Check if facing West
        if (west - 68 < 0)
        {
            if (cameraDirection > (west - 68 + 360) || cameraDirection < west + 68)
                horizontal = -1;
        }
        else if (west + 68 > 359)
        {
            if (cameraDirection < (west + 68 - 360) || cameraDirection > west - 68)
                horizontal = -1;
        }
        else if (west + 68 < 360 && west - 68 > 0)
        {
            if (cameraDirection > west - 68 && cameraDirection < west + 68)
                horizontal = -1;
        }

        //North
        if (vertical == 1 && horizontal == 0)
            direction = 0;
        //NorthEast
        if (vertical == 1 && horizontal == 1)
            direction = 1;
        //East
        if (vertical == 0 && horizontal == 1)
            direction = 2;
        //SouthEast
        if (vertical == -1 && horizontal == 1)
            direction = 3;
        //South
        if (vertical == -1 && horizontal == 0)
            direction = 4;
        //SouthWest
        if (vertical == -1 && horizontal == -1)
            direction = 5;
        //West
        if (vertical == 0 && horizontal == -1)
            direction = 6;
        //NorthWest
        if (vertical == 1 && horizontal == -1)
            direction = 7;
        

        Debug.Log(vertical);
        Debug.Log(horizontal);


    }

    //Repetitive Code
    private void EditWaypoint ( Vector3 waypoint, KeyCode keyCode)
    {
        //Set the Waypoint Indicator to adjust
        Image waypointToggle = waypointOne.GetComponent<Image>();

        if (keyCode == KeyCode.Alpha1)
        {
            waypointToggle = waypointOne.GetComponent<Image>();
        }
        if (keyCode == KeyCode.Alpha2)
        {
            waypointToggle = waypointTwo.GetComponent<Image>();
        }
        if (keyCode == KeyCode.Alpha3)
        {
            waypointToggle = waypointThree.GetComponent<Image>();
        }

        //Check Waypoint Status
        if (waypoint == Vector3.zero)
        {
            waypoint = transform.position;
            waypointInd = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/F2RCamera/Resources/Images/WaypointSet.png", typeof(Sprite));
        }
        else if ((transform.position.x < waypoint.x + waypointRadius && transform.position.x > waypoint.x - waypointRadius) && (transform.position.z < waypoint.z + waypointRadius && transform.position.z > waypoint.z - waypointRadius))
        {
            waypoint = Vector3.zero;
            waypointInd = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/F2RCamera/Resources/Images/WaypointOpen.png", typeof(Sprite));
        }
        else
        {
            transform.position = waypoint;
            waypointInd = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/F2RCamera/Resources/Images/WaypointSet.png", typeof(Sprite));
        }

        //Update Waypoint Position
        if (keyCode == KeyCode.Alpha1)
            waypointOnePos = waypoint;
        if (keyCode == KeyCode.Alpha2)
            waypointTwoPos = waypoint;
        if (keyCode == KeyCode.Alpha3)
            waypointThreePos = waypoint;

        //Update Indicator
        waypointToggle.sprite = waypointInd;
    }

    private void DrawLine( Vector3 waypoint, Gradient color, int markerNum)
    {
        //Create Marker
        GameObject Marker = new GameObject();

        if (markerNum == 1)
            Marker.gameObject.name = "MarkerOne";
        if (markerNum == 2)
            Marker.gameObject.name = "MarkerTwo";
        if (markerNum == 3)
            Marker.gameObject.name = "MarkerThree";

        Marker.AddComponent<LineRenderer>();
        LineRenderer marker = Marker.GetComponent<LineRenderer>();
        marker.material = (Material)AssetDatabase.LoadAssetAtPath("Assets/F2RCamera/Resources/WaypointMaterial.mat", typeof(Material));
        marker.startWidth = 38;
        marker.endWidth = 20;
        marker.colorGradient = color;
        marker.SetPosition(0, new Vector3(waypoint.x, waypoint.y - 5000, waypoint.z));
        marker.SetPosition(1, new Vector3(waypoint.x, waypoint.y + 5000, waypoint.z));
    }

    private void Accelleration (float speed, KeyCode keyCode, bool positive, bool sprint)
    {
        if (!sprint)
        {
            if (positive)
            {
                if (speed < maxMoveSpeed)
                    speed = Mathf.Lerp(speed, maxMoveSpeed, accelleration);
                if (speed > maxMoveSpeed - .05f)
                    speed = maxMoveSpeed;
            }
            else
            {
                if (speed > -maxMoveSpeed)
                    speed = Mathf.Lerp(speed, -maxMoveSpeed, accelleration);
                if (speed < -maxMoveSpeed + .05f)
                    speed = -maxMoveSpeed;
            }
        }
        else
        {
            if (positive)
            {
                if (speed < sprintSpeed)
                    speed = Mathf.Lerp(speed, sprintSpeed, accelleration);
                if (speed > sprintSpeed - .05f)
                    speed = sprintSpeed;
            }
            else
            {
                if (speed > -sprintSpeed)
                    speed = Mathf.Lerp(speed, -sprintSpeed, accelleration);
                if (speed < -sprintSpeed + .05f)
                    speed = -sprintSpeed;
            }
        }

        if (keyCode == inputForward || keyCode == inputBackward)
            currentMoveSpeed = speed;
        if (keyCode == inputLeft || keyCode == inputRight)
            currentStrafeSpeed = speed;
        if (keyCode == inputUp || keyCode == inputDown)
            currentLiftSpeed = speed;
    }

    private void Deccelleration (float speed, KeyCode keyCode)
    {
        if (speed != 0f)
            speed = Mathf.Lerp(speed, 0f, accelleration);
        if (speed < .01 && speed > -.01)
            speed = 0;

        if (keyCode == inputForward || keyCode == inputBackward)
            currentMoveSpeed = speed;
        if (keyCode == inputLeft || keyCode == inputRight)
            currentStrafeSpeed = speed;
        if (keyCode == inputUp || keyCode == inputDown)
            currentLiftSpeed = speed;
    }

    private void ResetChanges ()
    {
        accelleration = .1f;
        mouseSens = .25f;
        maxMoveSpeed = .5f;
        accellerationMax = 0.4f;
        mouseSensitivityMax = 0.8f;
        moveSpeedMax = 3f;
    }
}