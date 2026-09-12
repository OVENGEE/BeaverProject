using UnityEngine;

public class DestructibleTerrain : MonoBehaviour
{
    [SerializeField] 
    private SpriteRenderer m_spriteRenderer;
    private ModifiableTexture m_modifiableTexture;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_modifiableTexture = ModifiableTexture.CreateFromSprite(m_spriteRenderer.sprite);
        m_spriteRenderer.sprite = m_modifiableTexture.Sprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RemoveTerrainAt(Vector2 worldPosition, float radius)
    {
        float pixelSize = 1 / m_modifiableTexture.Spr
    }
}
