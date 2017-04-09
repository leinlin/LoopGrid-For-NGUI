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
* Filename: UIEventNotifyHelper
* Created:  2017/4/9 18:47:44
* Author:   HaYaShi ToShiTaKa
* Purpose:  NGUI 事件通知扩展
* ==============================================================================
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UIEventNotifyHelper {
    public static void RegistUIButton(this GameObject button, UIEventListener.VoidDelegate action) {
        UIEventListener listener = UIEventListener.Get(button);
        listener.onClick = action;
    }
    public static void RegistUIButton(this Component button, UIEventListener.VoidDelegate action) {
        RegistUIButton(button.gameObject, action);
    }
    public static void UnRegistUIButton(this Component button) {
        UnRegistUIButton(button.gameObject);
    }
    public static void UnRegistUIButton(this GameObject button) {
        UIEventListener listener = UIEventListener.Get(button);
        listener.onClick = null;
        listener.onPress = null;
        listener.onDragOver = null;
        listener.onDrop = null;
        listener.onSelect = null;
        listener.onHover = null;
    }
    public static void RegistOnPress(this GameObject button, UIEventListener.BoolDelegate action) {
        UIEventListener listener = UIEventListener.Get(button);
        listener.onPress = action;
    }
    public static void RegistOnDrag(this GameObject button, UIEventListener.VectorDelegate action) {
        UIEventListener listener = UIEventListener.Get(button);
        listener.onDrag = action;
    }
    public static void RegistOnDragOver(this GameObject button, UIEventListener.VoidDelegate action) {
        UIEventListener listener = UIEventListener.Get(button);
        listener.onDragOver = action;
    }

    public static void RegistOnDragOut(this GameObject button, UIEventListener.VoidDelegate action) {
        UIEventListener listener = UIEventListener.Get(button);
        listener.onDragOut = action;
    }

    public static void RegistOnDrop(this GameObject button, UIEventListener.ObjectDelegate action) {
        UIEventListener listener = UIEventListener.Get(button);
        listener.onDrop = action;
    }
    public static void RegistOnSelect(this GameObject button, UIEventListener.BoolDelegate action) {
        UIEventListener listener = UIEventListener.Get(button);
        listener.onSelect = action;
    }
    public static void RegistOnHover(this GameObject button, UIEventListener.BoolDelegate action) {
        UIEventListener listener = UIEventListener.Get(button);
        listener.onHover = action;
    }
}