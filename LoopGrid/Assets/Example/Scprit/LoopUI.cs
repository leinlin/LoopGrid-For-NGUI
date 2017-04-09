using UnityEngine;
using System.Collections.Generic;

public class LoopUI : MonoBehaviour {

    private List<int> m_testList = new List<int>(100);
    private Dictionary<int, List<int>> m_testDict = new Dictionary<int, List<int>>();

    private UILoopTitleGrid m_oneLoopGrid;
    private UILoopTitleGrid m_mulLoopGrid;
    private UILoopTitleGrid m_dictLoopGrid;
    void Awake() {
        for (int i = 1; i <= 100; i++) {
            m_testList.Add(i);
        }

        for (int i = 1; i <= 100; i++) {
            List<int> list = new List<int>(5);
            for (int j = 1; j <= 5; j++) {
                list.Add(j);
            }
            m_testDict.Add(i, list);
        }

        m_oneLoopGrid = transform.Find("one_row/bg/Scroll View/LoopTitleGrid").GetComponent<UILoopTitleGrid>();
        m_mulLoopGrid = transform.Find("mul_row/bg/Scroll View/LoopTitleGrid").GetComponent<UILoopTitleGrid>();
        m_dictLoopGrid = transform.Find("dict_row/bg/Scroll View/LoopTitleGrid").GetComponent<UILoopTitleGrid>();
    }
    // Use this for initialization
    void Start() {
        m_oneLoopGrid.CreateScrollView(m_testList, typeof(ListItem));

        m_mulLoopGrid.CreateScrollView(m_testList, typeof(ListItem));

        m_dictLoopGrid.CreateScrollView(m_testDict, typeof(DictItem));
    }

}
