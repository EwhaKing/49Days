using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Affinity/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("기본 정보")]
    public int fixedIndex; // 고정 인덱스 (1부터 시작, slot 에 고정되는 위치.)
    public string characterName;       // 캐릭터 이름

    [Header("이미지")]
    public Sprite slotImage;           // 왼쪽 캐릭터 슬롯에서 쓰는 이미지
    public Sprite profileImage;        // 오른쪽 프로필 요약에서 쓰는 이미지

    public bool hasMet;                // 만난 여부
    [TextArea] public string profileText; // 프로필 요약

    [Header("호감도 및 선호")]
    [Range(0, 20)] public int affinity; // 호감도 (0.5 단위 = 0~20)
    public string likes;
    public string dislikes;
}
