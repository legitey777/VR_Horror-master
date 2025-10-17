using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


/*
 * ���� 
 * 1. Get �Լ��� ù��° ���ڸ� false �� �ָ� �ش� �������� ����ϴ� Ǯ�� �����.
 *    �̶����� Reset() �Լ��� �̿����ָ� ������ Ǯ���� �������.(�� �����ִ� �� ��ũ��Ʈ���� �������ִ°���õ)
 *    
 * 2. Get �Լ��� ù��° ���ڸ� True�� �ָ� Dont Destroy Ǯ�� �ǰ� ���̵��� �ϴ���
 *    �����ʹ� ��������ʴ´� ���� ������ �ؾ��� ��Ȳ���ִٸ� ResetDD() �Լ��� �̿��ϸ�ȴ�.
 *    
 * 3. ����� ��ɵ��� ���ֱ��ѵ� �׽�Ʈ �����ʾ����� ������ �߻��ϸ� ��ġ�ų�
 *    �����ڿ��� �����.
 */



public class PoolManager : MonoBehaviour
{
    // ObjectPool ����Ƽ ���� ���� Ǯ ���
    //key�� ������Ʈ�� �̸����� �ϱ�� ����صд�.
    public Dictionary<string, ObjectPool<GameObject>> poolDic; //����� Ǯ
    public Dictionary<string, ObjectPool<GameObject>> uipoolDic; //����� Ǯ
    Dictionary<string, Transform> poolContainer;// Ǯ ���� ���� �����̳�

    Transform poolRoot; // ��� Ǯ���� ���������� ��


    //Dont Destroy Pool

    public Dictionary<string, ObjectPool<GameObject>> ddpoolDic; //����� Ǯ
    public Dictionary<string, ObjectPool<GameObject>> dduipoolDic; //����� Ǯ
    Dictionary<string, Transform> ddpoolContainer;// Ǯ ���� ���� �����̳�
    Transform DontDestroyPoolRoot; // �� ��ȯ�� ���������ʴ� Ǯ

    //UI Ǯ
    Canvas canvasRoot;

    public void erasePoolDicContet(string key , bool isDontDestroy = false)
    {
        if (!isDontDestroy)
        {
            poolDic.Remove(key);
        }
        else
        {
            ddpoolDic.Remove(key);
        }
    }

    public void eraseContainerContet(string key, bool isDontDestroy = false)
    {
        if (!isDontDestroy)
        {
            poolContainer.Remove(key);
        }
        else
        {
            ddpoolContainer.Remove(key);
        }
    }

    private void Awake()
    {
    }

    //Init new Pool Setting
    public void Init()
    {   // ������ �������
        poolDic = new Dictionary<string, ObjectPool<GameObject>>();
        uipoolDic = new Dictionary<string, ObjectPool<GameObject>>();
        poolContainer = new Dictionary<string, Transform>();
        poolRoot = new GameObject("PoolRoot").transform;


        ddpoolDic = new Dictionary<string, ObjectPool<GameObject>>();
        dduipoolDic = new Dictionary<string, ObjectPool<GameObject>>();
        ddpoolContainer = new Dictionary<string, Transform>();

        DontDestroyPoolRoot = new GameObject("DontDestroyPoolRoot").transform;
        DontDestroyPoolRoot.transform.parent = transform;

        canvasRoot = GameManager.Resource.Instantiate<Canvas>("UI/Canvas");
    }

    public void Reset()
    {
        canvasRoot = GameManager.Resource.Instantiate<Canvas>("UI/Canvas");
        uipoolDic = new Dictionary<string, ObjectPool<GameObject>>();

        poolDic = new Dictionary<string, ObjectPool<GameObject>>();
        poolContainer = new Dictionary<string, Transform>();
        poolRoot = new GameObject("PoolRoot").transform;
    }

