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
        const int GLFW_KEY_UNKNOWN       = -1; 
        const int GLFW_KEY_SPACE         = 32;
        const int GLFW_KEY_APOSTROPHE    = 39; // ' 
        const int GLFW_KEY_COMMA         = 44; // , 
        const int GLFW_KEY_MINUS         = 45; // - 
        const int GLFW_KEY_PERIOD        = 46; // . 
        const int GLFW_KEY_SLASH         = 47; // / 
        const int GLFW_KEY_0             = 48;
        const int GLFW_KEY_1             = 49;
        const int GLFW_KEY_2             = 50;
        const int GLFW_KEY_3             = 51;
        const int GLFW_KEY_4             = 52;
        const int GLFW_KEY_5             = 53;
        const int GLFW_KEY_6             = 54;
        const int GLFW_KEY_7             = 55;
        const int GLFW_KEY_8             = 56;
        const int GLFW_KEY_9             = 57;
        const int GLFW_KEY_SEMICOLON     = 59; // ; 
        const int GLFW_KEY_EQUAL         = 61; // = 
        const int GLFW_KEY_A             = 65;
        const int GLFW_KEY_B             = 66;
        const int GLFW_KEY_C             = 67;
        const int GLFW_KEY_D             = 68;
        const int GLFW_KEY_E             = 69;
        const int GLFW_KEY_F             = 70;
        const int GLFW_KEY_G             = 71;
        const int GLFW_KEY_H             = 72;
        const int GLFW_KEY_I             = 73;
        const int GLFW_KEY_J             = 74;
        const int GLFW_KEY_K             = 75;
        const int GLFW_KEY_L             = 76;
        const int GLFW_KEY_M             = 77;
        const int GLFW_KEY_N             = 78;
        const int GLFW_KEY_O             = 79;
        const int GLFW_KEY_P             = 80;
        const int GLFW_KEY_Q             = 81;
        const int GLFW_KEY_R             = 82;
        const int GLFW_KEY_S             = 83;
        const int GLFW_KEY_T             = 84;
        const int GLFW_KEY_U             = 85;
        const int GLFW_KEY_V             = 86;
        const int GLFW_KEY_W             = 87;
        const int GLFW_KEY_X             = 88;
        const int GLFW_KEY_Y             = 89;
        const int GLFW_KEY_Z             = 90;
        const int GLFW_KEY_LEFT_BRACKET  = 91; // [
        const int GLFW_KEY_BACKSLASH     = 92; // \
        const int GLFW_KEY_RIGHT_BRACKET = 93; // ]
        const int GLFW_KEY_GRAVE_ACCENT  = 96; // `
        const int GLFW_KEY_WORLD_1       = 161; // non-US #1
        const int GLFW_KEY_WORLD_2       = 162; // non-US #2
        const int GLFW_KEY_ESCAPE        = 256;
        const int GLFW_KEY_ENTER         = 257;
        const int GLFW_KEY_TAB           = 258;
        const int GLFW_KEY_BACKSPACE     = 259;
        const int GLFW_KEY_INSERT        = 260;
        const int GLFW_KEY_DELETE        = 261;
        const int GLFW_KEY_RIGHT         = 262;
        const int GLFW_KEY_LEFT          = 263;
        const int GLFW_KEY_DOWN          = 264;
        const int GLFW_KEY_UP            = 265;
        const int GLFW_KEY_PAGE_UP       = 266;
        const int GLFW_KEY_PAGE_DOWN     = 267;
        const int GLFW_KEY_HOME          = 268;
        const int GLFW_KEY_END           = 269;
        const int GLFW_KEY_CAPS_LOCK     = 280;
        const int GLFW_KEY_SCROLL_LOCK   = 281;
        const int GLFW_KEY_NUM_LOCK      = 282;
        const int GLFW_KEY_PRINT_SCREEN  = 283;
        const int GLFW_KEY_PAUSE         = 284;
        const int GLFW_KEY_F1            = 290;
        const int GLFW_KEY_F2            = 291;
        const int GLFW_KEY_F3            = 292;
        const int GLFW_KEY_F4            = 293;
        const int GLFW_KEY_F5            = 294;
        const int GLFW_KEY_F6            = 295;
        const int GLFW_KEY_F7            = 296;
        const int GLFW_KEY_F8            = 297;
        const int GLFW_KEY_F9            = 298;
        const int GLFW_KEY_F10           = 299;
        const int GLFW_KEY_F11           = 300;
        const int GLFW_KEY_F12           = 301;
        const int GLFW_KEY_F13           = 302;
        const int GLFW_KEY_F14           = 303;
        const int GLFW_KEY_F15           = 304;
        const int GLFW_KEY_F16           = 305;
        const int GLFW_KEY_F17           = 306;
        const int GLFW_KEY_F18           = 307;
        const int GLFW_KEY_F19           = 308;
        const int GLFW_KEY_F20           = 309;
        const int GLFW_KEY_F21           = 310;
        const int GLFW_KEY_F22           = 311;
        const int GLFW_KEY_F23           = 312;
        const int GLFW_KEY_F24           = 313;
        const int GLFW_KEY_F25           = 314;
        const int GLFW_KEY_KP_0          = 320;
        const int GLFW_KEY_KP_1          = 321;
        const int GLFW_KEY_KP_2          = 322;
        const int GLFW_KEY_KP_3          = 323;
        const int GLFW_KEY_KP_4          = 324;
        const int GLFW_KEY_KP_5          = 325;
        const int GLFW_KEY_KP_6          = 326;
        const int GLFW_KEY_KP_7          = 327;
        const int GLFW_KEY_KP_8          = 328;
        const int GLFW_KEY_KP_9          = 329;
        const int GLFW_KEY_KP_DECIMAL    = 330;
        const int GLFW_KEY_KP_DIVIDE     = 331;
        const int GLFW_KEY_KP_MULTIPLY   = 332;
        const int GLFW_KEY_KP_SUBTRACT   = 333;
        const int GLFW_KEY_KP_ADD        = 334;
        const int GLFW_KEY_KP_ENTER      = 335;
        const int GLFW_KEY_KP_EQUAL      = 336;
        const int GLFW_KEY_LEFT_SHIFT    = 340;
        const int GLFW_KEY_LEFT_CONTROL  = 341;
        const int GLFW_KEY_LEFT_ALT      = 342;
        const int GLFW_KEY_LEFT_SUPER    = 343;
        const int GLFW_KEY_RIGHT_SHIFT   = 344;
        const int GLFW_KEY_RIGHT_CONTROL = 345;
        const int GLFW_KEY_RIGHT_ALT     = 346;
        const int GLFW_KEY_RIGHT_SUPER   = 347;
        const int GLFW_KEY_MENU          = 348;
        const int GLFW_KEY_LAST          = GLFW_KEY_MENU;

        const int GLFW_MOUSE_BUTTON_1    = 0;
        const int GLFW_MOUSE_BUTTON_2    = 1;
        const int GLFW_MOUSE_BUTTON_LEFT = GLFW_MOUSE_BUTTON_1;
        const int GLFW_MOUSE_BUTTON_RIGHT = GLFW_MOUSE_BUTTON_2;

        GlobalsConfigFile globalConfig;
        

        // Game-related stuff
        Hashtable keys = new Hashtable();
        Vector4 rot = new Vector4();

        bool fpsMode = false;        
        int originX = 0;
        int originY = 0;        
        float sens = 1.0f;
        bool invertY = false;

        public InputManager()
        {
            // callback for keyboard; will trigger KeyboardInput() on every keypress or release
            Glfw.glfwSetKeyCallback(KeyboardInput);
            // callback for mouse; will trigger MousePos() on every mouse movement
            Glfw.glfwSetMousePosCallback(MousePos);
            // callback for mouse; will trigger MouseButton() on every mouse click/release
            Glfw.glfwSetMouseButtonCallback(MouseButton);

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
            // To-be-implemented; get invertY from config
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
        void TurnOnFPSMode()
        {
            fpsMode = true;
        }

        void TurnOffFPSMode()
        {
            fpsMode = false;
        }
    }
}
