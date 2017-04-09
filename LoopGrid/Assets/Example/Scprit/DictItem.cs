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
* Filename: DictItem
* Created:  2017/4/9 20:15:26
* Author:   HaYaShi ToShiTaKa
* Purpose:  
* ==============================================================================
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictItem : LoopGridBaseItem {

    private GameObject m_item;
    private GameObject m_title;
    private UILabel m_lblTitle;
    private UILabel m_lblIndex;
    private int data = 0;
    public override void FindItem() {
        base.FindItem();

        m_item = transform.Find("item").gameObject;
        m_title = transform.Find("title").gameObject;

        m_lblTitle = transform.Find("title/Label").GetComponent<UILabel>();
        m_lblIndex = transform.Find("item/Label").GetComponent<UILabel>();
    }

    public override void FillItem(IList datas, int index, int gridIndex, int lineIndex) {
        base.FillItem(datas, index, gridIndex, lineIndex);

        if (datas[index] != null) {
            data = (int)datas[index];
        }
        m_lblIndex.text = data.ToStringNoGC();
        m_item.RegistUIButton(Click);

        #region handle title
        bool hasTitleData = m_titleData != null;
        m_title.SetActive(hasTitleData);
        if (hasTitleData) {
            m_lblTitle.text = m_titleData.ToString();
        }
        #endregion
    }

    private void Click(GameObject sender) {
        Debug.Log(data);
    }
}