using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoorKeyArrowOverlay
{
    GameObject _canvasObject;
    GameObject _arrowObject;
    RectTransform _arrowRect;
    Image _arrowImage;
    GameObject _lineRoot;
    readonly List<RectTransform> _lineDashes = new List<RectTransform>();
    readonly List<Image> _lineDashImages = new List<Image>();
    bool _visible;

    const int LINE_DASH_COUNT = 36;
    const float ENDPOINT_PADDING = 120f;
    const float KEYBAR_EXTRA_PADDING = 30f;
    const float PLAYER_EXTRA_PADDING = 150f;

    public bool IsVisible => _visible;

    public void Show()
    {
        EnsureUI();
        _visible = true;
        if (_canvasObject != null)
            _canvasObject.SetActive(true);
    }

    public void Hide()
    {
        _visible = false;
        if (_canvasObject != null)
            _canvasObject.SetActive(false);
        if (_lineRoot != null)
            _lineRoot.SetActive(false);
    }

    public void Tick(Transform playerTransform, Transform keyBarParent)
    {
        if (!_visible) return;
        if (playerTransform == null || keyBarParent == null || Camera.main == null)
        {
            Hide();
            return;
        }

        RectTransform kbRect = keyBarParent.GetComponent<RectTransform>();
        if (kbRect == null) return;

        Vector3[] corners = new Vector3[4];
        kbRect.GetWorldCorners(corners);
        Vector2 keybarBottomCentre = ((Vector2)(corners[0] + corners[1])) * 0.5f;

        Vector3 playerScreenRaw = Camera.main.WorldToScreenPoint(playerTransform.position);
        if (playerScreenRaw.z < 0f) playerScreenRaw = -playerScreenRaw;
        float margin = 30f;
        Vector2 playerScreen = new Vector2(
            Mathf.Clamp(playerScreenRaw.x, margin, Screen.width - margin),
            Mathf.Clamp(playerScreenRaw.y, margin, Screen.height - margin));

        Vector2 lineStart = playerScreen + new Vector2(0f, 50f);
        Vector2 lineEnd = keybarBottomCentre;

        Vector2 dir = lineEnd - lineStart;
        float length = dir.magnitude;
        if (length < 1f) return;
        dir /= length;

        float pad = Mathf.Min(ENDPOINT_PADDING, length * 0.45f);
        Vector2 startP = lineStart + dir * (pad + PLAYER_EXTRA_PADDING);
        Vector2 endP = lineEnd - dir * (pad + KEYBAR_EXTRA_PADDING);

        float paddedLength = (endP - startP).magnitude;
        if (paddedLength < 1f)
        {
            startP = lineStart;
            endP = lineEnd;
        }

        Vector2 mid = (startP + endP) * 0.5f;
        Vector2 perp = new Vector2(dir.y, -dir.x);
        Vector2 control = mid - perp * Mathf.Min(80f, paddedLength * 0.4f);

        int count = _lineDashes.Count;
        if (count > 0 && _lineRoot != null)
            _lineRoot.SetActive(true);

        float endAngle = 0f;
        for (int i = 0; i < count; i++)
        {
            float t = (i + 1) / (float)(count + 1);
            float oneMinusT = 1f - t;

            Vector2 pos = oneMinusT * oneMinusT * startP
                           + 2f * oneMinusT * t * control
                           + t * t * endP;

            Vector2 tangent = 2f * oneMinusT * (control - startP)
                               + 2f * t * (endP - control);
            if (tangent.sqrMagnitude < 0.01f)
                tangent = endP - startP;

            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            if (i == count - 1) endAngle = angle;

            var dash = _lineDashes[i];
            if (dash != null)
            {
                dash.position = new Vector3(pos.x, pos.y, 0f);
                dash.rotation = Quaternion.Euler(0f, 0f, angle);
                dash.sizeDelta = new Vector2(12f, 2f);
            }

            if (i < _lineDashImages.Count && _lineDashImages[i] != null)
                _lineDashImages[i].color = Color.white;
        }

        if (count > 0)
            endAngle = Mathf.Atan2(endP.y - control.y, endP.x - control.x) * Mathf.Rad2Deg;

        _arrowRect.position = new Vector3(endP.x, endP.y, 0f);
        _arrowObject.transform.rotation = Quaternion.Euler(0f, 0f, endAngle - 90f);
    }

    public void Dispose()
    {
        if (_canvasObject != null)
            Object.Destroy(_canvasObject);
        _canvasObject = null;
        _arrowObject = null;
        _arrowRect = null;
        _arrowImage = null;
        _lineRoot = null;
        _lineDashes.Clear();
        _lineDashImages.Clear();
        _visible = false;
    }

    void EnsureUI()
    {
        if (_canvasObject != null) return;

        _canvasObject = new GameObject("KeybarPressArrowCanvas");
        _canvasObject.transform.SetParent(null);
        Object.DontDestroyOnLoad(_canvasObject);

        Canvas canvas = _canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 49;
        _canvasObject.AddComponent<CanvasScaler>();

        _arrowObject = new GameObject("Arrow");
        _arrowObject.transform.SetParent(_canvasObject.transform, false);

        _arrowRect = _arrowObject.AddComponent<RectTransform>();
        _arrowRect.sizeDelta = new Vector2(50f, 50f);

        _arrowImage = _arrowObject.AddComponent<Image>();
        _arrowImage.sprite = BuildArrowSprite();
        _arrowImage.color = Color.white;

        _lineRoot = new GameObject("ArrowLine");
        _lineRoot.transform.SetParent(_canvasObject.transform, false);

        for (int i = 0; i < LINE_DASH_COUNT; i++)
        {
            var dashGO = new GameObject("Dash");
            dashGO.transform.SetParent(_lineRoot.transform, false);
            var dashRect = dashGO.AddComponent<RectTransform>();
            dashRect.sizeDelta = new Vector2(16f, 4f);
            dashRect.pivot = new Vector2(0.5f, 0.5f);
            var dashImg = dashGO.AddComponent<Image>();
            dashImg.color = Color.white;
            _lineDashes.Add(dashRect);
            _lineDashImages.Add(dashImg);
        }

        Hide();
    }

    Sprite BuildArrowSprite()
    {
        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color clear = new Color(0, 0, 0, 0);
        Color white = Color.white;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x + 0.5f) / size * 2f - 1f;
                float ny = (y + 0.5f) / size * 2f - 1f;

                bool inTriangle = ny > 0.1f && Mathf.Abs(nx) < (0.6f - ny * 0.6f);
                bool inStem = ny >= -0.75f && ny <= 0.1f && Mathf.Abs(nx) < 0.13f;
                tex.SetPixel(x, y, (inTriangle || inStem) ? white : clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
