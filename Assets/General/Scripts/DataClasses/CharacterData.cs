using UnityEngine;
using System.Collections.Generic;

//customerdata에서 가져온 요소들
// 스프라이트 세트로 저장
[System.Serializable]
public class CharacterPose
{
    [Tooltip("포즈(감정)에 붙힐 이름")]
    public string poseName;

    [Tooltip("몸통 부분에 해당하는 스프라이트")]
    public Sprite bodySprite;

    [Tooltip("뜬 눈 스프라이트")]
    public Sprite eyesOpenSprite;

    [Tooltip("눈을 깜빡일 때 사용할 감은 눈 스프라이트")]
    public Sprite eyesClosedSprite;
}


[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("기본 정보")]
    public int fixedIndex; // 고정 인덱스 (0부터 시작, slot 에 고정되는 위치.)
    public string characterName;       // 캐릭터 이름

    [Header("이미지")]
    public Sprite slotImage;           // 왼쪽 캐릭터 슬롯에서 쓰는 이미지
    public Sprite profileImage;        // 오른쪽 프로필 요약에서 쓰는 이미지
    [TextArea] public string profileText; // 프로필 요약


    [Header("호감도 및 선호")]
    public string likes;
    public string dislikes;


    [Header("변화하는 값")]
    public bool hasMet;                // 만난 여부 (게임 첫 실행 시에만 쓰임.)
    [Range(0, 100)] public int affinity; // 호감도 (0.5 단위 = 0~20), (변화하는 요소, 게임 첫 실행 시에만 쓰임.)\


    //customerdata에서 가져온 요소
    [Header("포즈 세트(감정 별)")]
    [Tooltip("이 캐릭터가 가질 수 있는 모든 감정 표현의 리스트")]
    public List<CharacterPose> poses;
}
