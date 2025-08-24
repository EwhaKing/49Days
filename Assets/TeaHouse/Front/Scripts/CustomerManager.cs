using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;


// 손님 생성/퇴장을 관리하는 매니저
public class CustomerManager : SceneSingleton<CustomerManager>
{
    [Header("설정")]
    [SerializeField] private List<Transform> chairTransforms;
    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(1, 1, 0);

    [Header("캐릭터 데이터베이스")]
    [Tooltip("게임에 등장할 모든 CustomerData 파일을 여기에 등록.")]
    [SerializeField] private List<CustomerData> customerDatabase;


    // 어떤 의자(int)에 어떤 손님(Customer)이 앉아있는지 기록
    private static Dictionary<string, Customer> seatedCustomers;
    private Dictionary<string, CustomerData> customerDataDict;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitializeOnPlay()
    {
        seatedCustomers = new Dictionary<string, Customer>();
    }

    protected override void Awake()
    {
        base.Awake();
        if (customerDatabase != null)
        {
            customerDataDict = customerDatabase.ToDictionary(data => data.characterName);
        }
    }
    void Update()   // TESTTESTTESTTEST
    {
        if (Keyboard.current == null) return;

        // 숫자 키로 손님 등장
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SpawnCustomer("키루스", 0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SpawnCustomer("키루스", 1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SpawnCustomer("나란", 2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) SpawnCustomer("나란", 3);

        // Shift + 숫자 키로 손님 퇴장
        if (Keyboard.current.leftShiftKey.isPressed)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) ExitCustomerAt("키루스");
            if (Keyboard.current.digit2Key.wasPressedThisFrame) ExitCustomerAt("키루스");
            if (Keyboard.current.digit3Key.wasPressedThisFrame) ExitCustomerAt("나란");
            if (Keyboard.current.digit4Key.wasPressedThisFrame) ExitCustomerAt("나란");
        }
    }

    // 캐릭터 스폰 시에는 이 함수를 사용합니다.
    public Customer SpawnCustomer(string characterName, int chairIndex)
    {
        if (!customerDataDict.TryGetValue(characterName, out CustomerData dataToSpawn))
        {
            Debug.Log($"'{characterName}' 이름을 가진 캐릭터 데이터를 찾을 수 없습니다.");
            return null;
        }
        if (seatedCustomers.ContainsKey(characterName))
        {
            Debug.Log($"{chairIndex}번 의자에는 이미 손님이 있습니다.");
            return null;
        }

        Transform targetChair = chairTransforms[chairIndex];
        Vector3 spawnPosition = targetChair.position + spawnOffset;
        GameObject customerObject = Instantiate(customerPrefab, spawnPosition, Quaternion.identity);
        Customer customer = customerObject.GetComponent<Customer>();

        if (customer != null)
        {
            customer.Initialize(dataToSpawn);
            customer.GoToTarget(targetChair);
            seatedCustomers[characterName] = customer;
            return customer;
        }
        return null;
    }

    // 손님의 포즈를 변경할 때 이 함수.
    public void ChangeCustomerPose(string characterName, string poseName)
    {
        if (seatedCustomers.TryGetValue(characterName, out Customer customer))
        {
            customer.ChangePose(poseName);
            // "무표정"만 있음...
        }
        else
        {
            Debug.Log($"{characterName}");
        }
    }

    // 캐릭터 퇴장 시에는 이 함수를 사용합니다.
    public void ExitCustomerAt(string characterName)
    {
        if (seatedCustomers.TryGetValue(characterName, out Customer customerToExit) && customerToExit != null)
        {
            customerToExit.Exit();
            seatedCustomers.Remove(characterName);
        }
        else
        {
            Debug.Log($"{characterName}번 의자에는 손님이 없습니다.");
        }
    }
}
