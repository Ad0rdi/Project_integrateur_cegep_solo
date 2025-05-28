/* Original auho name: Donavan Sirois
 * Creation date : 
 * Goal : Code the pixel art filter
 *  Modification listing:
 */

using UnityEngine;
using UnityEngine.UI;

public class PixelArtCamera : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    private RawImage _rawImage;
    [SerializeField] private int _cameraHeight;

    private RenderTexture _renderTexture;

    private void Awake()
    {
        _rawImage = GameObject.Find("Camera Display").GetComponent<RawImage>();
    }

    private void Start()
    {
        UpdateRenderTexture();
    }

    public void UpdateRenderTexture()
    {
        if (!_rawImage) return; //Pour évité des erreurs inutiles au début du jeu.
        if (_renderTexture != null)
        {
            _renderTexture.Release();
        }

        float aspectRatio = (float)Screen.width / Screen.height;
        int _cameraWidth = Mathf.RoundToInt(aspectRatio * _cameraHeight);

        _renderTexture = new RenderTexture(_cameraWidth, _cameraHeight, 16, RenderTextureFormat.ARGB32);
        _renderTexture.filterMode = FilterMode.Point;

        _renderTexture.Create();
        _camera.targetTexture = _renderTexture;
        _rawImage.texture = _renderTexture;
    }
}