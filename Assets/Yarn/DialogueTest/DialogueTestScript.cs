using UnityEngine;
using Yarn.Unity;

public class NPC : MonoBehaviour
{
    public DialogueRunner runner;

    void Start()
    {
        var runner = FindObjectOfType<DialogueRunner>();
        runner?.StartDialogue("NpcStart"); // NpcStart는 .yarn 파일의 title
    }
}
