using UnityEngine;
[ExecuteInEditMode]
enum Contour_Property
{
    _Threshold,
    _InvRange,
    _ColorSensitivity,
    _DepthSensitivity,
    _NormalSensitivity,
    _InvFallOff
}
[AddComponentMenu("ContourEffect")]
public class ContourController : MonoBehaviour
{
    #region Private Properties
        //Rendering camera
        [SerializeField] private Camera renderCamera;
        [SerializeField] private Shader contourShader;
        // Line color
        [SerializeField] private static Color _lineColor = Color.black;
        // Normal sensitivity
        [SerializeField, Range(0, 1)] private float _normalSensitivity = 0f;
        // Lower threshold
        private float _lowerThreshold = 0f;
        // Upper threshold
        private float _upperThreshold = 0.3f;
        // Color sensitivity
         private float _colorSensitivity = 0.3f;
        // Depth sensitivity
        private float _depthSensitivity = 1f;       
        // Depth fall-off
        private float _fallOffDepth = 40;        
        private Material contourMTL;
        private Color _backgroundColor = Color.white - _lineColor;
    #endregion
    
    #region Public Properties        
        public Color lineColor
        {
            get { return _lineColor; }
            set { _lineColor = value; }
        }
    
        public Color backgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        //public float lowerThreshold
        //{
        //    get { return _lowerThreshold; }
        //    set { _lowerThreshold = value; }
        //}

        //public float upperThreshold
        //{
        //    get { return _upperThreshold; }
        //    set { _upperThreshold = value; }
        //}

        //public float colorSensitivity
        //{
        //    get { return _colorSensitivity; }
        //    set { _colorSensitivity = value; }
        //}

        //public float depthSensitivity
        //{
        //    get { return _depthSensitivity; }
        //    set { _depthSensitivity = value; }
        //}

        public float normalSensitivity
        {
            get { return _normalSensitivity; }
            set { _normalSensitivity = value; }
        }

        public float fallOffDepth
        {
            get { return _fallOffDepth; }
            set { _fallOffDepth = value; }
        }
        public void ContourUpdate()
        {
            if (_depthSensitivity > 0)
                renderCamera.depthTextureMode |= DepthTextureMode.Depth;
        }
        //
        public void SetLowerThreshold(float v)
        {
            _lowerThreshold = v;
        }
        public void SetUpperThreshold(float v)
        {
            _upperThreshold = v;
        }
        public void SetColorSensitivity(float v)
        {
            _colorSensitivity = v;
        }
        public void SetDepthSensitivity(float v)
        {
            _depthSensitivity = v;
        }
    #endregion

    #region MonoBehaviour Functions
    void OnValidate()
        {
            _lowerThreshold = Mathf.Min(_lowerThreshold, _upperThreshold);
        }

        void OnDestroy()
        {
            if (contourMTL != null)
            {
                if (Application.isPlaying)
                    Destroy(contourMTL);
                else
                    DestroyImmediate(contourMTL);
            }
        }
        
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (contourMTL == null)
            {
                contourMTL = new Material(contourShader);
                contourMTL.hideFlags = HideFlags.DontSave;
            }
            contourMTL.SetColor("_Color", _lineColor);
            contourMTL.SetColor("_Background", _backgroundColor);
            contourMTL.SetFloat("_Threshold", _lowerThreshold);
            contourMTL.SetFloat("_InvRange", 1 / (_upperThreshold - _lowerThreshold));
            contourMTL.SetFloat("_ColorSensitivity", _colorSensitivity);
            contourMTL.SetFloat("_DepthSensitivity", _depthSensitivity * 2);
            contourMTL.SetFloat("_NormalSensitivity", _normalSensitivity);
            contourMTL.SetFloat("_InvFallOff", 1 / _fallOffDepth);
            

            if (_colorSensitivity > 0)
                    contourMTL.EnableKeyword("_CONTOUR_COLOR");
                else
                    contourMTL.DisableKeyword("_CONTOUR_COLOR");

                if (_depthSensitivity > 0)
                    contourMTL.EnableKeyword("_CONTOUR_DEPTH");
                else
                    contourMTL.DisableKeyword("_CONTOUR_DEPTH");

                if (_normalSensitivity > 0)
                    contourMTL.EnableKeyword("_CONTOUR_NORMAL");
                else
                    contourMTL.DisableKeyword("_CONTOUR_NORMAL");

                Graphics.Blit(source, destination, contourMTL);
        }
    #endregion
}