    public void ResetDD() 
    { // dont destroy pool Reset
        if (DontDestroyPoolRoot != null)
        {
            Destroy(DontDestroyPoolRoot.gameObject);
        }

        canvasRoot = GameManager.Resource.Instantiate<Canvas>("UI/Canvas");
        dduipoolDic = new Dictionary<string, ObjectPool<GameObject>>();

        ddpoolDic = new Dictionary<string, ObjectPool<GameObject>>();
        ddpoolContainer = new Dictionary<string, Transform>();

        DontDestroyPoolRoot = new GameObject("DontDestroyPoolRoot").transform;
        DontDestroyPoolRoot.transform.parent = transform;
    }


    public void CleanUpDontDestroyRoot()
    { // ����ִ� �����̳� ����
        if (DontDestroyPoolRoot != null && DontDestroyPoolRoot.childCount > 0)
        {

            for (int i = 0; i < DontDestroyPoolRoot.childCount; i++)
            {
                var container = DontDestroyPoolRoot.GetChild(i);

                if (container.childCount <= 0)
                {
                    Destroy(container.gameObject);
                }

            }
        }
    }

    public void DestroyContainer(GameObject obj)
    {
        if (DontDestroyPoolRoot != null && DontDestroyPoolRoot.childCount > 0)
        {

            for (int i = 0; i < DontDestroyPoolRoot.childCount; i++)
            {
                var container = DontDestroyPoolRoot.GetChild(i);

                if (container.childCount > 0)
                {
                    if (container.GetChild(0).gameObject == obj)
                    {
                        Destroy(container.gameObject);
                    }
                }


            }
        }
    }



    public T Get<T>(bool IsDontDestroy, T original, Vector3 position, Quaternion rotation, Transform parent, string suffix = "") where T : Object
    {
        if (!IsDontDestroy)
        {

            if (original is GameObject) // gameObject �϶�
            {
                GameObject prefab = original as GameObject;
                string key = prefab.name; // Ű�� ������Ʈ�� �̸�����


                if (suffix != "")
                {
                    key += suffix;
                }
                GameObject obj;

                if (!poolDic.ContainsKey(key)) // �̹� Ű�� �����Ǿ� ����������(�ش��ϴ� �̸��� Ǯ�̾�����)
                    CreatePool(key, prefab); // Ǯ�θ����.

                obj = poolDic[key].Get(); // Ű�� �̹� ������ �����ͼ� ���� ������ ���� ���� Ǯ���� �����ͼ�����


                //�ش��ϴ� ������Ʈ�� �θ�,��ġ,������,�����̼� ����
                obj.transform.parent = parent;
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                return obj as T; //����� ����
            }
            else if (original is Component) // Component �϶� 
            {
                Component component = original as Component;
                string key = component.gameObject.name;// Ű�� �ش� ������Ʈ�� �������ִ� ������Ʈ�� �̸�����

                if (suffix != "")
                {
                    key += suffix;
                }

                GameObject obj;

                if (!poolDic.ContainsKey(key)) // �̹� Ű�� �����Ǿ� ����������(�ش��ϴ� �̸��� Ǯ�̾�����)
                    CreatePool(key, component.gameObject); // Ǯ�θ����.

                obj = poolDic[key].Get(); // Ű�� �̹� ������ �����ͼ� ���� ������ ���� ���� Ǯ���� �����ͼ�����

                //�ش��ϴ� ������Ʈ�� �θ�,��ġ,������,�����̼� ����
                obj.transform.parent = parent;
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                return obj.GetComponent<T>(); // ������� ������Ʈ ����
            }
            else // ���ӿ�����Ʈ���ƴϰ� ������Ʈ�� �ƴҶ� �ƹ��͵�����
            {
                return null;
            }
        }
        else 
        {
            if (original is GameObject) // gameObject �϶�
            {
                GameObject prefab = original as GameObject;
                string key = prefab.name; // Ű�� ������Ʈ�� �̸�����


                if (suffix != "")
                {
                    key += suffix;
                }

                GameObject obj;

                if (!ddpoolDic.ContainsKey(key)) // �̹� Ű�� �����Ǿ� ����������(�ش��ϴ� �̸��� Ǯ�̾�����)
                    DCreatePool(key, prefab); // Ǯ�θ����.

                obj = ddpoolDic[key].Get(); // Ű�� �̹� ������ �����ͼ� ���� ������ ���� ���� Ǯ���� �����ͼ�����


                //�ش��ϴ� ������Ʈ�� �θ�,��ġ,������,�����̼� ����
                obj.transform.parent = parent;
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                return obj as T; //����� ����
            }
            else if (original is Component) // Component �϶� 
            {
                Component component = original as Component;
                string key = component.gameObject.name;// Ű�� �ش� ������Ʈ�� �������ִ� ������Ʈ�� �̸�����

                if (suffix != "")
                {
                    key += suffix;
                }

                GameObject obj;

                if (!ddpoolDic.ContainsKey(key)) // �̹� Ű�� �����Ǿ� ����������(�ش��ϴ� �̸��� Ǯ�̾�����)
                    DCreatePool(key, component.gameObject); // Ǯ�θ����.

                obj = ddpoolDic[key].Get(); // Ű�� �̹� ������ �����ͼ� ���� ������ ���� ���� Ǯ���� �����ͼ�����

                //�ش��ϴ� ������Ʈ�� �θ�,��ġ,������,�����̼� ����
                obj.transform.parent = parent;
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                return obj.GetComponent<T>(); // ������� ������Ʈ ����
            }
            else // ���ӿ�����Ʈ���ƴϰ� ������Ʈ�� �ƴҶ� �ƹ��͵�����
            {
                return null;
            }

        }
    }


