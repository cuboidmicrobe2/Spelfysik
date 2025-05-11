using UnityEngine;
using UnityEngine.Rendering;

public class OnInteract : MonoBehaviour, IInteractable
{
    public void Hovering()
    {
        Debug.Log("Stooop looking at me bitch!");
    }

    public void Interact()
    {
        //Player.Instance.showScope = true;
    }
}