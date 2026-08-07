using UnityEngine;

public class CustomerManager : SingletonBehaviour<CustomerManager>
{
    // Manager này chỉ liên quan đến khách: spawn, bỏ đi, và đếm số lượng khách bị mất.
    // GameManager chỉ nhận kết quả từ đây để quyết định thắng/thua.
    #region Fields

    [Header("Game Flow")]
    [SerializeField] private CustomerSpawner customerSpawner;

    private int lostCustomerCount = 0;

    #endregion

    #region Properties

    public CustomerSpawner CustomerSpawner => customerSpawner;
    public int LostCustomerCount => lostCustomerCount;

    #endregion

    protected override void Awake()
    {
        base.Awake();

        if (customerSpawner == null)
        {
            Debug.LogWarning("CustomerManager: Vui lòng gán CustomerSpawner trong Inspector.", this);
        }
    }

    public void ResetState()
    {
        lostCustomerCount = 0;
    }

    public void StopSpawning()
    {
        if (customerSpawner != null)
        {
            customerSpawner.StopSpawning();
        }
    }

    public void ResumeSpawning()
    {
        if (customerSpawner != null)
        {
            customerSpawner.StartSpawning();
        }
    }

public void NotifyCustomerLeft()
    {
        lostCustomerCount++;

        // Thông báo cho ScoreManager ghi nhận khách giận bỏ về (-3 điểm, tăng angryCustomersCount, reset combo).
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnCustomerLeftAngry();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCustomerLost(lostCustomerCount);
        }
    }
}
