// using UnityEngine;
// using System.Collections.Generic;


// // 스프라이트 세트로 저장
// [System.Serializable]
// public class CharacterPose
// {
//     [Tooltip("포즈(감정)에 붙힐 이름")]
//     public string poseName;

//     [Tooltip("몸통 부분에 해당하는 스프라이트")]
//     public Sprite bodySprite;

//     [Tooltip("뜬 눈 스프라이트")]
//     public Sprite eyesOpenSprite;

//     [Tooltip("눈을 깜빡일 때 사용할 감은 눈 스프라이트")]
//     public Sprite eyesClosedSprite;
// }

// // 각 캐릭터의 Front에서의 데이터를 담는 오브젝트
// // 따로 더 넣을 게 있나...? 상의해야 함.
// [CreateAssetMenu(fileName = "New Customer Data", menuName = "Tea House/Customer Data")]
// public class CustomerData : ScriptableObject
// {
//     [Header("캐릭터 기본 정보")]
//     [Tooltip("캐릭터 이름")]
//     public string characterName;

//     [Header("포즈 세트(감정 별)")]
//     [Tooltip("이 캐릭터가 가질 수 있는 모든 감정 표현의 리스트")]
//     public List<CharacterPose> poses;
// }
