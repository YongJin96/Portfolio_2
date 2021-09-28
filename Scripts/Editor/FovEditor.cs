using UnityEngine;
using UnityEditor;

public class FovEditor : Editor
{
    private void OnSceneGUI()
    {
        EnemyFov enemyFov = (EnemyFov)target;

        Vector3 FromAnglePos = enemyFov.CirclePoint(-enemyFov.ViewAngle * 0.5f);

        Handles.color = new Color(1, 1, 1, 0.2f);

        Handles.DrawWireDisc(enemyFov.transform.position, Vector3.up, enemyFov.ViewRange);

        Handles.DrawSolidArc(enemyFov.transform.position, Vector3.up, FromAnglePos, enemyFov.ViewAngle, enemyFov.ViewRange);

        Handles.Label(enemyFov.transform.position + (enemyFov.transform.forward * 2f), enemyFov.ViewAngle.ToString());
    }
}
