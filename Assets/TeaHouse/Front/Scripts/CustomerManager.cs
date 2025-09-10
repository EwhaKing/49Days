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
    [SerializeField] private List<CharacterData> customerDatabase;

    // 현재 씬에 존재하는 손님 인스턴스 관리 (Key: 캐릭터 이름, Value: Customer 컴포넌트)
    private Dictionary<string, Customer> seatedCustomers = new Dictionary<string, Customer>();
    private Dictionary<string, CharacterData> customerDataDict;


    protected override void Awake()
    {
        base.Awake();
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

        foreach (var entry in OrderManager.Instance.seatedCustomerInfo)
        {
            int chairIndex = entry.Key;
            string characterName = entry.Value;
            SpawnSatCustomer(characterName, chairIndex);
        }
    }


    // 캐릭터 스폰 시에는 이 함수를 사용합니다.
    public Customer SpawnCustomer(string characterName, int chairIndex)
    {
        if (!customerDataDict.TryGetValue(characterName, out CharacterData dataToSpawn))
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
            OrderManager.Instance.seatedCustomerInfo.Add(chairIndex, characterName); // static 변수에 저장
            return customer;
        }
        return null;
    }

    public Customer SpawnSatCustomer(string characterName, int chairIndex)
    {
        if (!customerDataDict.TryGetValue(characterName, out CharacterData dataToSpawn))
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
        Vector3 spawnPosition = targetChair.position;
        GameObject customerObject = Instantiate(customerPrefab, spawnPosition, Quaternion.identity);
        Customer customer = customerObject.GetComponent<Customer>();
        customerObject.name = characterName;

        if (customer != null)
        {
            customer.Initialize(dataToSpawn);
            customer.PlaceAt(targetChair);
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
            int chairIndex = customerToExit.ChairIndex;
            if (OrderManager.Instance.seatedCustomerInfo.ContainsKey(chairIndex))
            {
                OrderManager.Instance.seatedCustomerInfo.Remove(chairIndex); // static 변수에서 삭제
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
