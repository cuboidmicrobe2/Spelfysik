using UnityEngine;

public class OnInteract : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("It work!");
    }
}