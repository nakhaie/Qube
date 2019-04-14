using UnityEngine;

public class Cube : MonoBehaviour, ICube
{
    [SerializeField] private MeshRenderer _renderer;
    
    private int _hp;

    public void Init(string unitName, int hp, Material material)
    {
        _hp = hp;
        _renderer.material = material;
        name = unitName;
    }

    public void DestroyBody()
    {
        Destroy(gameObject);
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
    int HasActive { get; set; }
    
    void Init(string unitName, int hp, Material material);
        
    void DestroyBody();

    void Damaged(int value);

    Vector3 GetPosition();
}