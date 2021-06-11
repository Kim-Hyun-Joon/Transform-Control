using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour {

	private static UIManager instance;

    [SerializeField] private TransformPanel positionLabel;
    [SerializeField] private TransformPanel rotationLabel;
    [SerializeField] private TransformPanel scaleLabel;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Dropdown modeBox;
    [SerializeField] private Text nameText;

    public static UIManager Instance {
		get {
			if (instance == null) instance = FindObjectOfType<UIManager>();

			return instance;
		}
	}

    public void SetTransformLabel(Transform transform) {
        positionLabel.SetTransform(transform.position);
        rotationLabel.SetTransform(transform.rotation.eulerAngles);
        scaleLabel.SetTransform(transform.localScale);
    }
    //나중에 UI 부분 인스턴스화 할때 사용할 방식
    /*
    [SerializeField] private GameObject gameoverUI;
    [SerializeField] private Crosshair crosshair;

    [SerializeField] private Text healthText;
    [SerializeField] private Text lifeText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text ammoText;
    [SerializeField] private Text waveText;

    public void UpdateAmmoText(int magAmmo, int remainAmmo) {
        ammoText.text = magAmmo + "/" + remainAmmo;
    }

    public void UpdateScoreText(int newScore) {
        scoreText.text = "Score : " + newScore;
    }

    public void UpdateWaveText(int waves, int count) {
        waveText.text = "Wave : " + waves + "\nEnemy Left : " + count;
    }

    public void UpdateLifeText(int count) {
        lifeText.text = "Life : " + count;
    }

    public void UpdateCrossHairPosition(Vector3 worldPosition) {
        crosshair.UpdatePosition(worldPosition);
    }

    public void UpdateHealthText(float health) {
        healthText.text = Mathf.Floor(health).ToString();
    }

    public void SetActiveCrosshair(bool active) {
        crosshair.SetActiveCrosshair(active);
    }

    public void SetActiveGameoverUI(bool active) {
        gameoverUI.SetActive(active);
    }

    public void GameRestart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    */

    public TransformControl control;


	public void OnModeChanged(int index) {
        if (control == null) return;
		switch (index) {
			case 0:
				control.mode = TransformControl.TransformMode.Translate;
				break;
			case 1:
				control.mode = TransformControl.TransformMode.Rotate;
				break;
			case 2:
				control.mode = TransformControl.TransformMode.Scale;
				break;
		}
	}

    public int GetMode() {
        return modeBox.value+1;
    }
    public void UpdateNameText(string name) {
        nameText.text = name;
    }

}

