using UnityEngine;

/// <summary>
/// Làm mặt nước rung nhẹ (nhô lên / lõm xuống rồi lặng dần) tại chỗ vật chạm nước.
/// Gắn lên object có BoxCollider2D (Is Trigger) mà MÉP TRÊN của collider nằm đúng mặt nước.
/// Kéo sprite nước (dùng shader WaterFlat) vào ô "Water Renderer".
/// Script chỉ gửi số vào shader; phần uốn mặt nước nằm trong Shader Graph.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class WaterRipple : MonoBehaviour
{
    [Header("Sprite nước dùng shader WaterFlat")]
    public SpriteRenderer waterRenderer;

    [Header("Biên độ (đơn vị world)")]
    public float minAmplitude = 0.04f;
    public float maxAmplitude = 0.18f;
    [Tooltip("Biên độ = tốc độ rơi x hệ số này, rồi kẹp trong khoảng min-max")]
    public float amplitudePerSpeed = 0.012f;
    [Tooltip("Tick: mặt nước lõm xuống trước. Bỏ tick: nhô lên trước.")]
    public bool dipFirst = false;

    [Header("Dao động")]
    [Tooltip("Số lần rung mỗi giây")]
    public float frequency = 3f;
    [Tooltip("Càng lớn càng nhanh lặng")]
    public float damping = 3.5f;
    [Tooltip("Bán kính ảnh hưởng quanh chỗ chạm (world units)")]
    public float radius = 2.5f;

    [Header("Khi ra khỏi nước")]
    public bool rippleOnExit = true;
    public float exitMultiplier = 0.6f;

    static readonly int SurfaceY = Shader.PropertyToID("_SurfaceY");
    static readonly int ImpactX = Shader.PropertyToID("_ImpactX");
    static readonly int Amp = Shader.PropertyToID("_Amp");
    static readonly int Phase = Shader.PropertyToID("_Phase");
    static readonly int Radius = Shader.PropertyToID("_Radius");

    BoxCollider2D box;
    MaterialPropertyBlock mpb;
    bool active;
    float startTime, strength, impactX;

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        mpb = new MaterialPropertyBlock();
        Apply(0f, 0f);
    }

    void Update()
    {
        if (!active) return;

        float age = Time.time - startTime;
        float amp = strength * Mathf.Exp(-damping * age);

        if (amp < 0.001f)
        {
            active = false;
            Apply(0f, 0f);
            return;
        }
        Apply(amp, age * frequency * Mathf.PI * 2f);
    }

    void Apply(float amp, float phase)
    {
        if (waterRenderer == null) return;

        waterRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat(SurfaceY, box.bounds.max.y);   // mép trên collider = mặt nước
        mpb.SetFloat(ImpactX, impactX);
        mpb.SetFloat(Amp, dipFirst ? -amp : amp);
        mpb.SetFloat(Phase, phase);
        mpb.SetFloat(Radius, radius);
        waterRenderer.SetPropertyBlock(mpb);
    }

    void Trigger(Collider2D other, float multiplier)
    {
        var rb = other.attachedRigidbody;
        if (rb == null) return;

#if UNITY_6000_0_OR_NEWER
        float speed = Mathf.Abs(rb.linearVelocity.y);
#else
        float speed = Mathf.Abs(rb.velocity.y);
#endif
        strength = Mathf.Clamp(speed * amplitudePerSpeed, minAmplitude, maxAmplitude) * multiplier;
        impactX = other.bounds.center.x;
        startTime = Time.time;
        active = true;
    }

    void OnTriggerEnter2D(Collider2D other) { Trigger(other, 1f); }

    void OnTriggerExit2D(Collider2D other)
    {
        if (rippleOnExit) Trigger(other, exitMultiplier);
    }
}