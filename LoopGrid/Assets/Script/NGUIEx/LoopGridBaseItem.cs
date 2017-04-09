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
* Filename: LoopGridBaseItem
* Created:  2017/3/16 15:49:59
* Author:   HaYaShi ToShiTaKa
* Purpose:  
* ==============================================================================
*/
using System.Collections;
using UnityEngine;

public class LoopGridBaseItem : MonoBehaviour {

    #region member
    protected UILoopTitleGrid m_grid;
    private IList m_datas;
    protected int m_index;
    protected object m_titleData;

    private UIScrollView.Movement m_moveType;
    private UIScrollView m_scrollView;
    #endregion

    #region property
    public int gridIndex { private set; get; }
    public int lineIndex { private set; get; }
    public UILoopTitleGrid grid {
        set {
            m_grid = value;
            m_scrollView = m_grid.transform.parent.GetComponent<UIScrollView>();
            m_moveType = m_scrollView.movement;
        }
        protected get {
            return m_grid;
        }
    }
    protected MonoBehaviour handleUI {
        get {
            return m_grid.handleUI;
        }
    }
    #endregion

    #region virtual
    public virtual void SetFirstItemData(IList datas, int index) {
        m_datas = datas;
        m_index = index;
    }
    public virtual void FindItem() {
    }
    public virtual void FillItem(IList datas, int index, int gridIndex, int lineIndex) {
        m_index = index;
        this.gridIndex = gridIndex;
        this.lineIndex = lineIndex;
        float space = m_grid.GetSpliteSpaceByIndex(gridIndex);
        if (m_moveType == UIScrollView.Movement.Horizontal) {
            transform.localPosition = new Vector3(m_grid.cellWidth * gridIndex + space, -m_grid.cellHeight * lineIndex, 0);
        }
        else if (m_moveType == UIScrollView.Movement.Vertical) {
            transform.localPosition = new Vector3(m_grid.cellWidth * lineIndex, -m_grid.cellHeight * gridIndex - space, 0);
        }
#if DEBUG
        gameObject.name = index.ToStringNoGC();
#endif
        //GetComponentInChildren<UILabel>().text = index.ToStringNoGC();

        gameObject.UnRegistUIButton();
        m_titleData = null;
        bool hasTileData = lineIndex == 0 && m_grid.TryGetTitleData(gridIndex, out m_titleData);
        gameObject.SetActive(datas[index] != null || hasTileData);
    }
    #endregion

    #region public api
    public bool UpdateItem() {
        bool result = false;
        do {
            if (m_datas == null) break;
            if (m_datas.Count <= 0) break;
            if (!IsIndexExit()) break;
            FillItem(m_datas, m_index, gridIndex, lineIndex);
            result = true;
        } while (false);

        return result;
    }
    #endregion

    #region private
    private void SetItemPositionByIndex(int gridIndex, int lineIndex) {
        this.gridIndex = gridIndex;
        this.lineIndex = lineIndex;
        float space = m_grid.GetSpliteSpaceByIndex(gridIndex);
        if (m_moveType == UIScrollView.Movement.Horizontal) {
            transform.localPosition = new Vector3(m_grid.cellWidth * gridIndex + space, -m_grid.cellHeight * lineIndex, 0);
        }
        else if (m_moveType == UIScrollView.Movement.Vertical) {
            transform.localPosition = new Vector3(m_grid.cellWidth * lineIndex, -m_grid.cellHeight * gridIndex - space, 0);
        }
    }
    private bool IsIndexExit() {
        return m_datas.Count > m_index;
    }
    #endregion

}