using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Wikitude;

using Plane = UnityEngine.Plane;

public class InstantTrackingController : SampleController
{
    /* The GameObject that contains the all  buttons. */
    public GameObject ButtonDock;
    /* The GameObject that contains the UI elements used to initialize instant tracking. */
    public GameObject InitializationControls;

    /* Button that initializes the tracking state. */
    public Button InitializeButton;

    /* The label indicating the current DeviceHeightAboveGround. */
    public Text HeightLabel;

    public InstantTracker Tracker;

    public Button ResetButton;
    public Button StartSeekButton;
    /* The order in theses arrays indicate which button corresponds to which model. */
    public List<Button> Buttons;
    public List<GameObject> Models;

    public Text MessageBox;
    public GameObject MessageHandler;
    public GameObject EggsPanel;
    /* Status bar at the bottom of the screen, indicating if the scene is being tracked or not. */
    public Image ActivityIndicator;

    /* The colors of the bottom status status bar */
    public Color EnabledColor = new Color(0.2f, 0.75f, 0.2f, 0.8f);
    public Color DisabledColor = new Color(1.0f, 0.2f, 0.2f, 0.8f);

    /* Controller that moves the objects based on user input. */
    private MoveController _moveController;

    /* Renders the grid used when initializing the tracker, indicating the ground plane. */
    private GridRenderer _gridRenderer;
    private ScaleController _scaleController;
    private TrackingDistanceToObjectController _distanceController;
    private AudioDistanceIndicator _audioController;
    private PanelGameController _panelController;
    /* The currently rendered augmentations. */
    private HashSet<GameObject> _activeModels = new HashSet<GameObject>();
    /* The state in which the tracker currently is. */
    private InstantTrackingState _currentState = InstantTrackingState.Initializing;
    public InstantTrackingState CurrentState {
        get { return _currentState; }
    }
    private bool _isTracking = false;
    public HashSet<GameObject> ActiveModels {
        get {
            return _activeModels;
        }
    }
    public void RemoveObject(GameObject obj){
        _activeModels.Remove(obj);
    }

    protected override void Awake() {
        base.Awake();
        
        Application.targetFrameRate = 60;

        _moveController = GetComponent<MoveController>();
        _gridRenderer = GetComponent<GridRenderer>();
        _distanceController = GetComponent<TrackingDistanceToObjectController>();
        _scaleController = GetComponent<ScaleController>();
        _audioController = GetComponent<AudioDistanceIndicator>();
        _panelController = GetComponent<PanelGameController>();

    }

    protected override void Start() {
        base.Start();
        QualitySettings.shadowDistance = 4.0f;

        MessageBox.text = "Starting the SDK";
        /* The Wikitude SDK needs to be fully started before we can query for platform assisted tracking support
         * SDK initialization happens during start, so we wait one frame in a coroutine
         */
        StartCoroutine(CheckPlatformAssistedTrackingSupport());
    }

    private IEnumerator CheckPlatformAssistedTrackingSupport() {
        yield return null;
        if (Tracker.SMARTEnabled) {
            Tracker.IsPlatformAssistedTrackingSupported((SmartAvailability smartAvailability) => {
                UpdateTrackingMessage(smartAvailability);
            });
        }
    }

