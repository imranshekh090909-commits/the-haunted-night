using UnityEngine;
using UnityEngine.Rendering;

namespace HauntedNight.Perf
{
    /// <summary>
    /// Auto-detects device class on first launch and applies a graphics tier:
    /// Low (2GB RAM / Mali-G52 class), Mid, High. Keeps the APK's promise of
    /// running on budget phones without shipping separate builds.
    /// </summary>
    public class LowEndOptimizer : MonoBehaviour
    {
        public enum Tier { Low, Mid, High }

        public static Tier CurrentTier { get; private set; } = Tier.Mid;

        [SerializeField] private float lowRenderScale = 0.7f;
        [SerializeField] private float midRenderScale = 0.85f;
        [SerializeField] private float highRenderScale = 1f;

        private void Awake()
        {
            Detect();
            Apply(CurrentTier);
        }

        private void Detect()
        {
            int ramMb = SystemInfo.systemMemorySize;
            int cpuCores = SystemInfo.processorCount;
            string gpu = SystemInfo.graphicsDeviceName ?? string.Empty;

            if (ramMb <= 2600 || cpuCores <= 4)
                CurrentTier = Tier.Low;
            else if (ramMb <= 5000 || gpu.Contains("Mali-G5"))
                CurrentTier = Tier.Mid;
            else
                CurrentTier = Tier.High;
        }

        public void Apply(Tier tier)
        {
            CurrentTier = tier;

            switch (tier)
            {
                case Tier.Low:
                    QualitySettings.SetQualityLevel(0, true);
                    QualitySettings.shadowDistance = 18f;
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    QualitySettings.antiAliasing = 0;
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = 30;
                    SetRenderScale(lowRenderScale);
                    break;

                case Tier.Mid:
                    QualitySettings.SetQualityLevel(2, true);
                    QualitySettings.shadowDistance = 32f;
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.antiAliasing = 2;
                    Application.targetFrameRate = 45;
                    SetRenderScale(midRenderScale);
                    break;

                default:
                    QualitySettings.SetQualityLevel(4, true);
                    QualitySettings.shadowDistance = 55f;
                    QualitySettings.antiAliasing = 4;
                    Application.targetFrameRate = 60;
                    SetRenderScale(highRenderScale);
                    break;
            }
        }

        private void SetRenderScale(float scale)
        {
            if (GraphicsSettings.currentRenderPipeline == null) return;
            var asset = GraphicsSettings.currentRenderPipeline;
            if (asset is UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urp)
                urp.renderScale = scale;
        }
    }
}
