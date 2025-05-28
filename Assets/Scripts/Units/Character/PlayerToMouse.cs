/* Original author name: Adam Turcotte
 * Creation date: 2025/04/25
 * Goal: Touver le vecteur entre le jouer et la souris selon le screen
 * Modification listing:
 * -
 */
using UnityEngine;

public static class PlayerToMouse
{
    /// <summary>
    /// Retourne un vecteur de direction de l'origine du joueur vers la souris.
    /// Lors d'un clique de la souris sur le <c>Screen</c> le converti selon la <c>PlayerCamera</c>.
    /// </summary>
    public static Vector2 Get(GameObject screen,GameObject playerCamera)
    {
        Vector3 screenToMouse = Input.mousePosition - screen.transform.position;
        Vector2 screenToMouse2d = new Vector2(screenToMouse.x,screenToMouse.y);
        Vector2 screenScale2d = new Vector2(screen.transform.localScale.x, screen.transform.localScale.y);
        Vector2 playerCamScale = new Vector2(playerCamera.transform.localScale.x, playerCamera.transform.localScale.y);
        return screenToMouse2d / screenScale2d*playerCamScale;
    }
}
