using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Wikitude;

public class SaveInstantTarget : MonoBehaviour {
    public InstantTracker Tracker;
    public InstantTrackingController Controller;
    public Text InfoMessage;
    public Button SaveButton;

    public void OnChangedState(InstantTrackingState state) {
        /* Only enable the save button when tracking */
        if (state == InstantTrackingState.Tracking) {
            SaveButton.interactable = true;
        } else {
            SaveButton.interactable = false;
        }
    }

    public void OnSaveButtonPressed() {
        if (Controller.CurrentState == InstantTrackingState.Tracking)
        {
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                SaveTarget("/InstantTargetHide.wto");
                SaveScene("/InstantSceneHide.json");
            }
            else
            {
                SaveTarget("/InstantTargetRun.wto");
                SaveScene("/InstantSceneRun.json");
            }
        }
        else
        {
            InfoMessage.text = "Instant targets can only be saved when the tracker is in the tracking state.";
        }
    }

    /* Saves the instant target to disk. This only saves the point cloud representation of the scene, without any augmentations. */
    private void SaveTarget(string fileName) {
        var path = Application.persistentDataPath  + fileName;
            Tracker.SaveCurrentInstantTarget(path, SaveSuccessHandler, SaveErrorHandler);

        }

    /* Saves all augmentations to disk. Each augmentation is represented by an AugmentationDescription and the entire scene is serialized as a SceneDescription. */
    private void SaveScene(string fileName) {
        var sceneDescription = new SceneDescription();

        foreach (var augmentation in Controller.ActiveModels) {
            int id = GetAugmentationId(augmentation);
            if (id == -1) {
                /* Early return in case we cannot find the ID of an augmentation. */
                Debug.LogError("Could not find ID for augmentation " + augmentation.name);
                return;
            } else {
                sceneDescription.Augmentations.Add(new AugmentationDescription(id, augmentation.transform));
            }
        }
            string json = JsonUtility.ToJson(sceneDescription);
            InfoMessage.text = "";
            InfoMessage.text = Application.persistentDataPath + fileName;
            File.WriteAllText(Application.persistentDataPath+ fileName, json);

    }

    private void SaveSuccessHandler(string path) {
        InfoMessage.text = "The instant target was successfully saved at path: " + path;
    }

    private void SaveErrorHandler(Error error) {
            InfoMessage.text = "The following error occurred when saving the instant target. " +
            "Error code: " + error.Code + " domain: " + error.Domain + " message: " + error.Message;
    }

    /* Utility method that searches for the ID of an augmentation.
     * InstantTrackingController.Models is a list of all the augmentation that can be added to the scene.
     * The ID is simply the index in this list.
     * Returns index of the augmentation in InstantTrackingController.Models. Returns -1 if not found.
     */
    private int GetAugmentationId(GameObject augmentation) {
        for (int i = 0; i < Controller.Models.Count; ++i) {
            /* Because instantiated prefabs have the "(Clone)" suffix applied, we only check if the names start the same way. */
            if (augmentation.name.StartsWith(Controller.Models[i].name)) {
                return i;
            }
        }
        return -1;
    }
}



