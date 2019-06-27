using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public List<Transform> exitPositions = new List<Transform>();
    public List<RetreatActionBase> retreatActions = new List<RetreatActionBase>();
    public static LevelManager instance;

    public static LevelManager GEtINstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }
    public RetreatActionBase ReturnRetreatPosition(Transform psTransform)
    {
        RetreatActionBase retval = null;
        for (int i = 0; i < retreatActions.Count; i++)
        {
            if (psTransform == retreatActions[i].retreatPosition)
            {
                retval = retreatActions[i];
                break;
            }
        }
        return retval;
    }
}
[System.Serializable]
public class RetreatActionBase
{
    public bool inUSe;
    public bool visited;
    public Transform retreatPosition;
    public List<AI> reinforceMents = new List<AI>();
}
