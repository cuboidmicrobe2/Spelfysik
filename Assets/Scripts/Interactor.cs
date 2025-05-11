using UnityEngine;

interface IInteractable
{
    public void Hovering();
    public void Interact();
}

public class Interactor : MonoBehaviour
{
    public Transform InteractorSource;
    public float InteractRange;

    private void Update()
    {
        Ray ray = new Ray(InteractorSource.position, InteractorSource.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, InteractRange))
        {
            if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj))
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactObj.Interact();
                }
                //interactObj.Hovering();   // om vi vill visa en icon för 'E' idk (inte viktigt iaf)
            }
        }
    }
}