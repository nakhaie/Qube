using UnityEngine;

public class Cube : MonoBehaviour, ICube
{
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private Collider _collider;
    
    private int _hp;

    public bool HasActive
    {
        get => gameObject.activeSelf;
        set => gameObject.SetActive(value);
    }
    
    public bool HasEnabled
    {
        get => _collider.enabled;
        set => _collider.enabled = value;
    }

    public void Init(string unitName, Vector3 pos, int hp, Material material)
    {
        _hp = hp;
        _renderer.material = material;
        name = unitName;

        transform.position = pos;

        HasActive = true;
    }
    
    public void Init(string unitName, Vector3 pos, int hp, Material material, Color cubeColor)
    {
        _hp = hp;
        _renderer.material = material;
        _renderer.material.color = cubeColor;
        name = unitName;

        transform.position = pos;

        HasActive = true;
    }

    public void DestroyBody()
    {
        HasActive = false;
    }

    public bool Damaged(int value)
    {
        _hp -= value;

        if (_hp >= 1)
            return false;
        
        DestroyBody();
        return true;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}

public struct SimpleCube : ICube
{
    public bool HasActive
    {
        get => false;
        set
        {
            
        }
    }

    public bool HasEnabled
    {
        get => false;
        set
        {
            
        }
    }

    private Vector3 _pos;

    public SimpleCube(Vector3 pos)
    {
        _pos = pos;
    }
    
    public void Init(string unitName, Vector3 pos, int hp, Material material)
    {
        _pos = pos;
    }

    public void Init(string unitName, Vector3 pos, int hp, Material material, Color cubeColor)
    {
        _pos = pos;
    }

    public void DestroyBody()
    {
        
    }

    public bool Damaged(int value)
    {
        return false;
    }

    public Vector3 GetPosition()
    {
        return _pos;
    }
}

public interface ICube
{
    bool HasActive { get; set; }
    
    bool HasEnabled { get; set; }
    
    void Init(string unitName, Vector3 pos, int hp, Material material);

    void Init(string unitName, Vector3 pos, int hp, Material material, Color cubeColor);
        
    void DestroyBody();

    bool Damaged(int value);

    Vector3 GetPosition();
}