    private void UpdateTrackingMessage(SmartAvailability smartAvailability) {
        if (Tracker.SMARTEnabled) {
            string sdk;
            if (Application.platform == RuntimePlatform.Android) {
                sdk = "ARCore";
            } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
                sdk = "ARKit";
            } else {
                MessageBox.text = "Running without platform assisted tracking support.";
                return;
            }

            switch (smartAvailability) {
                case SmartAvailability.IndeterminateQueryFailed: {
                    MessageBox.text = "Platform support query failed. Running without platform assisted tracking support.";
                    break;
                }
                case SmartAvailability.CheckingQueryOngoing: {
                    MessageBox.text = "Platform support query ongoing.";
                    break;
                }
                case SmartAvailability.Unsupported: {
                    MessageBox.text = "Running without platform assisted tracking support.";
                    break;
                }
                case SmartAvailability.SupportedUpdateRequired:
                case SmartAvailability.Supported: {
                    string runningWithMessage = "Running with platform assisted tracking support (" + sdk + ").";

                    if (_currentState == InstantTrackingState.Tracking) {
                        MessageBox.text = runningWithMessage;
                    } else {
                        MessageBox.text = runningWithMessage + "\n Move your phone around until the target turns green, which is when you can start tracking.";
                    }
                    break;
                }
            }
        } else {
            MessageBox.text = "Running without platform assisted tracking support.";
        }
    }

    protected override void Update() {
        base.Update();
        if (_currentState == InstantTrackingState.Initializing) {
            /* Change the color of the grid to indicate if tracking can be started or not. */
            if (Tracker.CanStartTracking()) {
                _gridRenderer.TargetColor = Color.green;
                if(InitializeButton != null) {
                    InitializeButton.gameObject.SetActive(true);
                }
            } else {
                _gridRenderer.TargetColor = GridRenderer.DefaultTargetColor;
                if(InitializeButton != null) {
                   InitializeButton.gameObject.SetActive(false);
                }
            }
        } else {
            _gridRenderer.TargetColor = GridRenderer.DefaultTargetColor;
        }

        if(_isTracking){
            if(_activeModels.Count >= 9){
                Buttons[0].gameObject.SetActive(false);
            }
        }
    }

    #region UI Events
    public void OnInitializeButtonClicked()
    {
        Tracker.SetState(InstantTrackingState.Tracking);
    }


    public void OnStartSeekButtonClikced() {
        _gridRenderer.enabled = false;
        _moveController.enabled = false;
        _scaleController.enabled = false;
        ButtonDock.SetActive(false);
        ResetButton.gameObject.SetActive(false);
        StartSeekButton.gameObject.SetActive(false);
        MessageHandler.SetActive(false);
        _distanceController.enabled = true;
        _audioController.enabled = true;
        _panelController.enabled = true;

    }

    public void OnHeightValueChanged(float newHeightValue) {
        HeightLabel.text = string.Format("{0:0.##} m", newHeightValue);
        Tracker.DeviceHeightAboveGround = newHeightValue;
    }

    public void OnBeginDrag (int modelIndex) {
        if (_isTracking) {
            /* If we're tracking, instantiate a new model prefab based on the button index and */
            GameObject modelPrefab = Models[modelIndex];
            GameObject modelT = Instantiate(modelPrefab);
            //Transform model = Instantiate(modelPrefab).transform;
            _activeModels.Add(modelT);
            /* Set model position at touch position */
            var cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane p = new Plane(Vector3.up, Vector3.zero);
            float enter;
            if (p.Raycast(cameraRay, out enter)) {
                modelT.transform.position = cameraRay.GetPoint(enter);
            }

            /* Set model orientation to face toward the camera */
            //Quaternion modelRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(-Camera.main.transform.forward, Vector3.up), Vector3.up);
            //model.rotation = modelRotation;

            /* Assign the new model to the move controller, so that it can be further dragged after it leaves the button area. */
            _moveController.SetMoveObject(modelT.transform);
        }
    }

    public void OnResetButtonClicked() {
        //Tracker.SetState(InstantTrackingState.Initializing);
        //ResetButton.gameObject.SetActive(false);
        //StartSeekButton.gameObject.SetActive(false);
        //MessageBox.text = "Starting the SDK";
         foreach (var model in _activeModels) {
                Destroy(model);
            }
            _activeModels.Clear();
    }

    public void onTryAgainButtonClicked() { 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion

    #region Tracker Events
    public void OnSceneRecognized(InstantTarget target) {
        SetSceneActive(true);
    }

    public void OnSceneLost(InstantTarget target) {
        SetSceneActive(false);
    }

    private void SetSceneActive(bool active) {
        /* Because SetSceneActive(false) can be called when the scene is destroyed,
         * first check if the GameObjects and Components are still valid.
         */
        foreach (var button in Buttons) {
            if (button) {
                button.interactable = active;
            }
        }

        foreach (var model in _activeModels) {
            if (model) {
                model.SetActive(active);
            }
        }

        if (ActivityIndicator) {
            ActivityIndicator.color = active ? EnabledColor : DisabledColor;
        }

        if (_gridRenderer) {
            _gridRenderer.enabled = active;
        }
        _isTracking = active;
    }

    public void OnStateChanged(InstantTrackingState newState) {
        _currentState = newState;
        if (newState == InstantTrackingState.Tracking) {
            if (InitializationControls != null) {
                InitializationControls.SetActive(false);
            }
            StartSeekButton.gameObject.SetActive(true);
            ButtonDock.SetActive(true);
            ResetButton.gameObject.SetActive(true);
        } else {
            /* When the state is changed back to initialization, make sure that all the previous augmentations are cleared */
            foreach (var model in _activeModels) {
                Destroy(model);
            }
            _activeModels.Clear();

            if (InitializationControls != null) {
                InitializationControls.SetActive(true);
            }
            _gridRenderer.enabled = true;
            _moveController.enabled = true;
            _scaleController.enabled = true;
             MessageHandler.SetActive(true);
             ButtonDock.SetActive(false);
            _distanceController.enabled = false;
            _audioController.enabled = false;
            _panelController.enabled = false;
        }
    }

    internal void LoadAugmentation(AugmentationDescription augmentation) {
        GameObject modelPrefab = Models[augmentation.ID];
        Transform model = Instantiate(modelPrefab).transform;
        _activeModels.Add(model.gameObject);

        model.localPosition = augmentation.LocalPosition;
        model.localRotation = augmentation.LocalRotation;
        model.localScale = augmentation.LocalScale;

        model.gameObject.SetActive(false);
    }

    public void OnError(Error error) {
        PrintError("Instant Tracker error!", error);
    }

    public void OnFailedStateChange(InstantTrackingState failedState, Error error) {
        PrintError("Failed to change state to " + failedState, error);
    }
    #endregion
}
