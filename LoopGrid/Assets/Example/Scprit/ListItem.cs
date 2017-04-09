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
* Filename: ListItem
* Created:  2017/4/9 19:46:31
* Author:   HaYaShi ToShiTaKa
* Purpose:  
* ==============================================================================
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListItem : LoopGridBaseItem {
    private UILabel m_lblIndex;
    private int data = 0;
    public override void FindItem() {
        base.FindItem();

        m_lblIndex = transform.Find("Label").GetComponent<UILabel>();
    }

    public override void FillItem(IList datas, int index, int gridIndex, int lineIndex) {
        base.FillItem(datas, index, gridIndex, lineIndex);

        data = (int)datas[index];
        m_lblIndex.text = data.ToStringNoGC();
        gameObject.RegistUIButton(Click);
    }

    private void Click(GameObject sender) {
        Debug.Log(data);
    }
}