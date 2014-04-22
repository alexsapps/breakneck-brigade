using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.Glfw;
using SousChef;

namespace Breakneck_Brigade
{
    class InputManager
    {
        public const int GLFW_KEY_UNKNOWN       = -1; 
        public const int GLFW_KEY_SPACE         = 32;
        //public const int GLFW_KEY_APOSTROPHE    = 39; // ' 
        //public const int GLFW_KEY_COMMA         = 44; // , 
        //public const int GLFW_KEY_MINUS         = 45; // - 
        //public const int GLFW_KEY_PERIOD        = 46; // . 
        //public const int GLFW_KEY_SLASH         = 47; // / 
        //public const int GLFW_KEY_0             = 48;
        //public const int GLFW_KEY_1             = 49;
        //public const int GLFW_KEY_2             = 50;
        //public const int GLFW_KEY_3             = 51;
        //public const int GLFW_KEY_4             = 52;
        //public const int GLFW_KEY_5             = 53;
        //public const int GLFW_KEY_6             = 54;
        //public const int GLFW_KEY_7             = 55;
        //public const int GLFW_KEY_8             = 56;
        //public const int GLFW_KEY_9             = 57;
        //public const int GLFW_KEY_SEMICOLON     = 59; // ; 
        //public const int GLFW_KEY_EQUAL         = 61; // = 
        public const int GLFW_KEY_A             = 65;
        //public const int GLFW_KEY_B             = 66;
        //public const int GLFW_KEY_C             = 67;
        public const int GLFW_KEY_D             = 68;
        //public const int GLFW_KEY_E             = 69;
        //public const int GLFW_KEY_F             = 70;
        //public const int GLFW_KEY_G             = 71;
        //public const int GLFW_KEY_H             = 72;
        //public const int GLFW_KEY_I             = 73;
        //public const int GLFW_KEY_J             = 74;
        //public const int GLFW_KEY_K             = 75;
        //public const int GLFW_KEY_L             = 76;
        //public const int GLFW_KEY_M             = 77;
        //public const int GLFW_KEY_N             = 78;
        //public const int GLFW_KEY_O             = 79;
        //public const int GLFW_KEY_P             = 80;
        //public const int GLFW_KEY_Q             = 81;
        //public const int GLFW_KEY_R             = 82;
        public const int GLFW_KEY_S             = 83;
        //public const int GLFW_KEY_T             = 84;
        //public const int GLFW_KEY_U             = 85;
        //public const int GLFW_KEY_V             = 86;
        public const int GLFW_KEY_W             = 87;
        //public const int GLFW_KEY_X             = 88;
        //public const int GLFW_KEY_Y             = 89;
        //public const int GLFW_KEY_Z             = 90;
        //public const int GLFW_KEY_LEFT_BRACKET  = 91; // [
        //public const int GLFW_KEY_BACKSLASH     = 92; // \
        //public const int GLFW_KEY_RIGHT_BRACKET = 93; // ]
        //public const int GLFW_KEY_GRAVE_ACCENT  = 96; // `
        //public const int GLFW_KEY_WORLD_1       = 161; // non-US #1
        //public const int GLFW_KEY_WORLD_2       = 162; // non-US #2
        public const int GLFW_KEY_ESCAPE        = 257;
        public const int GLFW_KEY_ENTER         = 294;
        public const int GLFW_KEY_TAB           = 258;
        //public const int GLFW_KEY_BACKSPACE     = 259;
        //public const int GLFW_KEY_INSERT        = 260;
        //public const int GLFW_KEY_DELETE        = 261;
        //public const int GLFW_KEY_RIGHT         = 262;
        //public const int GLFW_KEY_LEFT          = 263;
        //public const int GLFW_KEY_DOWN          = 264;
        //public const int GLFW_KEY_UP            = 265;
        //public const int GLFW_KEY_PAGE_UP       = 266;
        //public const int GLFW_KEY_PAGE_DOWN     = 267;
        //public const int GLFW_KEY_HOME          = 268;
        //public const int GLFW_KEY_END           = 269;
        //public const int GLFW_KEY_CAPS_LOCK     = 280;
        //public const int GLFW_KEY_SCROLL_LOCK   = 281;
        //public const int GLFW_KEY_NUM_LOCK      = 282;
        //public const int GLFW_KEY_PRINT_SCREEN  = 283;
        //public const int GLFW_KEY_PAUSE         = 284;
        //public const int GLFW_KEY_F1            = 290;
        //public const int GLFW_KEY_F2            = 291;
        //public const int GLFW_KEY_F3            = 292;
        //public const int GLFW_KEY_F4            = 293;
        //public const int GLFW_KEY_F5            = 294;
        //public const int GLFW_KEY_F6            = 295;
        //public const int GLFW_KEY_F7            = 296;
        //public const int GLFW_KEY_F8            = 297;
        //public const int GLFW_KEY_F9            = 298;
        //public const int GLFW_KEY_F10           = 299;
        //public const int GLFW_KEY_F11           = 300;
        //public const int GLFW_KEY_F12           = 301;
        //public const int GLFW_KEY_F13           = 302;
        //public const int GLFW_KEY_F14           = 303;
        //public const int GLFW_KEY_F15           = 304;
        //public const int GLFW_KEY_F16           = 305;
        //public const int GLFW_KEY_F17           = 306;
        //public const int GLFW_KEY_F18           = 307;
        //public const int GLFW_KEY_F19           = 308;
        //public const int GLFW_KEY_F20           = 309;
        //public const int GLFW_KEY_F21           = 310;
        //public const int GLFW_KEY_F22           = 311;
        //public const int GLFW_KEY_F23           = 312;
        //public const int GLFW_KEY_F24           = 313;
        //public const int GLFW_KEY_F25           = 314;
        //public const int GLFW_KEY_KP_0          = 320;
        //public const int GLFW_KEY_KP_1          = 321;
        //public const int GLFW_KEY_KP_2          = 322;
        //public const int GLFW_KEY_KP_3          = 323;
        //public const int GLFW_KEY_KP_4          = 324;
        //public const int GLFW_KEY_KP_5          = 325;
        //public const int GLFW_KEY_KP_6          = 326;
        //public const int GLFW_KEY_KP_7          = 327;
        //public const int GLFW_KEY_KP_8          = 328;
        //public const int GLFW_KEY_KP_9          = 329;
        //public const int GLFW_KEY_KP_DECIMAL    = 330;
        //public const int GLFW_KEY_KP_DIVIDE     = 331;
        //public const int GLFW_KEY_KP_MULTIPLY   = 332;
        //public const int GLFW_KEY_KP_SUBTRACT   = 333;
        //public const int GLFW_KEY_KP_ADD        = 334;
        //public const int GLFW_KEY_KP_ENTER      = 335;
        //public const int GLFW_KEY_KP_EQUAL      = 336;
        //public const int GLFW_KEY_LEFT_SHIFT    = 340;
        //public const int GLFW_KEY_LEFT_CONTROL  = 341;
        //public const int GLFW_KEY_LEFT_ALT      = 342;
        //public const int GLFW_KEY_LEFT_SUPER    = 343;
        //public const int GLFW_KEY_RIGHT_SHIFT   = 344;
        //public const int GLFW_KEY_RIGHT_CONTROL = 345;
        //public const int GLFW_KEY_RIGHT_ALT     = 346;
        //public const int GLFW_KEY_RIGHT_SUPER   = 347;
        //public const int GLFW_KEY_MENU          = 348;
        //public const int GLFW_KEY_LAST          = GLFW_KEY_MENU;

