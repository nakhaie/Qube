using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simplePool
{
    const int DEFAULT_POOL_SIZE = 3;

    class Pool
    {
        int nextId = 1;

        // The structure containing our inactive objects.
        // Why a Stack and not a List? Because we'll never need to
        // pluck an object from the start or middle of the array.
        // We'll always just grab the last one, which eliminates
        // any need to shuffle the objects around in memory.
        Stack<GameObject> inactive;

        // The prefab that we are pooling
        GameObject prefab;

        // Constructor
        public Pool(GameObject prefab, int initialQty)
        {
            this.prefab = prefab;

            // If Stack uses a linked list internally, then this
            // whole initialQty thing is a placebo that we could
            // strip out for more minimal code. But it can't *hurt*.
            inactive = new Stack<GameObject>(initialQty);
        }

        // Spawn an object from our pool
        public GameObject Spawn(Vector3 pos, Quaternion rot)
        {
            GameObject obj;
            if (inactive.Count == 0)
            {
                // We don't have an object in our pool, so we
                // instantiate a whole new object.
                obj = (GameObject)GameObject.Instantiate(prefab, pos, rot);
                obj.name = prefab.name + " (" + (nextId++) + ")";

                // Add a PoolMember component so we know what pool
                // we belong to.
                obj.AddComponent<PoolMember>().myPool = this;
            }
            else
            {
                // Grab the last object in the inactive array
                obj = inactive.Pop();

                if (obj == null)
                {
                    // The inactive object we expected to find no longer exists.
                    // The most likely causes are:
                    //   - Someone calling Destroy() on our object
                    //   - A scene change (which will destroy all our objects).
                    //     NOTE: This could be prevented with a DontDestroyOnLoad
                    //	   if you really don't want this.
                    // No worries -- we'll just try the next one in our sequence.

                    return Spawn(pos, rot);
                }
            }

            obj.transform.position = pos;
            obj.transform.rotation = rot;
            obj.SetActive(true);
            return obj;

        }

        // Return an object to the inactive pool.
        public void Despawn(GameObject obj)
        {
            obj.SetActive(false);

            // Since Stack doesn't have a Capacity member, we can't control
            // the growth factor if it does have to expand an internal array.
            // On the other hand, it might simply be using a linked list 
            // internally.  But then, why does it allow us to specify a size
            // in the constructor? Maybe it's a placebo? Stack is weird.
            inactive.Push(obj);
        }

    }

    class PoolMember : MonoBehaviour
    {
        public Pool myPool;
    }

    // All of our pools
    static Dictionary<GameObject, Pool> pools;

    /// <summary>
    /// Initialize our dictionary.
    /// </summary>
    static void Init(GameObject prefab = null, int qty = DEFAULT_POOL_SIZE)
    {
        if (pools == null)
        {
            pools = new Dictionary<GameObject, Pool>();
        }
        if (prefab != null && pools.ContainsKey(prefab) == false)
        {
            pools[prefab] = new Pool(prefab, qty);
        }
    }

    static public void Preload(GameObject prefab, int qty = 1)
    {
        Init(prefab, qty);

        // Make an array to grab the objects we're about to pre-spawn.
        GameObject[] obs = new GameObject[qty];
        for (int i = 0; i < qty; i++)
        {
            obs[i] = Spawn(prefab, Vector3.zero, Quaternion.identity);
        }

        // Now despawn them all.
        for (int i = 0; i < qty; i++)
        {
            Despawn(obs[i]);
        }
    }

    static public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        Init(prefab);

        return pools[prefab].Spawn(pos, rot);
    }

    /// <summary>
    /// Despawn the specified gameobject back into its pool.
    /// </summary>
    static public void Despawn(GameObject obj)
    {
        PoolMember pm = obj.GetComponent<PoolMember>();
        if (pm == null)
        {
            Debug.Log("Object '" + obj.name + "' wasn't spawned from a pool. Destroying it instead.");
            GameObject.Destroy(obj);
        }
        else
        {
            pm.myPool.Despawn(obj);
        }
    }

}
