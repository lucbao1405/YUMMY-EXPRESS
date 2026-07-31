using UnityEngine;

/// <summary>
/// Base class cho Singleton pattern trong Unity.
/// Tự động xử lý việc kiểm tra Instance duplicate và Destroy nếu đã tồn tại.
/// </summary>
/// <typeparam name="T">Type của class kế thừa (phải là MonoBehaviour)</typeparam>
public abstract class SingletonBehaviour<T> : MonoBehaviour where T : Component
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this as T;
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this as T)
        {
            Instance = null;
        }
    }
}

