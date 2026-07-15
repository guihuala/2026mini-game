using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ScenePortal : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt = "E  前往出口（自动保存）";
    [SerializeField] private string destinationScene = "PaperDiorama";
    [SerializeField] private string destinationSpawnPoint = "Return";
    [SerializeField] private string requiredFlag = "greybox.gate_open";
    [SerializeField] private bool completesDemo;
    [SerializeField] private DemoCompletionController demoCompletion;
    private bool loading;

    public bool CanInteract(PlayerInteractor interactor) => !loading && DialogueRuntimeState.EvaluateCondition("flag:" + requiredFlag);
    public string GetInteractionPrompt(PlayerInteractor interactor) => prompt;

    public void Interact(PlayerInteractor interactor)
    {
        if (!CanInteract(interactor)) return;
        loading = true;
        ExplorationSession.SavePlayer(interactor.GetComponent<PlayerMotor>(), destinationSpawnPoint);
        if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();
        if (completesDemo && demoCompletion != null)
        {
            demoCompletion.Show();
            loading = false;
            return;
        }
        StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(destinationScene);
        while (!operation.isDone) yield return null;
    }
}
