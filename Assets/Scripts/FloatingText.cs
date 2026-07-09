using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public float floatSpeed = 1.5f;
    public float fadeSpeed = 1.5f;
    public float lifetime = 1f;
    
    private TextMesh textMesh;
    private float timer;
    
    void Start()
    {
        textMesh = GetComponent<TextMesh>();
        timer = 0f;
    }
    
    void Update()
    {
        timer += Time.unscaledDeltaTime;
        transform.position += Vector3.up * floatSpeed * Time.unscaledDeltaTime;
        
        if (textMesh != null)
        {
            Color c = textMesh.color;
            c.a = Mathf.Lerp(1f, 0f, timer / lifetime);
            textMesh.color = c;
        }
        
        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
