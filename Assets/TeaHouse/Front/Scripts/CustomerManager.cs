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



    [Header("이펙트 설정")]
    [Tooltip("호감도 상승/하락 시 나타날 하트 이펙트 프리팹")]
    [SerializeField] private GameObject heartEffectPrefab;
    [Tooltip("손님 머리 위로 이펙트가 나타날 위치 오프셋")]
    [SerializeField] private Vector3 heartOffset = new Vector3(0, 0.5f, 0);

    [Tooltip("하트 이펙트가 생성될 부모 Canvas의 RectTransform")]
    [SerializeField] private RectTransform mainCanvasRectTransform;



    [Header("캐릭터 데이터베이스")]
    [Tooltip("게임에 등장할 모든 CustomerData 파일을 여기에 등록.")]
    [SerializeField] private List<CustomerData> customerDatabase;

    // 씬 전환 시에도 유지될 착석 정보(Key: 의자 인덱스, Value: 캐릭터 이름)
    private static Dictionary<int, string> seatedCustomerInfo;
    // 현재 씬에 존재하는 손님 인스턴스 관리 (Key: 캐릭터 이름, Value: Customer 컴포넌트)
    private static Dictionary<string, Customer> seatedCustomers = new Dictionary<string, Customer>();
    private Dictionary<string, CustomerData> customerDataDict;


    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        if (customerDatabase != null)
        {
            customerDataDict = customerDatabase.ToDictionary(data => data.characterName);
        }
    }

    void Start()
    {
        foreach (var customer in seatedCustomers.Values)
        {
            if (customer != null) Destroy(customer.gameObject);
        }
        seatedCustomers.Clear();

        foreach (var entry in seatedCustomerInfo)
        {
            int chairIndex = entry.Key;
            string characterName = entry.Value;
            SpawnCustomer(characterName, chairIndex);
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

        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            HeartUp("키루스");
        }
        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            HeartDown("키루스");
        }
    }

    // 캐릭터 스폰 시에는 이 함수를 사용합니다.
    public Customer SpawnCustomer(string characterName, int chairIndex)
    {
        if (seatedCustomerInfo == null) {
            seatedCustomerInfo = new Dictionary<int, string>();
        }
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
        customerObject.name = characterName;


        if (customer != null)
        {
            customer.Initialize(dataToSpawn);
            customer.GoToTarget(targetChair);
            seatedCustomers[characterName] = customer;
            seatedCustomers[characterName] = customer;
            seatedCustomerInfo[chairIndex] = characterName; // static 변수에 저장
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
            int chairIndex = customerToExit.ChairIndex;
            if (seatedCustomerInfo.ContainsKey(chairIndex))
            {
                seatedCustomerInfo.Remove(chairIndex); // static 변수에서 삭제
            }
            customerToExit.Exit();
            seatedCustomers.Remove(characterName);
        }
        else
        {
            Debug.Log($"{characterName} 손님이 없습니다.");
        }
    }


    public void HeartUp(string characterName)
    {
        if (seatedCustomers.TryGetValue(characterName, out Customer customer))
        {
            ShowHeartEffect(customer, HeartEffect.HeartType.Full);
        }
        else
        {
            Debug.LogWarning($"{characterName} 손님이 앉아있지 않아 호감도 이펙트를 표시할 수 없습니다.");
        }
    }

    public void HeartDown(string characterName)
    {
        if (seatedCustomers.TryGetValue(characterName, out Customer customer))
        {
            ShowHeartEffect(customer, HeartEffect.HeartType.Broken);
        }
        else
        {
            Debug.LogWarning($"{characterName} 손님이 앉아있지 않아 호감도 이펙트를 표시할 수 없습니다.");
        }
    }
    
        private void ShowHeartEffect(Customer customer, HeartEffect.HeartType type)
    {
        if (heartEffectPrefab == null || mainCanvasRectTransform == null || customer == null)
        {
            Debug.LogError("Heart Effect Prefab 또는 Main Canvas가 설정되지 않았습니다!");
            return;
        }

        GameObject effectObject = Instantiate(heartEffectPrefab, mainCanvasRectTransform);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(customer.transform.position + heartOffset);
        effectObject.transform.position = screenPos;
        HeartEffect heartEffect = effectObject.GetComponent<HeartEffect>();
        if (heartEffect != null)
        {
            heartEffect.ShowEffect(type);
        }
    }
    

}
