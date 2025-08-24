using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 손님 생성/퇴장을 관리하는 매니저
public class CustomerManager : SceneSingleton<CustomerManager>
{
    [Header("설정")]
    [Tooltip("손님이 앉을 수 있는 모든 의자의 위치(Transform) 리스트")]
    [SerializeField] private List<Transform> chairTransforms;

    [Tooltip("생성할 손님 프리팹 (Customer.cs가 붙어있어야 함)")]
    [SerializeField] private GameObject customerPrefab;

    [Tooltip("의자 위치 기준으로 손님이 처음 생성될 위치 오프셋")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(1, 1, 0);

    [Header("캐릭터 데이터베이스")]
    [Tooltip("게임에 등장할 모든 CustomerData 파일을 여기에 등록.")]
    [SerializeField] private List<CustomerData> customerDatabase;


    // 어떤 의자(int)에 어떤 손님(Customer)이 앉아있는지 기록
    private static Dictionary<int, Customer> seatedCustomers = new Dictionary<int, Customer>();
    private Dictionary<string, CustomerData> customerDataDict;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitializeOnPlay()
    {
        seatedCustomers = new Dictionary<int, Customer>();
    }


    void Update()   // TESTTESTTESTTEST
    {
        if (Keyboard.current == null) return;

        // 숫자 키로 손님 등장
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SpawnCustomerAt(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SpawnCustomerAt(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SpawnCustomerAt(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) SpawnCustomerAt(3);

        // Shift + 숫자 키로 손님 퇴장
        if (Keyboard.current.leftShiftKey.isPressed)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) ExitCustomerAt(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) ExitCustomerAt(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) ExitCustomerAt(2);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) ExitCustomerAt(3);
        }
    }

    // 캐릭터 스폰 시에는 이 함수를 사용합니다.
    public void SpawnCustomerAt(int chairIndex)
    {
        if (chairIndex < 0 || chairIndex >= chairTransforms.Count) { Debug.Log($"잘못된 의자 번호입니다: {chairIndex}"); return; }
        if (customerPrefab == null) { Debug.Log("손님 프리팹이 설정되지 않았습니다."); return; }
        
        if (seatedCustomers.ContainsKey(chairIndex))
        {
            Debug.Log($"{chairIndex}번 의자에는 이미 손님이 있습니다.");
            return;
        }

        Transform targetChair = chairTransforms[chairIndex];
        Vector3 spawnPosition = targetChair.position + spawnOffset;
        
        GameObject customerObject = Instantiate(customerPrefab, spawnPosition, Quaternion.identity);
        Customer customer = customerObject.GetComponent<Customer>();

        if (customer != null)
        {
            seatedCustomers[chairIndex] = customer;
            customer.GoToTarget(targetChair);
        }
    }

    // 손님의 포즈를 변경할 때 이 함수.
    public void ChangeCustomerPose(int chairIndex, string poseName)
    {
        if (seatedCustomers.TryGetValue(chairIndex, out Customer customer))
        {
            customer.ChangePose(poseName);
        }
        else
        {
            Debug.Log($"{chairIndex}번 의자에는 손님이 없습니다.");
        }
    }

    // 캐릭터 퇴장 시에는 이 함수를 사용합니다.
    public void ExitCustomerAt(int chairIndex)
    {
        if (seatedCustomers.TryGetValue(chairIndex, out Customer customerToExit) && customerToExit != null)
        {
            customerToExit.Exit();
            seatedCustomers.Remove(chairIndex);
        }
        else
        {
            Debug.Log($"{chairIndex}번 의자에는 손님이 없습니다.");
        }
    }
}
