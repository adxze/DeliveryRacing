using System.Collections.Generic;
using UnityEngine;

public class OffScreenTargetIndicatorByTag : MonoBehaviour
{
    [Tooltip("Tag of objects that will receive an off-screen indicator.")]
    public string targetTag = "Items";

    [Tooltip("Prefab for the indicator (must have a SpriteRenderer).")]
    public GameObject indicatorPrefab;

    private SpriteRenderer _spriteRenderer;
    private float _spriteWidth;
    private float _spriteHeight;

    private Camera _camera;

    private Dictionary<GameObject, GameObject> _targetIndicators = new Dictionary<GameObject, GameObject>();

    private void Start()
    {
        _camera = Camera.main;
        _spriteRenderer = indicatorPrefab.GetComponent<SpriteRenderer>();

        var bounds = _spriteRenderer.bounds;
        _spriteWidth = bounds.size.x * 0.5f;
        _spriteHeight = bounds.size.y * 0.5f;

        RegisterAllTaggedObjects();
    }

    private void Update()
    {
        RegisterAllTaggedObjects();
        
        RemoveNullEntries();
        
        UpdateAllIndicators();
    }



    private void RegisterAllTaggedObjects()
    {
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(targetTag);
        foreach (var obj in objectsWithTag)
        {
            if (!_targetIndicators.ContainsKey(obj))
            {
                GameObject indicator = Instantiate(indicatorPrefab);
                indicator.SetActive(false);
                _targetIndicators.Add(obj, indicator);
            }
        }
    }

  
    private void RemoveNullEntries()
    {
        List<GameObject> destroyedTargets = new List<GameObject>();

        foreach (var kvp in _targetIndicators)
        {
            GameObject target = kvp.Key;
            if (target == null)
            {
                destroyedTargets.Add(target);
            }
        }

        foreach (var deadTarget in destroyedTargets)
        {
            if (_targetIndicators[deadTarget] != null)
            {
                Destroy(_targetIndicators[deadTarget]);
            }
            _targetIndicators.Remove(deadTarget);
        }
    }

    private void UpdateAllIndicators()
    {
        foreach (var kvp in _targetIndicators)
        {
            GameObject target = kvp.Key;
            GameObject indicator = kvp.Value;

            UpdateTargetIndicator(target, indicator);
        }
    }

    private void UpdateTargetIndicator(GameObject target, GameObject indicator)
    {
        Vector3 viewportPos = _camera.WorldToViewportPoint(target.transform.position);

        bool isOffScreen = 
            (viewportPos.x <= 0f || viewportPos.x >= 1f ||
             viewportPos.y <= 0f || viewportPos.y >= 1f);

        if (isOffScreen)
        {
            indicator.SetActive(true);

            // menentukan besar ViewPort (besar layar)
            Vector3 spriteSizeInViewPort =
                _camera.WorldToViewportPoint(new Vector3(_spriteWidth, _spriteHeight, 0f)) -
                _camera.WorldToViewportPoint(Vector3.zero);

            // Clamp posisi agar diam di pinggir layar
            viewportPos.x = Mathf.Clamp(viewportPos.x, spriteSizeInViewPort.x, 1f - spriteSizeInViewPort.x);
            viewportPos.y = Mathf.Clamp(viewportPos.y, spriteSizeInViewPort.y, 1f - spriteSizeInViewPort.y);

            // Ubah posisi Clamp ke World positition 
            Vector3 worldPos = _camera.ViewportToWorldPoint(viewportPos);
            worldPos.z = 0f;
            indicator.transform.position = worldPos;

            // Rotate Indicator ke arah target
            Vector3 direction = target.transform.position - indicator.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            indicator.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            // If on-screen, matikan indicator nya
            indicator.SetActive(false);
        }
    }
}
