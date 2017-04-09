/*
               #########                       
              ############                     
              #############                    
             ##  ###########                   
            ###  ###### #####                  
            ### #######   ####                 
           ###  ########## ####                
          ####  ########### ####               
         ####   ###########  #####             
        #####   ### ########   #####           
       #####   ###   ########   ######         
      ######   ###  ###########   ######       
     ######   #### ##############  ######      
    #######  #####################  ######     
    #######  ######################  ######    
   #######  ###### #################  ######   
   #######  ###### ###### #########   ######   
   #######    ##  ######   ######     ######   
   #######        ######    #####     #####    
    ######        #####     #####     ####     
     #####        ####      #####     ###      
      #####       ###        ###      #        
        ###       ###        ###               
         ##       ###        ###               
__________#_______####_______####______________

                我们的未来没有BUG              
* ==============================================================================
* Filename: UILoopTitleGrid
* Created:  2017/3/16 13:24:12
* Author:   HaYaShi ToShiTaKa
* Purpose:  
* ==============================================================================
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILoopTitleGrid : UIWidgetContainer {

    #region member
    [Range(3, 10)]
    [SerializeField]
    [Tooltip("当Splite Padding 为负的时候\n如果往上拉出现空白的时候加大这个数值")]
    private int m_cacheNum = 3;

    public GameObject upArrow;
    public GameObject downArrow;
    public GameObject itemTemplate;
    private UIDragScrollView m_scrollViewDrag;
    [Range(1, 10)]
    public int maxPerLine = 1;
    public float cellWidth = 200f;
    public float cellHeight = 200f;
    public bool asyncLoad = true;
    [HideInInspector]
    public int selectIndex = -1;//

    [Range(-200, 200)]
    [SerializeField]
    private float m_splitePadding = 0;//分割索引的item 所需空间
    public float splitePadding {
        get {
            return m_splitePadding;
        }
    }

    public int testCount = 100;//仅供测试的时候使用

    public float cellLength {
        get {
            float result = cellHeight;
            if (!m_initiated) {
                InitPosAndScroll();
            }
            if (m_moveType == UIScrollView.Movement.Vertical) {
                result = cellHeight;
            }
            else if (m_moveType == UIScrollView.Movement.Horizontal) {
                result = cellWidth;
            }
            return result;
        }
    }
    private bool m_enableDestory = false;
    private bool m_initiated = false;
    private UIScrollView m_scrollView;
    private List<List<LoopGridBaseItem>> m_items = new List<List<LoopGridBaseItem>>();
    private UIPanel m_panel;
    private static UIRoot m_uiRoot;
    private Vector3 m_panelInitPos = Vector3.zero;                  //最初的panel位置
    private Vector2 m_initOffset = Vector2.zero;                    //panel的offset
    private UIScrollView.Movement m_moveType = UIScrollView.Movement.Vertical;

    private Type m_loopGridType;
    //重刷的时候被重置的数据
    private IList m_datas;                                          //绘制的数据

    private int m_maxArrangeNum = 0;                                //无限加载时最多的Item行或列数量
    private int m_dataArrangeNum = 0;                               //实际数据的行列数量
    private int m_fillCount = 0;                                    //填满panel的行列数量
    private int m_floorFillCount = 0;                               //正好不超过panel边缘的行列数量
    private float m_cellHalf = 0;                                   //cell中间的位置
    private float m_maxDistance = 0;                                //数据所需panel最大长度

    private int m_lastDataIndex = 0;                                //上次显示出来的第一个格子，在数据中的行列索引
    private int m_maxIndex = 0;                                     //显示出来的数据最大行列索引
    private int m_minIndex = 0;                                     //显示出来的数据最小行列索引
    private int m_forwardCacheNum = 0;                              //用于缓存向指定方向滑动，预加载的格子数

    private Dictionary<int, object> m_spTitleDict = new Dictionary<int, object>();
    private Dictionary<int, int> m_spIndexDict = new Dictionary<int, int>();           //所需空间与cell设置不一样的索引集合
    private List<float> m_spDistList = new List<float>();                              //倒过来遍历寻找相应索引
    private Dictionary<float, int> m_spIndexDistDict = new Dictionary<float, int>();   //所需空间与cell设置不一样的距离集合
    private HashSet<int> m_allLineDataNull = new HashSet<int>();                       //该行数据全部为空
    //async
    private Coroutine m_addItemTimer;
    private bool m_isNotNeedAsync = false;

    public MonoBehaviour handleUI { get; private set; }
    #endregion

    #region pool
    private GameObject m_poolGo;
    private ObjectPool<GameObject> m_itemCOPool = new ObjectPool<GameObject>();
    private GameObject CreateItemGO() {
        GameObject itemGO = Instantiate(itemTemplate);
        itemGO.gameObject.SetActive(false);
        return itemGO;
    }
    private void Init(GameObject itemCO) {
        itemCO.SetActive(false);
        itemCO.transform.parent = m_poolGo.transform;
    }
    private void CreateItemPool(GameObject itemTemplate, int poolNum) {
        if (m_poolGo != null) return;
        m_poolGo = new GameObject();
        m_poolGo.name = "pool";
        m_poolGo.transform.localScale = Vector3.zero;
        m_poolGo.transform.parent = transform.parent;

        m_itemCOPool.Init(poolNum, CreateItemGO, Init);
    }
    private GameObject GetGridItem() {
        return m_itemCOPool.GetObject();
    }
    #endregion

    #region virtual
    void OnValidate() {
        if (!Application.isPlaying && NGUITools.GetActive(this)) {
            if (m_datas != null) {
                ResetToBegin();
            }
        }
    }

    void OnDestroy() {
        if (m_addItemTimer != null && UIRoot.list.Count > 0) {
            m_uiRoot.StopCoroutine(m_addItemTimer);
        }
    }
    #endregion

    #region public
    /// <summary>
    /// 设置一个UI操作类，提供给item绑定类调用
    /// </summary>
    public void RegistHandleUIClass(MonoBehaviour ui) {
        handleUI = ui;
    }

    /// <summary>
    /// 移动到对应数据index的列
    /// </summary>
    /// <param name="index"></param>
    public void MoveToIndex(int index) {
        index =  Mathf.CeilToInt((float)index / (float)maxPerLine);
        MoveRelative(index);
    }
    public void MoveRelative(int delta) {
        m_scrollView.DisableSpring();
        if (m_scrollView.canMoveVertically) {
            m_scrollView.MoveRelative(new Vector3(0, (delta - m_lastDataIndex) * cellHeight));
        }
        else {
            m_scrollView.MoveRelative(new Vector3((m_lastDataIndex - delta) * cellWidth, 0));
        }
        m_scrollView.InvalidateBounds();
        m_scrollView.RestrictWithinBounds(true);

        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }
    public bool TryGetTitleData(int spIndex, out object result) {
        return m_spTitleDict.TryGetValue(spIndex, out result);
    }

    public int GetTitleIndex(int spIndex) {
        int result = 0;

        var itr = m_spTitleDict.GetEnumerator();
        while (itr.MoveNext()) {
            if (itr.Current.Key == spIndex) {
                break;
            }
            result++;
        }
        itr.Dispose();

        return result;
    }

    public bool TryGetNextTitleData(int spIndex, out int nextIndex) {
        bool result = false;
        nextIndex = spIndex;

        var itr = m_spTitleDict.GetEnumerator();
        while (itr.MoveNext()) {
            if (itr.Current.Key == spIndex && itr.MoveNext()) {
                nextIndex = itr.Current.Key;
                result = true;
                break;
            }
        }
        itr.Dispose();

        return result;
    }
    public void InvalidatePanel() {
        m_initiated = false;
        m_spTitleDict.Clear();
    }
    public bool CheckAllLineDataNull(int gridIndex) {
        return m_allLineDataNull.Contains(gridIndex);
    }
    public IList CreateScrollView(IDictionary dict, Type t) {
#if DEBUG
        if (!typeof(LoopGridBaseItem).IsAssignableFrom(t)) throw new Exception("传入类型必须是LoopGridBase的派生类");
#endif
        m_allLineDataNull.Clear();
        List<int> splitIndexList = null;

        IList result = null;
        result = MakeDictToTitleList(dict, out splitIndexList);
        ClearSplit();
        for (int i = 0, imax = splitIndexList.Count; i < imax; i++) {
            AddSplitIndex(splitIndexList[i]);
        }
        CreateScrollView(result, t);
        return result;
    }

    private IList MakeDictToTitleList(IDictionary dict, out List<int> splitIndexList) {
        IList result = new List<object>();
        splitIndexList = new List<int>();
        m_spTitleDict.Clear();

#if DEBUG
        var valItr = dict.Values.GetEnumerator();
        if (valItr.MoveNext()) {
            if (!(valItr.Current is IList)) throw new Exception("传入的字典value必须是个list");
        }
#endif

        var keyItr = dict.Keys.GetEnumerator();
        while (keyItr.MoveNext()) {
            if (m_splitePadding > 0) {
                AddPlusSplitItems(dict, keyItr, result, splitIndexList);
            }
            else {
                AddMinusSplitItems(dict, keyItr, result, splitIndexList);
            }
        }
        return result;
    }

    private void AddPlusSplitItems(IDictionary dict, IEnumerator keyItr, IList result, List<int> splitIndexList) {
        IList list = dict[keyItr.Current] as IList;
        int index = result.Count / maxPerLine;
        splitIndexList.Add(index);
        m_spTitleDict.Add(index, keyItr.Current);

        //添加item,不足部分补上null
        int count = list.Count;
        //list长度为0处理
        if (count <= 0) {
            for (int i = 0, imax = maxPerLine; i < imax; i++) {
                result.Add(null);
            }
            m_allLineDataNull.Add(index);
        }
        else {
            for (int i = 0, imax = list.Count; i < imax; i++) {
                result.Add(list[i]);
            }
            int remainder = list.Count % maxPerLine;
            remainder = remainder != 0 ? maxPerLine - remainder : 0;
            for (int i = 0, imax = remainder; i < imax; i++) {
                result.Add(null);
            }
        }
    }

    private void AddMinusSplitItems(IDictionary dict, IEnumerator keyItr, IList result, List<int> splitIndexList) {
        IList list = dict[keyItr.Current] as IList;
        int index = (result.Count + 1) / maxPerLine;
        splitIndexList.Add(index);
        m_spTitleDict.Add(index, keyItr.Current);
        //添加title, 一行剩余部分补上null
        result.Add(keyItr.Current);
        for (int i = 0; i < maxPerLine - 1; i++) {
            result.Add(null);
        }

        //添加item,不足部分补上null
        int count = list.Count;
        //list长度为0处理
        for (int i = 0; i < count; i++) {
            result.Add(list[i]);
        }
        int remainder = count % maxPerLine;
        remainder = remainder != 0 ? maxPerLine - remainder : 0;
        for (int i = 0; i < remainder; i++) {
            result.Add(null);
        }
    }

    public void CreateScrollView(IList datas, Type t) {
#if DEBUG
        if (!typeof(LoopGridBaseItem).IsAssignableFrom(t)) throw new Exception("传入类型必须是LoopGridBase的派生类");
        if (m_loopGridType != null && m_loopGridType != t) throw new Exception("同一个Grid 不要绑定不同的组件");
#endif
        if (!m_initiated) {
            InitPosAndScroll();
        }
        m_loopGridType = t;
        Clear();
        SlotData(datas);
        ResetToBegin();
    }
    /// <summary>
    /// 不改变数据，重新绘制scroll
    /// scroll重置到原来的位置
    /// </summary>
    public void ResetToBegin() {
        ResetPosition();
        CheckIsNeedArrows();
        CheckDragBack();
        CheckLoopMove();

        m_scrollView.DisableSpring();
        if (m_isNotNeedAsync || !Application.isPlaying || !asyncLoad) {
            AddItems();
        }
        else {
            if (m_addItemTimer != null && UIRoot.list.Count > 0) {
                m_uiRoot.StopCoroutine(m_addItemTimer);
            }
            m_addItemTimer = m_uiRoot.StartCoroutine(AddItemsAsync());
        }

    }

    /// <summary>
    /// 某个数据发生变化，直接改变数据,然后调用这个函数进行重新填充（比如折叠的item展开）
    /// 特点不会把scroll重置到原始状态
    /// </summary>
    public void RefreshData() {
        int dataArrangeNum = Mathf.CeilToInt((float)m_datas.Count / (float)maxPerLine);
        bool preIsLoop = m_panel.onClipMove == OnPanelLoopClipMove;
        bool curIsLoop = dataArrangeNum >= m_fillCount + m_cacheNum;

        //调整item数量
        if (preIsLoop && curIsLoop) {
            m_dataArrangeNum = dataArrangeNum;
            // 把所有的ITEM遍历一遍，重新根据变化的数据填值
            bool isMove = false;
            for (int i = m_items.Count - 1; i >= 0; i--) {
                List<LoopGridBaseItem> list = m_items[i];
                for (int j = list.Count - 1; j >= 0; j--) {
                    LoopGridBaseItem item = list[j];
                    isMove = !item.UpdateItem();
                    if (isMove) {
                        m_forwardCacheNum = m_forwardCacheNum - (m_cacheNum - 1);
                        MoveToIndex(Mathf.Max(0, m_datas.Count - m_fillCount * maxPerLine));
                        m_forwardCacheNum = m_forwardCacheNum + (m_cacheNum - 1);
                    }
                }
            }
        }
        else {
            ResetToBegin();
        }
    }

    /// <summary>
    /// 获取到索引对于数据填充的item,如果找到不为空就重新绘制它
    /// </summary>

    public float GetSpliteSpaceByIndex(int index) {
        int result = 0;

        while (!m_spIndexDict.TryGetValue(index, out result)) {
            if (index < -1) break;
            index--;
        }

        return (float)result * m_splitePadding;
    }
    public float GetSplitSpaceByDist(float dist) {
        int result = 0;

        for (int i = m_spDistList.Count - 1; i >= 0; i--) {
            float idx = m_spDistList[i];
            if (idx <= dist) {
                result = m_spIndexDistDict[idx];
                break;
            }
        }

        return (float)result * m_splitePadding;
    }
    #endregion

    #region menu
    public void ModifyDragArea(UIDragScrollView ds) {
        UIWidget uw = ds.GetComponent<UIWidget>();
        if (!uw) {
            uw = ds.gameObject.AddComponent<UIWidget>();
        }

        uw.depth = -100;
        uw.width = 2;
        uw.height = 2;
        BoxCollider bx = ds.GetComponent<BoxCollider>();
        if (!bx) {
            bx = ds.gameObject.AddComponent<BoxCollider>();
        }
        if (!m_panel) {
            InitPosAndScroll();
        }

        Vector3 size = bx.size;
        size.x = m_panel.width;
        size.y = m_panel.height;
        bx.size = size;

        m_scrollViewDrag = ds;

        FixBoxPostion();
}
    private void FixBoxPostion() {
        BoxCollider bx = m_scrollViewDrag.GetComponent<BoxCollider>();
        if (!bx) {
            return;
        }
        Vector3 center = m_panel.baseClipRegion;
        center.z = 0;
        Vector3 offset = m_panel.clipOffset;


        center = center + offset;
        m_scrollViewDrag.transform.localPosition = center;
    }

    [ContextMenu("Execute")]
    private void Execute() {
        List<int> datas = new List<int>(testCount);
        for (int i = 0; i < testCount; i++) {
            datas.Add(i);
        }
        ClearSplit();
        CreateScrollView(datas, typeof(LoopGridBaseItem));
    }
    [ContextMenu("Execute Data Count Change")]
    private void ExecuteDataCountChange() {
        for (int i = m_datas.Count - 1; i > 50; i--) {
            m_datas.RemoveAt(i);
        }
        RefreshData();
    }

    [ContextMenu("ExecuteSplit")]
    private void ExecuteSplit() {
        List<int> datas = new List<int>(testCount);
        for (int i = 0; i < testCount; i++) {
            datas.Add(i);
        }
        Debug.Log("偶数所需cell长度将不一样");

        ClearSplit();
        int arrangeNum = Mathf.CeilToInt((float)datas.Count / (float)maxPerLine);

        for (int i = 0; i < arrangeNum; i++) {
            if (i % 2 == 0) {
                AddSplitIndex(i);
            }
        }
        CreateScrollView(datas, typeof(LoopGridBaseItem));
    }
    [ContextMenu("ExecuteDict")]
    private void ExecuteDict() {
        Dictionary<int, List<int>> dict = new Dictionary<int, List<int>>();
        for (int i = 0; i < testCount; i++) {
            List<int> list = new List<int>();
            list.Add(0);
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            list.Add(5);
            dict.Add(i, list);
        }
        CreateScrollView(dict, typeof(LoopGridBaseItem));
    }
    [ContextMenu("ResetChild")]
    private void ResetChild() {
        if (m_poolGo != null) {
            NGUITools.Destroy(m_poolGo);
            m_poolGo = null;
        }
        var itr = m_itemCOPool.GetIter();
        while (itr.MoveNext()) {
            NGUITools.Destroy(itr.Current.gameObject);
        }
        itr.Dispose();
        m_itemCOPool.Clear();
        for (int i = 0, imax = m_items.Count; i < imax; ++i) {
            List<LoopGridBaseItem> itemList = m_items[i];
            for (int j = 0, jmax = itemList.Count; j < jmax; ++j) {
                NGUITools.Destroy(itemList[j].gameObject);
            }
        }
        m_items.Clear();
        m_allLineDataNull.Clear();
        m_datas = null;
        m_isNotNeedAsync = false;
        NGUITools.Destroy(m_scrollViewDrag.gameObject);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.UnloadUnusedAssetsImmediate();
#endif
    }

    [ContextMenu("Recycle")]
    private void StoreAllItem() {
        for (int i = 0; i < m_items.Count; i++) {
            for (int j = 0; j < m_items[i].Count; j++) {
                m_itemCOPool.Store(m_items[i][j].gameObject);
            }
        }
        m_items.Clear();
    }

    [ContextMenu("test invisible")]
    private void Test() {
        EnableDestory();
    }
    #endregion

    #region private
    protected void EnableDestory() {
        do {
            if (m_enableDestory) break;
            if (gameObject.activeInHierarchy) break;

            Transform tmp = transform.parent;
            transform.parent = null;
            bool active = gameObject.activeSelf;
            gameObject.SetActive(true);
            gameObject.SetActive(active);
            transform.parent = tmp;

        } while (false);

        m_enableDestory = true;
    }
    private void CalDistance(float distance, out float targetDis, out int index) {
        FixBoxPostion();
        distance = Mathf.Abs(distance);
        targetDis = distance;
        if (m_splitePadding != 0) {
            float splitDist = GetSplitSpaceByDist(distance);
            targetDis = splitDist > 0 ? distance - splitDist : 0;
        }
        index = Mathf.FloorToInt(targetDis / cellLength);
        ShowArrow(index, distance);
    }
    private void AddSplitIndex(int index) {
        int count = m_spIndexDict.Count + 1;
        m_spIndexDict.Add(index, count);
        float dist = index * cellLength + count * m_splitePadding;
        m_spDistList.Add(dist);
        m_spIndexDistDict.Add(dist, count);
    }

    private void ClearSplit() {
        m_spIndexDict.Clear();
        m_spDistList.Clear();
        m_spIndexDistDict.Clear();
    }

    private void InitPosAndScroll() {
        m_scrollView = transform.parent.GetComponent<UIScrollView>();
        if (m_scrollView == null) Debug.LogException(new Exception("父节点必须有ScrollView这个组件"));
        m_panel = m_scrollView.GetComponent<UIPanel>();
        if (m_panel == null) Debug.LogException(new Exception("父节点必须有UIPanel这个组件"));
        m_panelInitPos = m_scrollView.transform.localPosition;
        m_initOffset = m_panel.clipOffset;
        m_moveType = m_scrollView.movement;
        EnableDestory();
        if (m_uiRoot == null) {
            if (UIRoot.list.Count <= 0) throw new Exception("必须要有一个没被隐藏的UIRoot");
            m_uiRoot = UIRoot.list[0];
        }
        if (itemTemplate != null) {
            itemTemplate.SetActive(false);
        }
        if (!m_scrollViewDrag) {
            m_scrollViewDrag = NGUITools.AddChild<UIDragScrollView>(transform.parent.gameObject);
            ModifyDragArea(m_scrollViewDrag);
        }
        
        m_initiated = true;
    }

    private void Clear() {
        m_datas = null;
        m_maxArrangeNum = 0;
        m_maxDistance = 0;
        m_dataArrangeNum = 0;
        m_fillCount = 0;
        m_floorFillCount = 0;
        m_cellHalf = 0;

        m_lastDataIndex = 0;
        m_maxIndex = 0;
        m_minIndex = 0;
        m_forwardCacheNum = 0;
    }

    private void ResetPosition() {
        m_scrollView.transform.localPosition = m_panelInitPos;
        m_panel.clipOffset = m_initOffset;
    }

    private void SlotData(IList datas) {
        if (datas == null) Debug.LogException(new Exception("绑定数据不能为空"));
        m_datas = datas;
        if (itemTemplate == null) Debug.LogException(new Exception("模板不能为空"));

        m_dataArrangeNum = Mathf.CeilToInt((float)datas.Count / (float)maxPerLine);

        float len = cellLength;
        float allLen = m_moveType == UIScrollView.Movement.Horizontal ? m_panel.width : m_panel.height;

        m_fillCount = Mathf.CeilToInt(allLen / len);
        m_floorFillCount = Mathf.FloorToInt(allLen / len);
        m_cellHalf = len * 0.5f;

        m_maxArrangeNum = Math.Min(m_dataArrangeNum, m_fillCount + m_cacheNum);
        m_maxDistance = (m_dataArrangeNum - 0.05f) * len + m_spDistList.Count * m_splitePadding - allLen;

        CreateItemPool(itemTemplate, 1);
    }

    private void AddItems() {
        StoreAllItem();
        for (int i = 0; i < m_maxArrangeNum; i++) {
            for (int j = 0; j < maxPerLine; j++) {
                if (i * maxPerLine + j >= m_datas.Count) { break; }

                LoopGridBaseItem item = AddOneItem(i, j);

                if (m_items.Count - 1 < i) {
                    m_items.Add(new List<LoopGridBaseItem>());
                }
                m_items[i].Add(item);
            }
        }
    }
    private LoopGridBaseItem AddOneItem(int gridIndex, int lineIndex) {
        GameObject go = GetGridItem();

        LoopGridBaseItem item = go.GetComponent(m_loopGridType) as LoopGridBaseItem;
        if (item == null) {
            item = go.AddComponent(m_loopGridType) as LoopGridBaseItem;
            item.grid = this;
            item.SetFirstItemData(m_datas, gridIndex * maxPerLine + lineIndex);
            item.FindItem();
        }

        item.transform.parent = transform;
        item.transform.localScale = Vector3.one;
        item.gameObject.SetActive(true);
        item.FillItem(m_datas, gridIndex * maxPerLine + lineIndex, gridIndex, lineIndex);

        return item;
    }

    private IEnumerator AddItemsAsync() {
        StoreAllItem();

        m_scrollViewDrag.enabled = false;
        for (int i = 0; i < m_maxArrangeNum; i++) {
            for (int j = 0; j < maxPerLine; j++) {
                if (i * maxPerLine + j >= m_datas.Count) { break; }
                LoopGridBaseItem item = AddOneItem(i, j);

                if (m_items.Count - 1 < i) {
                    m_items.Add(new List<LoopGridBaseItem>());
                }
                m_items[i].Add(item);
                yield return null;
            }
        }
        m_scrollViewDrag.enabled = true;
        m_isNotNeedAsync = true;
        m_addItemTimer = null;
    }

    private void CheckIsNeedArrows() {
        if (upArrow) {
            upArrow.SetActive(false);
            UIEventListener listener = UIEventListener.Get(upArrow);
            listener.onClick = UpArrowClick;
        }
        if (downArrow) {
            downArrow.SetActive(m_dataArrangeNum > m_floorFillCount);
            UIEventListener listener = UIEventListener.Get(downArrow);
            listener.onClick = DownArrowClick;
        }
    }
    private void UpArrowClick(GameObject go) {
       MoveRelative(m_lastDataIndex - 1);
    }
    private void DownArrowClick(GameObject go) {
        MoveRelative(m_lastDataIndex + 1);
    }
    private void CheckLoopMove() {
        m_panel.onClipMove = null;
        if (m_dataArrangeNum < m_maxArrangeNum) {
            m_panel.onClipMove = OnPanelNormalClipMove;
        }
        else {
            m_lastDataIndex = 0; //上次显示出来的第一个格子，在grid数据中的index
            m_maxIndex = m_maxArrangeNum - 1;
            m_minIndex = 0;
            m_panel.onClipMove = OnPanelLoopClipMove;
        }

    }

    private void CheckDragBack() {
        m_scrollView.onMomentumMove = null;
        m_scrollView.onDragFinished = null;
        // 面板没被占满拖拽回滚
        if (!m_scrollView.disableDragIfFits && m_dataArrangeNum < m_fillCount) {
            m_scrollView.onMomentumMove = OnMoveBack;
            m_scrollView.onDragFinished = OnMoveBack;
        }
    }

    private void MoveGridItem(bool isTopToBottom, bool isMoveForward) {

        List<LoopGridBaseItem> items;
        // 判断是否是 上（左）移动到下（右)
        int curIndex;
        int itemIndex;
        int sign;
        if (isTopToBottom) {
            curIndex = m_maxIndex + 1;
            itemIndex = 0;
            sign = 1;
        }
        else {
            curIndex = m_minIndex - 1;
            itemIndex = m_items.Count - 1;
            sign = -1;
        }

        items = m_items[itemIndex];

        int targetIndex = itemIndex == 0 ? m_items.Count - 1 : 0;

        m_items.Remove(items);
        m_items.Insert(targetIndex, items);

        for (int i = 0; i < items.Count; i++) {
            if (curIndex * maxPerLine + i < 0) {
                break;
            }
            if (curIndex * maxPerLine + i > m_datas.Count - 1) {
                break;
            }
            LoopGridBaseItem item = items[i];
            item.FillItem(m_datas, curIndex * maxPerLine + i, curIndex, i);
        }

        m_minIndex += sign;
        m_maxIndex += sign;
        if (isMoveForward) {
            m_forwardCacheNum -= sign;
        }

    }

    private void ShowArrow(int index, float distance) {
        if (index == 0 && distance >= m_cellHalf) {
            index = 1;
        }

        if (upArrow) {
            upArrow.gameObject.SetActive(index > 0);
        }

        if (downArrow) {
            downArrow.gameObject.SetActive(m_maxDistance > distance);
        }

    }
    #endregion

    #region callback
    private void OnMoveBack() {
        if (m_panel == null) Debug.LogException(new Exception("父节点必须有UIPanel这个组件"));
        SpringPanel.Begin(m_panel.gameObject, m_panelInitPos, 13f).strength = 8f;
    }

    private void OnPanelNormalClipMove(UIPanel panel) {
        Vector3 delta = m_panelInitPos - panel.transform.localPosition;
        float distance = -1;
        int index;//当前显示出来的第一个格子，在grid数据中的index
        distance = delta.y != 0 ? delta.y : delta.x;
        // 满的时候向上滑不管它
        if (distance > 0 && m_moveType == UIScrollView.Movement.Vertical) return;
        if (distance < 0 && m_moveType == UIScrollView.Movement.Horizontal) return;

        CalDistance(distance, out distance, out index);
    }
    private void OnPanelLoopClipMove(UIPanel panel) {
        Vector3 delata = m_panelInitPos - panel.transform.localPosition;
        float distance = -1;

        int index;//当前显示出来的第一个格子，在grid数据中的index
        distance = delata.y != 0 ? delata.y : delata.x;
        // 满的时候向上滑不管它
        if (distance > 0 && m_moveType == UIScrollView.Movement.Vertical) return;
        if (distance < 0 && m_moveType == UIScrollView.Movement.Horizontal) return;

        CalDistance(distance, out distance, out index);
        // 拖拽不满一个单元格
        if (index == m_lastDataIndex) return;

        // 拉到底了
        if (index + m_fillCount >= m_dataArrangeNum) {
            index = m_dataArrangeNum - m_fillCount;
        }
        // 重刷
        int offset = Math.Abs(index - m_lastDataIndex);

        // 判断要把最上（左）的item移动到最下（右）,还是相反
        if (m_lastDataIndex < index) {
            //如果有上一次的缓存数量，就清掉
            if (m_forwardCacheNum > 0) {
                while (m_forwardCacheNum > 1) {
                    //上（左）移动到下（右）
                    MoveGridItem(true, true);
                }
            }
            // 滑到底的时候，把上部缓存的那一个item移动到下部
            if ((m_forwardCacheNum > 0 && index + m_maxArrangeNum == m_dataArrangeNum)) {
                //上（左）移动到下（右）
                MoveGridItem(true, true);
            }
            for (int i = 1; i <= offset; i++) {
                //上（左）移动到下（右）
                MoveGridItem(true, false);
            }

        }
        else {
            m_forwardCacheNum = m_forwardCacheNum - offset;
            //缓存数量
            while ((m_forwardCacheNum < 1 && index >= 1)
                || (m_forwardCacheNum < 0 && index < 1)) {
                // 下（右）移动到上（左）
                MoveGridItem(false, true);
            }
        }

        m_lastDataIndex = index;
    }
    #endregion

}