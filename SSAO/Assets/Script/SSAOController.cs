using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SSAOController : MonoBehaviour
{
    [SerializeField] private Material ssaoMaterial;
    [SerializeField] private Camera renderCamera;    
    [SerializeField] private bool showAO = false;
    [SerializeField] private bool showSC = false;
    
    [Range(0, 0.002f)]
    [SerializeField] private float DepthBiasValue = 0.002f;        
    [Range(0, 2)]
    [SerializeField] private int DownSample = 0;
    [Range(1, 4)]
    [SerializeField] private int BlurRadius = 1;
    [Range(0, 0.2f)]
    [SerializeField] private float BilaterFilterStrength = 0.2f;

    private List<Vector4> sampleKernelList = new List<Vector4>();
    private float SampleKernelRadius = 1.0f;
    private int SampleKernelCount = 16;
    private float AOStrength = 1.0f;

    public enum SSAOPassName
    {
        GenerateAO = 0,
        BilateralFilter = 1,
        Composite = 2,
        SC = 3
    }
   
    private void OnEnable()
    {
        renderCamera.depthTextureMode |= DepthTextureMode.DepthNormals;        
    }

    private void OnDisable()
    {
        renderCamera.depthTextureMode &= ~DepthTextureMode.DepthNormals;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        GenerateAOSampleKernel();

        var aoRT = RenderTexture.GetTemporary(source.width >> DownSample, source.height >> DownSample, 0);
        var scRT = RenderTexture.GetTemporary(source.width >> DownSample, source.height >> DownSample, 0);
        ssaoMaterial.SetMatrix("_InverseProjectionMatrix", renderCamera.projectionMatrix.inverse);
        ssaoMaterial.SetFloat("_DepthBiasValue", DepthBiasValue);
        ssaoMaterial.SetVectorArray("_SampleKernelArray", sampleKernelList.ToArray());
        ssaoMaterial.SetFloat("_SampleKernelCount", sampleKernelList.Count);
        ssaoMaterial.SetFloat("_AOStrength", AOStrength);
        ssaoMaterial.SetFloat("_SampleKeneralRadius", SampleKernelRadius);
        Graphics.Blit(source, aoRT, ssaoMaterial, (int)SSAOPassName.GenerateAO);

        var blurRT = RenderTexture.GetTemporary(source.width >> DownSample, source.height >> DownSample, 0);
        ssaoMaterial.SetFloat("_BilaterFilterFactor", 1.0f - BilaterFilterStrength);

        ssaoMaterial.SetVector("_BlurRadius", new Vector4(BlurRadius, 0, 0, 0));
        Graphics.Blit(aoRT, blurRT, ssaoMaterial, (int)SSAOPassName.BilateralFilter);

        ssaoMaterial.SetVector("_BlurRadius", new Vector4(0, BlurRadius, 0, 0));
        if (showAO && !showSC)
        {
            Graphics.Blit(blurRT, destination, ssaoMaterial, (int)SSAOPassName.BilateralFilter);
        }
        else if (showAO && showSC)
        {
            Graphics.Blit(blurRT, destination, ssaoMaterial, (int)SSAOPassName.BilateralFilter);
            Graphics.Blit(scRT, destination, ssaoMaterial, (int)SSAOPassName.SC);
        }
        else
        {
            Graphics.Blit(blurRT, aoRT, ssaoMaterial, (int)SSAOPassName.BilateralFilter);
            ssaoMaterial.SetTexture("_AOTex", aoRT);
            Graphics.Blit(source, destination, ssaoMaterial, (int)SSAOPassName.Composite);
        }

        RenderTexture.ReleaseTemporary(aoRT);
        RenderTexture.ReleaseTemporary(scRT);
        RenderTexture.ReleaseTemporary(blurRT);
    }

    private void GenerateAOSampleKernel()
    {        
        if (SampleKernelCount == sampleKernelList.Count)
            return;
        sampleKernelList.Clear();
        for (int i = 0; i < SampleKernelCount; i++)
        {
            var vec = new Vector4(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(0, 1.0f), 1.0f);
            vec.Normalize();
            var scale = (float)i / SampleKernelCount;            
            scale = Mathf.Lerp(0.01f, 1.0f, scale * scale);
            vec *= scale;
            sampleKernelList.Add(vec);
        }
    }
    public void SetShowAO(bool status)
    {
        showAO = status;
    }
    public void SetShowSC(bool status)
    {
        showSC = status;
    }
    public void SetAOStrength(float v)
    {
        AOStrength = v;
    }
    public void SetSampleCount(int v)
    {
        SampleKernelCount = v;
    }
    public void SetSampleRadius(float v)
    {
        SampleKernelRadius = v;
    }
    //
    public bool GetShowAO()
    {
        return showAO;
    }

    public bool GetShowSC()
    {
        return showSC;
    }
}