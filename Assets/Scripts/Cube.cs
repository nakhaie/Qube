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

    public void DestroyBody()
    {
        HasActive = false;
    }

    public void Damaged(int value)
    {
        _hp -= value;

        if (_hp < 1)
        {
            DestroyBody();
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}

public interface ICube
{
    bool HasActive { get; set; }
    
    bool HasEnabled { get; set; }
    
    void Init(string unitName, Vector3 pos, int hp, Material material);
        
    void DestroyBody();

    void Damaged(int value);

    Vector3 GetPosition();
}