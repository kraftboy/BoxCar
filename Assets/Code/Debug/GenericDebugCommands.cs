using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BoxCar.Debug
{
    class GenericDebugCommands
    {
        static public string AddDebugWidget(string[] args)
        {
            string help = "debugw [objectname]";

            if(args.Length < 1)
            {
                return help;
            }

            string objectName = args[0];

            GameObject obj = GameObject.Find(objectName);

            if(!obj)
            {
                return "Couldn't find object named: " + objectName;
            }

            GameObject debugObject = Resources.Load("Debug/DebugWidget") as GameObject;
            GameObject instance = GameObject.Instantiate(debugObject);
            instance.transform.SetParent(obj.transform);
            
            return "Success";
        }
    }
}