        public const int GLFW_MOUSE_BUTTON_1    = 0;
        public const int GLFW_MOUSE_BUTTON_2    = 1;
        public const int GLFW_MOUSE_BUTTON_LEFT = GLFW_MOUSE_BUTTON_1;
        public const int GLFW_MOUSE_BUTTON_RIGHT = GLFW_MOUSE_BUTTON_2;

        GlobalsConfigFile globalConfig;
        

        // Game-related stuff
        public Hashtable keys = new Hashtable();
        Vector4 rot = new Vector4();

        bool fpsMode = false;        
        int originX = 0;
        int originY = 0;        
        float sens = 1.0f;
        bool invertY = false;

        private Glfw.GLFWkeyfun         keyboardCallback;
        private Glfw.GLFWmouseposfun    mouseMoveCallback;
        private Glfw.GLFWmousebuttonfun mouseButtonCallback;

        public InputManager()
        {
            // callback for keyboard; will trigger KeyboardInput() on every keypress or release
            keyboardCallback = KeyboardInput; //Need to follow this convention for declaring callbacks so C# does not GC the reference immediately
            Glfw.glfwSetKeyCallback(keyboardCallback);
            // callback for mouse; will trigger MousePos() on every mouse movement
            mouseMoveCallback = MousePos;
            Glfw.glfwSetMousePosCallback(mouseMoveCallback);
            // callback for mouse; will trigger MouseButton() on every mouse click/release
            mouseButtonCallback = MouseButton;
            Glfw.glfwSetMouseButtonCallback(mouseButtonCallback);

            globalConfig = new GlobalsConfigFolder(BBXml.GetLocalConfigFolder()).Open("keys.xml");

            keys[GLFW_KEY_W] = 0;
            keys[GLFW_KEY_A] = 0;
            keys[GLFW_KEY_S] = 0;
            keys[GLFW_KEY_D] = 0;
            keys[GLFW_KEY_SPACE] = 0;
            keys[GLFW_KEY_ESCAPE] = 0;

            keys[GLFW_MOUSE_BUTTON_LEFT] = 0;
            keys[GLFW_MOUSE_BUTTON_RIGHT] = 0;
        }