    // �Ʒ��� Get �Լ����� ���� �ϳ��� ���龿 �ȳ����� �����ε�
    // �ȳ��� ���� �⺻������ �־��ش�.

    public T Get<T>(bool IsDontDestroy, T original, Vector3 position, Quaternion rotation, string suffix) where T : Object
    {
        return Get<T>(IsDontDestroy, original, position, rotation, null, suffix);
    }


    public T Get<T>(bool IsDontDestroy, T original, Vector3 position, Quaternion rotation) where T : Object
    {
        return Get<T>(IsDontDestroy, original, position, rotation, null);
    }

    public T Get<T>(bool IsDontDestroy, T original, Transform parent) where T : Object
    {
        return Get<T>(IsDontDestroy, original, Vector3.zero, Quaternion.identity, parent);
    }

    public T Get<T>(bool IsDontDestroy, T original) where T : Object
    {
        return Get<T>(IsDontDestroy, original, Vector3.zero, Quaternion.identity, null);
    }



    //Ǯ ���� ���� / ������
    //Ǯ�� ������ 
    public bool Release<T>(T instance , bool isDonDestroy = false) where T : Object
    {
        if (!isDonDestroy) 
        {
            if (instance is GameObject)
            {
                GameObject go = instance as GameObject;
                string key = go.name;

                if (!poolDic.ContainsKey(key)) // �ش��ϴ� key �� Ǯ�������� �翬�� ������ ����
                    return false;

                poolDic[key].Release(go); // ���⿡ Release �� ����Ƽ�� ObjectPool ������ �� ���
                return true;
            }
            else if (instance is Component)
            {
                Component component = instance as Component;
                string key = component.gameObject.name;

                if (!poolDic.ContainsKey(key))// �ش��ϴ� key �� Ǯ�������� �翬�� ������ ����
                    return false;

                poolDic[key].Release(component.gameObject); // ������Ʈ�� ������Ʈ�� �������ִ� ������Ʈ�� �������Ѵ�.
                return true;
            }
            else
            {
                return false;
            }

        }
        else
        {
            if (instance is GameObject)
            {
                GameObject go = instance as GameObject;
                string key = go.name;

                if (!ddpoolDic.ContainsKey(key)) // �ش��ϴ� key �� Ǯ�������� �翬�� ������ ����
                    return false;

                ddpoolDic[key].Release(go); // ���⿡ Release �� ����Ƽ�� ObjectPool ������ �� ���
                return true;
            }
            else if (instance is Component)
            {
                Component component = instance as Component;
                string key = component.gameObject.name;

                if (!ddpoolDic.ContainsKey(key))// �ش��ϴ� key �� Ǯ�������� �翬�� ������ ����
                    return false;

                ddpoolDic[key].Release(component.gameObject); // ������Ʈ�� ������Ʈ�� �������ִ� ������Ʈ�� �������Ѵ�.
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    //Ǯ�ȿ� �ش��ϴ� ������Ʈ�� �ִ� �� Ȯ���ϴ� �Լ�
    public bool IsContain<T>(T original, bool isDonDestroy = false) where T : Object
    {
        if (!isDonDestroy)
        {
            if (original is GameObject)
            {
                GameObject prefab = original as GameObject;
                string key = prefab.name;

                if (poolDic.ContainsKey(key))
                    return true;
                else
                    return false;

            }
            else if (original is Component)
            {
                Component component = original as Component;
                string key = component.gameObject.name;

                if (poolDic.ContainsKey(key))
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
        else 
        {
            if (original is GameObject)
            {
                GameObject prefab = original as GameObject;
                string key = prefab.name;

                if (ddpoolDic.ContainsKey(key))
                    return true;
                else
                    return false;

            }
            else if (original is Component)
            {
                Component component = original as Component;
                string key = component.gameObject.name;

                if (ddpoolDic.ContainsKey(key))
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
      
    }

    //Ǯ�� ������ ����� �Լ�
    private void CreatePool(string key, GameObject prefab)
    {
        GameObject root = new GameObject();
        root.gameObject.name = $"{key}Container";

        root.transform.parent = poolRoot;
        poolContainer.Add(key, root.transform);

        //����Ƽ ���� ObjectPool �� ���鶧 
        // ���鶧,�����ö�,�������Ҷ�,���ﶧ �� �׼��� �־�����Ѵ�.
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject obj = Instantiate(prefab);
                obj.gameObject.name = key;
                return obj;
            },
            actionOnGet: (GameObject obj) =>
            {
                obj.gameObject.SetActive(true);
                obj.transform.parent = null;
            },
            actionOnRelease: (GameObject obj) =>
            {
                obj.gameObject.SetActive(false);
                obj.transform.parent = poolContainer[key];
            },
            actionOnDestroy: (GameObject obj) =>
            {
                Destroy(obj);
            }
            );


        poolDic.Add(key, pool);

    }

    //DonDestroy Pool Create

    private void DCreatePool(string key, GameObject prefab)
    {
        GameObject root = new GameObject();
        root.gameObject.name = $"{key}DContainer";

        root.transform.parent = DontDestroyPoolRoot;
        ddpoolContainer.Add(key, root.transform);

        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject obj = Instantiate(prefab);
                obj.gameObject.name = key;
                return obj;
            },
            actionOnGet: (GameObject obj) =>
            {
                obj.gameObject.SetActive(true);
                obj.transform.parent = null;
            },
            actionOnRelease: (GameObject obj) =>
            {
                obj.gameObject.SetActive(false);
                obj.transform.parent = ddpoolContainer[key];
            },
            actionOnDestroy: (GameObject obj) =>
            {
                Destroy(obj);
            }
            );
        ddpoolDic.Add(key, pool);
    }



    // UI Ǯ
    public T GetUI<T>(T original, Vector3 position, string suffix = "") where T : Object
    {
        if (original is GameObject)
        {
            GameObject prefab = original as GameObject;
            string key = prefab.name;

            if (suffix != "")
            {
                key += suffix;
            }


            if (!uipoolDic.ContainsKey(key))
                CreateUIPool(key, prefab);

            GameObject obj = uipoolDic[key].Get();
            obj.transform.position = position;
            return obj as T;
        }
        else if (original is Component)
        {
            Component component = original as Component;
            string key = component.gameObject.name;

            if (suffix != "")
            {
                key += suffix;
            }

            if (!uipoolDic.ContainsKey(key))
                CreateUIPool(key, component.gameObject);

            GameObject obj = uipoolDic[key].Get();
            obj.transform.position = position;
            return obj.GetComponent<T>();
        }
        else
        {
            return null;
        }
    }

    public T GetUI<T>(T original, Transform parent, string suffix = "") where T : Object
    {
        if (original is GameObject)
        {
            GameObject prefab = original as GameObject;
            string key = prefab.name;

            if (suffix != "")
            {
                key += suffix;
            }

            if (!uipoolDic.ContainsKey(key))
                CreateUIPool(key, prefab);

            GameObject obj = uipoolDic[key].Get();
            //obj.transform.position = position;
            obj.transform.SetParent(parent, false);
            return obj as T;
        }
        else if (original is Component)
        {
            Component component = original as Component;
            string key = component.gameObject.name;

            if (suffix != "")
            {
                key += suffix;
            }

            if (!uipoolDic.ContainsKey(key))
                CreateUIPool(key, component.gameObject);

            GameObject obj = uipoolDic[key].Get();
            //obj.transform.position = position;
            obj.transform.SetParent(parent, false);
            return obj.GetComponent<T>();
        }
        else
        {
            return null;
        }
    }

    public T GetUI<T>(T original, string suffix = "") where T : Object
    {
        if (original is GameObject)
        {
            GameObject prefab = original as GameObject;
            string key = prefab.name;

            if (suffix != "")
            {
                key += suffix;
            }

            if (!uipoolDic.ContainsKey(key))
                CreateUIPool(key, prefab);

            GameObject obj = uipoolDic[key].Get();
            return obj as T;
        }
        else if (original is Component)
        {
            Component component = original as Component;
            string key = component.gameObject.name;

            if (suffix != "")
            {
                key += suffix;
            }

            if (!uipoolDic.ContainsKey(key))
                CreateUIPool(key, component.gameObject);

            GameObject obj = uipoolDic[key].Get();
            return obj.GetComponent<T>();
        }
        else
        {
            return null;
        }
    }

    public bool ReleaseUI<T>(T instance) where T : Object
    {
        if (instance is GameObject)
        {
            GameObject go = instance as GameObject;
            string key = go.name;

            if (!uipoolDic.ContainsKey(key))
                return false;

            uipoolDic[key].Release(go);
            return true;
        }
        else if (instance is Component)
        {
            Component component = instance as Component;
            string key = component.gameObject.name;

            if (!uipoolDic.ContainsKey(key))
                return false;

            uipoolDic[key].Release(component.gameObject);
            return true;
        }
        else
        {
            return false;
        }
    }

    // UI �� ĵ������ �־�ΰ� �ٽû��� �̵���Ű�°�
    // ��ġ���� ���̻�������.
    private void CreateUIPool(string key, GameObject prefab)
    {
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject obj = Instantiate(prefab);
                obj.gameObject.name = key;
                return obj;
            },
            actionOnGet: (GameObject obj) =>
            {
                obj.gameObject.SetActive(true);
            },
            actionOnRelease: (GameObject obj) =>
            {
                obj.gameObject.SetActive(false);
                obj.transform.SetParent(canvasRoot.transform, false);
            },
            actionOnDestroy: (GameObject obj) =>
            {
                Destroy(obj);
            }
            );
        uipoolDic.Add(key, pool);
    }


    public bool IsContainUI<T>(T original) where T : Object
    {
        if (original is GameObject)
        {
            GameObject prefab = original as GameObject;
            string key = prefab.name;

            if (uipoolDic.ContainsKey(key))
                return true;
            else
                return false;

        }
        else if (original is Component)
        {
            Component component = original as Component;
            string key = component.gameObject.name;

            if (uipoolDic.ContainsKey(key))
                return true;
            else
                return false;
        }
        else
        {
            return false;
        }
    }
}
