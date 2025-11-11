using UnityEngine;

public class ContainerVisualisation : MonoBehaviour
{
    [SerializeField]
    private Transform ground;

    [SerializeField]
    private Transform firstSideX;

    [SerializeField]
    private Transform secondSideX;

    [SerializeField]
    private Transform firstSideZ;

    [SerializeField]
    private Transform secondSideZ;

    public void UpdateVisualization(Vector3 containerSize)
    {
        var halfX = containerSize.x / 2;
        var halfY = containerSize.y / 2;
        var halfZ = containerSize.z / 2;

        ground.localScale = new Vector3(containerSize.x, 1f, containerSize.z);
        ground.position = new Vector3(halfX, -0.5f, halfZ);

        firstSideX.position  = new Vector3(halfX, halfY, containerSize.z);
        secondSideX.position = new Vector3(halfX, halfY, 0);
        firstSideX.localScale = secondSideX.localScale = new Vector3(containerSize.x / 10f, 1, containerSize.y / 10);

        firstSideZ.position = new Vector3(0, halfY, halfZ);
        secondSideZ.position = new Vector3(containerSize.x, halfY, halfZ);
        firstSideZ.localScale = secondSideZ.localScale = new Vector3(containerSize.z / 10, 1, containerSize.y / 10);
    }
}