        void KeyboardInput(int key, int action)
        {
            bool pressed = action == Glfw.GLFW_PRESS ? true : false;
            int temp = remap(key);
            keys[temp] = pressed;

            Console.WriteLine("key " + temp + (pressed ? " pressed." : " released."));
        }

        void MousePos(int x, int y)
        {
            if (fpsMode)
            {
                // Get difference of mouse pos & origin
                int tempX = originX - x;
                int tempY = originY - y;

                // add movement to rotation modified by sensitivity setting
                // positive X movement negatively rotates around Y axis
                rot[1] -= tempX * sens;

                if (!invertY)
                {
                    // positive Y movement negatively rotates around X axis with no invert
                    rot[0] -= tempY * sens;
                }
                else
                {
                    rot[0] += tempY * sens;
                }

                // Reset mouse to origin
                Glfw.glfwSetMousePos(originX, originY);
            }
        }

        /// <summary>
        /// Sets the initial position of the mouse cursor for FPS mode, and saves the values for later use.
        /// </summary>
        void MousePosInit()
        {
            int width, height;
            Glfw.glfwGetWindowSize(out width, out height);

            Glfw.glfwSetMousePos(width/2, height/2);
            Glfw.glfwGetMousePos(out originX, out originY);

            // To-be-implemented; get sens from config
            sens = float.Parse(globalConfig.GetSetting("sens", 0.1f));
            // To-be-implemented; get invertY from config
            invertY = bool.Parse(globalConfig.GetSetting("invertY", false));
            // To-be-implemented; swap left/right click
            /* if(swapFlag)
             * {
             *      mouse button left = mouse button 2;
             *      mouse button right = mouse button 1;
             * }
             */
        }

        void MouseButton(int button, int action)
        {
            bool pressed = action == Glfw.GLFW_PRESS ? true : false;
            keys[button] = pressed;
        }

        public void Clear()
        {
            rot[0] = 0.0f;
            rot[1] = 0.0f;
        }

        /// <summary>
        /// Remaps rebound keys from config file to locally defined values.
        /// </summary>
        /// <param name="key">value of key passed to callback method</param>
        /// <returns>value of key as defined by config file or default value if not defined</returns>
        private int remap(int key)
        {
            // if config load is broken,
            // return key;
            return int.Parse(globalConfig.GetSetting(key.ToString(), key));
        }

        /// <summary>
        /// Methods for toggling FPS mouse mode.
        /// </summary>
        public void TurnOnFPSMode()
        {
            MousePosInit();
            fpsMode = true;
            
        }

        public void TurnOffFPSMode()
        {
            fpsMode = false;
        }

        public float GetRotX()
        {
            return rot[0];
        }

        public float GetRotY()
        {
            return rot[1];
        }

        public Hashtable GetKeys()
        {
            return keys;
        }
    }
}
