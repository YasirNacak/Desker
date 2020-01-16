#define DDEB

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeskerManager : MonoBehaviour
{
    private static DeskerManager _instance;

    public static DeskerManager Instance
    {
        get { return _instance; }
    }

    public void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public GameObject Canvas;

    public List<GameObject> Objects;

    public GameObject MoveRightButton;

    public GameObject MoveLeftButton;

    public GameObject ObjectSelectorScrollArea;

    public GameObject ShowObjectSelectorButton;

    public GameObject RemoveObjectButton;

    public GameObject ShareButton;

    private bool _isObjectSelectorOpen;

    public Material SelectedMaterial;

    private GameObject _selected;

    private Material[] _selectedObjectOriginalMaterials;

    void Start()
    {
        _isObjectSelectorOpen = false;
    }

    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (Physics.Raycast(ray, out var hit))
                {
                    if (!IsPointerOverUiObject())
                    {
                        if (_selected != null)
                        {
                            SetObjectSelected(_selected, false);
                        }

                        var selection = hit.transform;
                        _selected = selection.gameObject;
                        SetObjectSelected(_selected, true);

                        SetControlGuiEnabled(true);
                    }
                }
                else
                {
                    if (!IsPointerOverUiObject())
                    {
                        if (_selected != null)
                        {
                            SetObjectSelected(_selected, false);
                            _selected = null;

                            SetControlGuiEnabled(false);
                        }
                    }
                }
            }
        }
    }

    private void SetObjectSelected(GameObject go, bool isSelected)
    {
        var meshRenderer = go.GetComponent<MeshRenderer>();
        var matCount = meshRenderer.materials.Length;
        
        if (isSelected)
        {
            _selectedObjectOriginalMaterials = new Material[matCount];
            var newMaterials = new Material[matCount];
            for (int i = 0; i < matCount; i++)
            {
                _selectedObjectOriginalMaterials[i] = meshRenderer.materials[i];
                newMaterials[i] = SelectedMaterial;
            }

            meshRenderer.materials = newMaterials;
        }
        else
        {
            meshRenderer.materials = _selectedObjectOriginalMaterials;
        }
    }

    private void SetControlGuiEnabled(bool isEnabled)
    {
        MoveRightButton.SetActive(isEnabled);
        MoveLeftButton.SetActive(isEnabled);
        RemoveObjectButton.SetActive(isEnabled);
    }

    private bool IsPointerOverUiObject()
    {
        var pEventData = new PointerEventData(EventSystem.current);
        pEventData.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pEventData, raycastResults);
        return raycastResults.Count > 0;
    }

    public void OnShowObjectSelectorButtonClicked()
    {
        if (_isObjectSelectorOpen == false)
        {
            LeanTween.moveX(ShowObjectSelectorButton.GetComponent<RectTransform>(), -475, 0.2f).setEase(LeanTweenType.easeOutSine);
            LeanTween.moveX(ShareButton.GetComponent<RectTransform>(), -475, 0.2f).setEase(LeanTweenType.easeOutSine);
            LeanTween.moveX(ObjectSelectorScrollArea.GetComponent<RectTransform>(), -200, 0.2f).setEase(LeanTweenType.easeOutSine);
            _isObjectSelectorOpen = true;
        }
        else
        {
            LeanTween.moveX(ShowObjectSelectorButton.GetComponent<RectTransform>(), -75, 0.2f).setEase(LeanTweenType.easeInSine);
            LeanTween.moveX(ShareButton.GetComponent<RectTransform>(), -75, 0.2f).setEase(LeanTweenType.easeInSine);
            LeanTween.moveX(ObjectSelectorScrollArea.GetComponent<RectTransform>(), 200, 0.2f).setEase(LeanTweenType.easeInSine);
            _isObjectSelectorOpen = false;
        }
    }

    public void OnMoveRightButtonClicked()
    {
        if (_selected != null)
        {
            _selected.transform.position = new Vector3(_selected.transform.position.x + 0.05f, _selected.transform.position.y, _selected.transform.position.z);
        }
    }

    public void OnMoveLeftButtonClicked()
    {
        if (_selected != null)
        {
            _selected.transform.position = new Vector3(_selected.transform.position.x - 0.05f, _selected.transform.position.y, _selected.transform.position.z);
        }
    }

    public void OnRemoveSelectedButtonClicked()
    {
        if (_selected != null)
        {
            SetObjectSelected(_selected, false);
            _selected.gameObject.SetActive(false);
            _selected = null;

            SetControlGuiEnabled(false);
        }
    }

    public void AddObject(string objectName)
    {
        foreach (var o in Objects)
        {
            if (o.name.Equals(objectName))
            {
                o.SetActive(true);
            }
        }
    }

    public void OnShareButtonClicked()
    {
        Canvas.SetActive(false);

        ScreenCapture.CaptureScreenshot("desker_design.png");

        StartCoroutine(ShareScreenshot());
    }

    private IEnumerator ShareScreenshot()
    {
        yield return new WaitForSeconds(0.5f);

        var packageToSendWith = "com.google.android.gm";

        if (NativeShare.TargetExists("org.telegram.messenger"))
        {
            packageToSendWith = "org.telegram.messenger";
        } 
        else if (NativeShare.TargetExists("com.whatsapp"))
        {
            packageToSendWith = "com.whatsapp";
        }

        new NativeShare().AddFile(Application.persistentDataPath + "/" + "desker_design.png").SetSubject("Designed with Desker").SetText("Designed with Desker").SetTarget(packageToSendWith).Share();

        Canvas.SetActive(true);
    }
}
