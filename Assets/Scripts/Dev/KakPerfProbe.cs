#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Unity.Profiling;
using UnityEngine;

/// <summary>
/// Performans ölçümü (performans bütçesi, 00-ana-prompt.md): kare süresi, ana iş parçacığı süresi,
/// kare başına GC tahsisi, batch / SetPass / draw call. Sadece editör ve development build.
/// Not: Editörde ölçülen GC ve süre değerleri editör yükünü de içerebilir; kesin ölçüm cihazda.
/// </summary>
public class KakPerfProbe : MonoBehaviour
{
    ProfilerRecorder mainThread, gcAlloc, batches, setPass, drawCalls;
    public int Frames { get; private set; }
    double sumFrame, sumMain, sumGc;
    public float MaxFrameMs { get; private set; }
    public long MaxGcBytes { get; private set; }
    public long MaxBatches { get; private set; }
    long sumBatches, sumSetPass, sumDraw;
    int gcFrames;

    void OnEnable()
    {
        mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
        gcAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
        setPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
    }

    void OnDisable()
    {
        mainThread.Dispose(); gcAlloc.Dispose(); batches.Dispose(); setPass.Dispose(); drawCalls.Dispose();
    }

    void Update()
    {
        if (Time.frameCount < 10) return; // yükleme karelerini atla
        Frames++;
        float ft = Time.unscaledDeltaTime * 1000f;
        sumFrame += ft;
        if (ft > MaxFrameMs) MaxFrameMs = ft;
        if (mainThread.Valid) sumMain += mainThread.LastValue * 1e-6;
        if (gcAlloc.Valid)
        {
            long g = gcAlloc.LastValue;
            sumGc += g;
            if (g > 0) gcFrames++;
            if (g > MaxGcBytes) MaxGcBytes = g;
        }
        if (batches.Valid) { sumBatches += batches.LastValue; if (batches.LastValue > MaxBatches) MaxBatches = batches.LastValue; }
        if (setPass.Valid) sumSetPass += setPass.LastValue;
        if (drawCalls.Valid) sumDraw += drawCalls.LastValue;
    }

    public float AvgGcBytes => Frames > 0 ? (float)(sumGc / Frames) : 0f;
    public float AvgBatches => Frames > 0 ? (float)sumBatches / Frames : 0f;

    public string Report()
    {
        if (Frames == 0) return "[KakPerfProbe] veri yok";
        return $"[KakPerfProbe] {Frames} kare | kare ort {sumFrame / Frames:F1} ms (en kötü {MaxFrameMs:F1}) | ana iş parçacığı ort {sumMain / Frames:F2} ms"
             + $" | GC ort {sumGc / Frames:F0} B/kare, tahsisli kare %{100f * gcFrames / Frames:F0}, en fazla {MaxGcBytes} B"
             + $" | batch ort {AvgBatches:F0} (en fazla {MaxBatches}), SetPass ort {(float)sumSetPass / Frames:F0}, draw call ort {(float)sumDraw / Frames:F0}";
    }
}
#endif
